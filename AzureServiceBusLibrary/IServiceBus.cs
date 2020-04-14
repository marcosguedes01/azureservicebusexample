using System;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public interface IServiceBus
    {
        Task SendQueueMessage(string messsage);
        void ReceiveQueueMessage(PrintMessage printMessage);
        void ReceiveQueueMessage(PrintMessage printMessage, PrintMessageException printMessageException);
        Task CloseConnectionAsync();
    }

    public delegate void PrintMessage(long sequenceNumber, string messageBody);
    public delegate void PrintMessageException(ServiceBusException messageException);

    public struct ServiceBusException
    {
        public Exception Exception { get; set; }
        public string ContextEndpoint { get; set; }
        public string ContextEntityPath { get; set; }
        public string ContextAction { get; set; }
    }
}
