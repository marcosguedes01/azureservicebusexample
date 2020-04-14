using AzureServiceBusLibrary;
using System;

namespace AzureServiceBusConsole
{
    class Program
    {
        private const string ServiceBusConnectionString = "{AZURE_SERVICE_BUS_CONNECTION_STRING}";
        private const string QueueName = "{QUEUE_NAME}";

        private static ServiceBusQueue helloServiceBus;

        static void Main(string[] args)
        {
            helloServiceBus = new ServiceBusQueue(ServiceBusConnectionString, QueueName);
            helloServiceBus.Receive(printMessage);

            Console.WriteLine("Press a key to finish...");
            Console.ReadKey();

            helloServiceBus.CloseConnectionAsync().GetAwaiter();
        }

        private static void printMessage(long sequenceNumber, string messageBody)
        {
            Console.WriteLine($"Received message: SequenceNumber:{sequenceNumber} Body:{messageBody}");
        }
    }
}
