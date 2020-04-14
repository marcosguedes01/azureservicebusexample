using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public sealed class ServiceBusQueue : IServiceBusQueue
    {
        public delegate void PrintMessage(long sequenceNumber, string messageBody);

        // Connection String for the namespace can be obtained from the Azure portal under the 
        // 'Shared Access policies' section.
        private string serviceBusConnectionString;
        private string queueName;
        private IQueueClient queueClient;
        private PrintMessage printMessage;

        public ServiceBusQueue(string serviceBusConnectionString, string queueName)
        {
            this.serviceBusConnectionString = serviceBusConnectionString;
            this.queueName = queueName;
        }

        #region Queue send
        public async Task Send()
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(serviceBusConnectionString, queueName);

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            await CloseConnectionAsync();
        }

        private async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Send the message to the queue
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        #endregion

        #region Queue receive
        public void Receive(PrintMessage printMessage)
        {
            this.printMessage = printMessage;

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
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
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
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
        #endregion

        public async Task CloseConnectionAsync()
        {
            await queueClient.CloseAsync();
        }
    }

    public interface IServiceBusQueue
    {
        Task Send();
        void Receive(ServiceBusQueue.PrintMessage printMessage);
        Task CloseConnectionAsync();
    }
}
