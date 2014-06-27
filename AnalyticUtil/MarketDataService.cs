#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Third Party
using CalcFarm.AnalyticUtil.Entity;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
#endregion

namespace CalcFarm.AnalyticUtil
{
    public class MarketDataService
    {
        private IConnection Connection;
        private IModel Channel;

        public MarketDataService(string QueueUrl = "localhost" )
        {
            var Factory = new ConnectionFactory() { HostName = QueueUrl };

            Connection = Factory.CreateConnection();
            Channel = Connection.CreateModel();
            
            bool Durable = true;
            bool Exclusive = false;
            bool AutoDelete = false;
            Channel.QueueDeclare(Constants.MKTDATASRV_EQTYUPD_QUEUE, Durable, Exclusive, AutoDelete, null);

            return;
        }

        public void PublishPrice(Price px)
        {
            string RouteKey = px.InstIdentifier;
            var MessageBytes = MessageSerializer.Serialize(px); // Choose faster serializer here!

            var properties = Channel.CreateBasicProperties();
            properties.SetPersistent(true);
            Channel.BasicPublish("", Constants.MKTDATASRV_EQTYUPD_QUEUE, null, MessageBytes);
            return;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
