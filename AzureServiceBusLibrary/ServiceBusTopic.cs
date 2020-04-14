using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions
    /// </summary>
    public sealed class ServiceBusTopic : ServiceBusBase
    {
        private readonly string topicName;
        private readonly string subscriptionName;

        private ITopicClient topicClient;
        private ISubscriptionClient subscriptionClient;

        public ServiceBusTopic(string serviceBusConnectionString, string topicName, string subscriptionName)
            : base(serviceBusConnectionString)
        {
            this.topicName = topicName;
            this.subscriptionName = subscriptionName;
        }

        public override async Task SendMessage(string message)
        {
            try
            {
                topicClient = new TopicClient(serviceBusConnectionString, topicName);

                var msg = new Message(Encoding.UTF8.GetBytes(message));

                msg.To = subscriptionName;

                // Send the message to the topic
                await topicClient.SendAsync(msg);

            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public override void ReceiveMessage(PrintMessage printMessage)
        {
            ReceiveMessage(printMessage, null);
        }

        public override void ReceiveMessage(PrintMessage printMessage, PrintMessageException printMessageException)
        {
            this.printMessage = printMessage;
            this.printMessageException = printMessageException;

            subscriptionClient = new SubscriptionClient(serviceBusConnectionString, topicName, subscriptionName);
            
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = MaxConcurrentCalls,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = AutoComplete
            };

            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var sequenceNumber = message.SystemProperties.SequenceNumber;
            var messageBody = Encoding.UTF8.GetString(message.Body);

            printMessage(sequenceNumber, messageBody);

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        public override async Task CloseConnectionAsync()
        {
            if (subscriptionClient != null) await subscriptionClient.CloseAsync();
            if (topicClient != null) await topicClient.CloseAsync();
        }
    }
}
