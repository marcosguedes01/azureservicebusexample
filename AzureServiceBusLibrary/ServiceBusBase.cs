using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;

namespace AzureServiceBusLibrary
{
    public abstract class ServiceBusBase : IServiceBus
    {
        public int MaxConcurrentCalls { get; set; } = 1;
        public bool AutoComplete { get; set; } = false;

        // Connection String for the namespace can be obtained from the Azure portal under the 
        // 'Shared Access policies' section.
        protected readonly string serviceBusConnectionString;
        
        protected PrintMessage printMessage;
        protected PrintMessageException printMessageException;

        public ServiceBusBase(string serviceBusConnectionString)
        {
            this.serviceBusConnectionString = serviceBusConnectionString;
        }

        public abstract Task SendMessage(string messsage);

        public abstract void ReceiveMessage(PrintMessage printMessage);
        public virtual void ReceiveMessage(PrintMessage printMessage, PrintMessageException printMessageException)
        {
            throw new System.NotImplementedException();
        }

        public abstract Task CloseConnectionAsync();

        protected virtual Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
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
    }
}
