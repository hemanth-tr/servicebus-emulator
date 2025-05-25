using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text;

namespace Publisher
{
    public class Publisher
    {
        private readonly PublisherConfiguration _serviceBusConfiguration;
        private readonly ServiceBusClient _serviceBusClient;

        public Publisher(IOptions<PublisherConfiguration> publisherConfigurationOptions, ServiceBusClient serviceBusClient)
        {
            this._serviceBusConfiguration = publisherConfigurationOptions.Value;
            _serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(string deliveryType, string deliveryMessage)
        {
            try
            {
                // Create a ServiceBus client
                var dType = string.Equals(deliveryType, "topic", StringComparison.OrdinalIgnoreCase) ? _serviceBusConfiguration.Topic : _serviceBusConfiguration.Queue;
                var sender = _serviceBusClient.CreateSender(dType);

                // Create the message
                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(deliveryMessage));

                // Send the message
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Message sent to {deliveryType}! {_serviceBusConfiguration.Queue}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex}");
            }
        }
    }
}
