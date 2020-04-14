using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public sealed class ServiceBusQueue : ServiceBusBase
    {
        public int MaxConcurrentCalls { get; set; } = 1;
        public bool AutoComplete { get; set; } = false;

        public ServiceBusQueue(string serviceBusConnectionString, string queueName)
            : base(serviceBusConnectionString, queueName) { }

        public override async Task SendQueueMessage(string message)
        {
            queueClient = new QueueClient(serviceBusConnectionString, queueName);

            try
            {
                var msg = new Message(Encoding.UTF8.GetBytes(message));

                // Send the message to the queue
                await queueClient.SendAsync(msg);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        #region Queue receive
        public override void ReceiveQueueMessage(PrintMessage printMessage)
        {
            ReceiveQueueMessage(printMessage, null);
        }

        public override void ReceiveQueueMessage(PrintMessage printMessage, PrintMessageException printMessageException)
        {
            this.printMessage = printMessage;
            this.printMessageException = printMessageException;

            queueClient = new QueueClient(serviceBusConnectionString, queueName);

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = MaxConcurrentCalls,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = AutoComplete
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var sequenceNumber = message.SystemProperties.SequenceNumber;
            var messageBody = Encoding.UTF8.GetString(message.Body);

            printMessage(sequenceNumber, messageBody);

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            if (printMessageException != null)
            {
                var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

                printMessageException(new ServiceBusException
                {
                    Exception = exceptionReceivedEventArgs.Exception,
                    ContextEndpoint = context.Endpoint,
                    ContextEntityPath = context.EntityPath,
                    ContextAction = context.Action
                });
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
