using AzureServiceBusLibrary;
using System;

namespace AzureServiceBusConsole
{
    class Program
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";

        private static ServiceBusQueue helloServiceBus;

        static void Main()
        {
            helloServiceBus = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            helloServiceBus.ReceiveQueueMessage(printMessage, printMessageException);

            Console.ReadKey();

            helloServiceBus.CloseConnectionAsync().GetAwaiter();
        }

        private static void printMessage(long sequenceNumber, string messageBody)
        {
            Console.WriteLine($"Received message: SequenceNumber:{sequenceNumber} Body:{messageBody}");
        }

        private static void printMessageException(ServiceBusException messageException)
        {
            Console.WriteLine($"Message handler encountered an exception {messageException.Exception}.");
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {messageException.ContextEndpoint}");
            Console.WriteLine($"- Entity Path: {messageException.ContextEntityPath}");
            Console.WriteLine($"- Executing Action: {messageException.ContextAction}");
        }
    }
}
