using AzureServiceBusLibrary;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureServiceBusTests
{
    public class AzureServiceBusLibraryTest
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";

        private IServiceBusQueue serviceBusQueue;
        private bool connectionOpened;
        private long sequenceNumber;
        private string messageBody;

        [Fact]
        public async Task QueueSendTestAsync()
        {
            serviceBusQueue = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            await serviceBusQueue.Send();
        }

        [Fact]
        public void QueueReceiveTestAsync()
        {
            connectionOpened = true;

            serviceBusQueue = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            serviceBusQueue.Receive(printMessage);

            while (connectionOpened) { }

            Assert.True(sequenceNumber > 0);
            Assert.True(!string.IsNullOrEmpty(messageBody));

            serviceBusQueue.CloseConnectionAsync().GetAwaiter();
        }

        private void printMessage(long sequenceNumber, string messageBody)
        {
            this.sequenceNumber = sequenceNumber;
            this.messageBody = messageBody;

            connectionOpened = false;
        }
    }
}
