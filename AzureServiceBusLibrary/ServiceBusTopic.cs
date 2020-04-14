using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public sealed class ServiceBusTopic : ServiceBusBase
    {
        public ServiceBusTopic(string serviceBusConnectionString, string queueName)
            : base(serviceBusConnectionString, queueName) { }

        public override void ReceiveQueueMessage(PrintMessage printMessage)
        {
            throw new NotImplementedException();
        }

        public override Task SendQueueMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}
