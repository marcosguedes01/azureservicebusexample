using AzureServiceBusLibrary;
using System.Threading.Tasks;
using Xunit;

namespace AzureServiceBusTests
{
    public class ServiceBusTopicTest
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";
        private const string SubscriptionName = "{SUBSCRIPTION_NAME}";

        private IServiceBus serviceBusQueue;
        private bool connectionOpened;
        private long sequenceNumber;
        private string messageBody;

        [Fact]
        public async Task SendTestAsync()
        {
            serviceBusQueue = new ServiceBusTopic(ServiceBusConnectionString, QueueName, SubscriptionName);

            for (int i = 1; i <= 10; i++)
            {
                var message = $"Topic Message {i}";
                await serviceBusQueue.SendMessage(message);
            }

            await serviceBusQueue.CloseConnectionAsync();
        }

        [Fact]
        public void ReceiveTestAsync()
        {
            connectionOpened = true;

            serviceBusQueue = new ServiceBusTopic(ServiceBusConnectionString, QueueName, SubscriptionName);
            serviceBusQueue.ReceiveMessage(printMessage);

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
