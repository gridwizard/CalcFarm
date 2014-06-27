#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

#region Third Party
using CalcFarm.AnalyticUtil.Entity;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
#endregion

namespace CalcFarm.AnalyticUtil
{
    public class CalcService : ICalcService, IPriceListener, ICalcResultPublisher
    {
        private ApplicationController AppCtrl { get; set; }

        private static System.Threading.TimerCallback cb;
        private static System.Threading.Timer tmr;
        private static int CountPxUpdateRecv { get; set; }
        private static int CountCalcCompleted { get; set; }
        private static int Gap { get; set; }

        private static void ReportStatistics(object e)
        {
            ApplicationController AppCtrl = e as ApplicationController;
            StringBuilder buffer = new StringBuilder();
            buffer.Append("CountPxUpdateRecv: ").Append(CountPxUpdateRecv / 1000).Append("k, CountCalcCompleted: ").Append(CountCalcCompleted / 1000).Append("k").Append(", Gap: ").Append(Gap/1000).Append("k");
            AppCtrl.Info(buffer.ToString());

            // Write dump file
            System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "CalcServerStatistics.log", Environment.NewLine);
            System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "CalcServerStatistics.log", buffer.ToString());

            return;
        }
        
        public CalcService(ApplicationController AppController, string QueueUrl = "localhost", bool DetailLog = false)
        {
            AppCtrl = AppController;
            Init(QueueUrl, DetailLog);
            return;
        }

        public void Init(string QueueUrl, bool DetailLog)
        {
            InitAnalyticMsgQueue(QueueUrl);
            StartListenForEqtyUpd(QueueUrl, DetailLog);
            
            return;
        }

        #region Message bus
        private IConnection MktDataConnection;
        private IModel MktDataChannel;
        private QueueingBasicConsumer MktDataQueueListener;

        private IConnection AnalyticConnection;
        private IModel AnalyticChannel;

        public void StartListenForEqtyUpd(string QueueUrl, bool DetailLog)
        {
            BasicDeliverEventArgs MQMsg;
            StringBuilder buffer = new StringBuilder();
            var Factory = new ConnectionFactory() { HostName = QueueUrl };

            try
            {
                MktDataConnection = Factory.CreateConnection();
                MktDataChannel = MktDataConnection.CreateModel();
                // MktDataChannel.BasicQos(0, 1, false);

                bool Durable = true;
                bool Exclusive = false;
                bool AutoDelete = false;
                MktDataChannel.QueueDeclare(Constants.MKTDATASRV_EQTYUPD_QUEUE, Durable, Exclusive, AutoDelete, null);
                MktDataQueueListener = new QueueingBasicConsumer(MktDataChannel);
                /* i.e. noAck = true means Autoacknowledgement that message processed successfully, and message can be deleted from queue. 
                 * http://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
                 */
                bool noAck = true;
                MktDataChannel.BasicConsume(Constants.MKTDATASRV_EQTYUPD_QUEUE, noAck, MktDataQueueListener);

                cb = new System.Threading.TimerCallback(ReportStatistics);
                tmr = new Timer(cb, AppCtrl, 0, 1000);
                CountPxUpdateRecv = 0;
                CountCalcCompleted = 0;
                while (true)
                {
                    try
                    {
                        /* 
                        * On this dev box with Intel 2.6GHz (Single Processor) with 4GB RAM, PriceServer publishes about 3,000 price updates per sec.
                        * Note that threads running on same machine shares the same set of resources (CPU, memory...etc). 
                        * As such, max parallelism is achieved by running multiple instances of CalcServer on multiple machines.
                        * 
                        * Now, PriceServer is publishing at 3k/sec - it does only one thing: Publish to message bus. Here, however, three actions are performed.
                        * (a) Dequeue from Message Bus (Should not take longer than PriceServer publishing it)
                        * (b) QuantLib.handlePrice (Fast calculation only! For example, update pnl for a few positions, NOT thousands. Or run a tree based model to price an option)
                        * (c) publish back list of CalcResult (# of CalcResult in the list and size of each critical!)
                        *
                        * Because PriceServer is publishing at rate 1000ms/3000ticks = 0.33 ms, if (a)-(c) takes more than 0.33ms each, CalcServer will lag behind PriceServer.
                        * So, every additional 0.33ms along execution path (a)-(c) you need add one CalcServer, or additional physical CPU/Processor.
                        * If you add 1ms, you probably need additional 3 CPU to distribute the load.
                        * If your calculation takes 100ms to complete, that's additional 300 CPU. 
                        * On Dual Quad Core machines (8 CPU/processors each), that's 37.5 physical servers.
                        *
                        * That's not too bad. Facebook Hadoop implementation has 1100 machine cluster: http://wiki.apache.org/hadoop/PoweredBy#F
                        */
                        MQMsg = (BasicDeliverEventArgs)MktDataQueueListener.Queue.Dequeue();
                        CountPxUpdateRecv++;

                        if (!noAck) // If noAck==false, don't need send ack for message to get auto deleted.
                        {
                            MktDataChannel.BasicAck(MQMsg.DeliveryTag, false);
                        }

                        
                        ThreadPool.QueueUserWorkItem(
                                x =>
                                {
                                    try
                                    {
                                        EqtyUpdMessageHandler(MQMsg, DetailLog);

                                        CountCalcCompleted++;
                                    }
                                    catch (Exception SingleProcessingEx)
                                    {
                                        // Calculation/Processing exceptions
                                        AppCtrl.Warn(ExceptionUtil.ExceptionToString(SingleProcessingEx));
                                    }

                                    // Update statistics
                                    Gap = CountPxUpdateRecv - CountCalcCompleted;

                                    #region Increase thread pool size?
                                    if (Gap > 1000)
                                    {
                                        if ((AppCtrl.ThreadPoolSize + 2) < AppCtrl.MaxThreadPoolSize)
                                        {
                                            AppCtrl.ThreadPoolSize += 2;
                                            ThreadPool.SetMaxThreads(AppCtrl.ThreadPoolSize, AppCtrl.ThreadPoolSize);
                                            AppCtrl.Info("ThreadPoolSize adjusted: " + AppCtrl.ThreadPoolSize);
                                        }
                                    }
                                    #endregion
                                }
                                );
                    }
                    catch (Exception SingleMsgBusEx)
                    {
                        // Error dequeuing from message bus
                        AppCtrl.Warn(ExceptionUtil.ExceptionToString(SingleMsgBusEx));
                    }
                }
            }
            catch (Exception Ex)
            {
                AppCtrl.Fatal(ExceptionUtil.ExceptionToString(Ex));
            }

            return;
        }

        public void InitAnalyticMsgQueue(string QueueUrl)
        {
            var Factory = new ConnectionFactory() { HostName = QueueUrl };

            AnalyticConnection = Factory.CreateConnection();
            AnalyticChannel = AnalyticConnection.CreateModel();

            bool Durable = true;
            bool Exclusive = false;
            bool AutoDelete = false;
            AnalyticChannel.QueueDeclare(Constants.ANALYTICUPD_QUEUE, Durable, Exclusive, AutoDelete, null);
            return;
        }

        protected void EqtyUpdMessageHandler(BasicDeliverEventArgs MQMsg, bool DetailLog = false)
        {
            StringBuilder buffer = new StringBuilder();
            byte[] btMessage = MQMsg.Body;
            Price px = (Price) MessageSerializer.Deserialize(btMessage);

            handlePrice(px.InstIdentifier, px);

            if (DetailLog)
            {
                buffer.Append("EqtyUpdMessageHandler received px update for ").Append(px.ToString());
                AppCtrl.Info(buffer.ToString());
            }

            return;
        }
        #endregion

        #region IPriceListener implementation
        public void handlePrice(String instrumentId, Price price)
        {
            IList<CalcResult> Results = null;
            StringBuilder buffer = null;

            try
            {
                Results = QuantLib.fastCalcOnly(instrumentId, price); // Dummy function ...
                if (Results != null && Results.Count > 0)
                {
                    publish(Results);
                }
            }
            catch (Exception CalcEx)
            {
                buffer = new StringBuilder();
                buffer.Append("Error while processing update ").Append(price.InstIdentifier);
                buffer.Append(", Details: ").Append(ExceptionUtil.ExceptionToString(CalcEx));
                AppCtrl.Warn(buffer.ToString());

                buffer.Clear();
                buffer = null;
            }
            finally
            {
                if (Results != null)
                {
                    Results.Clear();
                    Results = null;
                }
            }
            return;
        }
        #endregion

        #region ICalcService implementation
        public IList<CalcResult> calculate(String instrumentId, Price price)
        {
            IList<CalcResult> Result = null;


            return Result;
        }
        #endregion

        #region ICalcResultPublisher implementation
        public void publish(IList<CalcResult> calcResults)
        {
            if (calcResults != null && calcResults.Count > 0)
            {
                foreach (CalcResult calcResult in calcResults)
                {
                    publishSingleResult(calcResult);
                }
            }
            return;
        }

        public void publishSingleResult(CalcResult result)
        {
            StringBuilder buffer = null;
            string RouteKey = result.Px.InstIdentifier;

            try
            {
                var MessageBytes = MessageSerializer.Serialize(result); // Choose faster serializer here!

                var properties = AnalyticChannel.CreateBasicProperties();
                properties.SetPersistent(true);
                AnalyticChannel.BasicPublish("", Constants.ANALYTICUPD_QUEUE, null, MessageBytes);

                if (AppCtrl.DetailLog)
                {
                    buffer = new StringBuilder();
                    buffer.Append("Published analytic update for ").Append(result.Px.InstIdentifier);
                    AppCtrl.Info(buffer.ToString());
                    buffer.Clear();
                    buffer = null;
                }
            }
            catch (Exception PublishEx)
            {
                buffer = new StringBuilder();
                buffer.Append("Error while publishing analytic ").Append(result.Px.InstIdentifier);
                buffer.Append(", Details: ").Append(ExceptionUtil.ExceptionToString(PublishEx));
                AppCtrl.Warn(buffer.ToString());

                buffer.Clear();
                buffer = null;
            }
            return;
        }
        #endregion
    }
}
