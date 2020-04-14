using AzureServiceBusLibrary;
using System.Threading.Tasks;
using Xunit;

namespace AzureServiceBusTests
{
    public class AzureServiceBusLibraryTest
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";

        private IServiceBus serviceBusQueue;
        private bool connectionOpened;
        private long sequenceNumber;
        private string messageBody;

        [Fact]
        public async Task QueueSendTestAsync()
        {
            serviceBusQueue = new ServiceBusQueue(ServiceBusConnectionString, QueueName);

            for (int i = 1; i <= 10; i++)
            {
                var message = $"Message {i}";
                await serviceBusQueue.SendQueueMessage(message);
            }

            await serviceBusQueue.CloseConnectionAsync();
        }

        [Fact]
        public void QueueReceiveTestAsync()
        {
            connectionOpened = true;

            serviceBusQueue = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            serviceBusQueue.ReceiveQueueMessage(printMessage);

            while (connectionOpened) { }

            serviceBusQueue.CloseConnectionAsync().GetAwaiter();

            Assert.True(sequenceNumber > 0);
            Assert.True(!string.IsNullOrEmpty(messageBody));
        }

        private void printMessage(long sequenceNumber, string messageBody)
        {
            this.sequenceNumber = sequenceNumber;
            this.messageBody = messageBody;

            connectionOpened = false;
        }
    }
}
