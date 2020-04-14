using AzureServiceBusLibrary;
using System.Threading.Tasks;
using Xunit;

namespace AzureServiceBusTests
{
    public class ServiceBusQueueTest
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";
        
        private IServiceBus serviceBus;
        private bool connectionOpened;
        private long sequenceNumber;
        private string messageBody;

        [Fact]
        public async Task SendTestAsync()
        {
            serviceBus = new ServiceBusQueue(ServiceBusConnectionString, QueueName);

            for (int i = 1; i <= 10; i++)
            {
                var message = $"Message {i}";
                await serviceBus.SendMessage(message);
            }

            await serviceBus.CloseConnectionAsync();
        }

        [Fact]
        public void ReceiveTestAsync()
        {
            connectionOpened = true;

            serviceBus = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            serviceBus.ReceiveMessage(printMessage);

            while (connectionOpened) { }

            serviceBus.CloseConnectionAsync().GetAwaiter();

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
