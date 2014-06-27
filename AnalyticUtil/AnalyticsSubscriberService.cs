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
    public class AnalyticsSubscriberService
    {
        private ApplicationController AppCtrl { get; set; }

        private static System.Threading.TimerCallback cb;
        private static System.Threading.Timer tmr;
        private static int CountAnalyticUpdateRecv { get; set; }

        private IConnection AnalyticConnection;
        private IModel AnalyticChannel;
        private QueueingBasicConsumer AnalyticQueueListener;

        private static void ReportStatistics(object e)
        {
            ApplicationController AppCtrl = e as ApplicationController;
            StringBuilder buffer = new StringBuilder();
            buffer.Append("CountAnalyticUpdateRecv: ").Append(CountAnalyticUpdateRecv / 1000).Append("k");
            AppCtrl.Info(buffer.ToString());
            return;
        }

        public AnalyticsSubscriberService(ApplicationController AppController, string QueueUrl = "localhost", bool DetailLog = false)
        {
            AppCtrl = AppController;
            Init(QueueUrl, DetailLog);
            return;
        }

        public void Init(string QueueUrl, bool DetailLog)
        {
            StartListenForAnalyticUpd(QueueUrl, DetailLog);
            
            return;
        }

        public void StartListenForAnalyticUpd(string QueueUrl, bool DetailLog)
        {
            BasicDeliverEventArgs MQMsg;
            StringBuilder buffer = new StringBuilder();
            var Factory = new ConnectionFactory() { HostName = QueueUrl };

            try
            {
                AnalyticConnection = Factory.CreateConnection();
                AnalyticChannel = AnalyticConnection.CreateModel();
                // AnalyticChannel.BasicQos(0, 1, false); --> We're not removing from queue either thru noAck=true, or manually sending back an acknowledge.

                bool Durable = true;
                bool Exclusive = false;
                bool AutoDelete = false;
                AnalyticChannel.QueueDeclare(Constants.ANALYTICUPD_QUEUE, Durable, Exclusive, AutoDelete, null);
                AnalyticQueueListener = new QueueingBasicConsumer(AnalyticChannel);
                /* i.e. noAck = true means Autoacknowledgement that message processed successfully, and message can be deleted from queue. 
                 * http://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
                 */
                bool noAck = false;
                AnalyticChannel.BasicConsume(Constants.ANALYTICUPD_QUEUE, noAck, AnalyticQueueListener);

                cb = new System.Threading.TimerCallback(ReportStatistics);
                tmr = new Timer(cb, AppCtrl, 0, 1000 * 10);

                CountAnalyticUpdateRecv = 0;
                while (true)
                {
                    try
                    {
                        MQMsg = (BasicDeliverEventArgs)AnalyticQueueListener.Queue.Dequeue();
                        CountAnalyticUpdateRecv++;

                        AnalyticUpdMessageHandler(MQMsg, DetailLog);
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

        protected void AnalyticUpdMessageHandler(BasicDeliverEventArgs MQMsg, bool DetailLog = false)
        {
            StringBuilder buffer = new StringBuilder();
            byte[] btMessage = MQMsg.Body;
            object Result= MessageSerializer.Deserialize(btMessage);
            CalcResult cResult = Result as CalcResult;

            if (DetailLog)
            {
                buffer.Append("AnalyticUpdMessageHandler received analytic update: ").Append(cResult.ToString());
                AppCtrl.Info(buffer.ToString());
            }

            return;
        }
    }
}
