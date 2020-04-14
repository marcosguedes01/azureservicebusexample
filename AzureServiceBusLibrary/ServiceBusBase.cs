using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public abstract class ServiceBusBase : IServiceBus
    {
        // Connection String for the namespace can be obtained from the Azure portal under the 
        // 'Shared Access policies' section.
        protected readonly string serviceBusConnectionString;
        protected readonly string queueName;

        protected PrintMessage printMessage;
        protected PrintMessageException printMessageException;

        protected IQueueClient queueClient;

        public ServiceBusBase(string serviceBusConnectionString, string queueName)
        {
            this.serviceBusConnectionString = serviceBusConnectionString;
            this.queueName = queueName;
        }

        public abstract Task SendQueueMessage(string messsage);

        public abstract void ReceiveQueueMessage(PrintMessage printMessage);
        public virtual void ReceiveQueueMessage(PrintMessage printMessage, PrintMessageException printMessageException)
        {
            throw new System.NotImplementedException();
        }

        public virtual async Task CloseConnectionAsync()
        {
            await queueClient.CloseAsync();
        }
    }
}
