using Azure.Messaging.ServiceBus;

namespace Consumer
{
    internal static class ServiceBusExtensions
    {
        public static async Task StopProcessorAsync(this ServiceBusProcessor processor)
        {
            if (processor == null)
            {
                return;
            }

            await processor.StopProcessingAsync();
        }
    }
}
