using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text;

namespace Publisher
{
    public class Publisher
    {
        private readonly PublisherConfiguration _serviceBusConfiguration;

        public Publisher(IOptions<PublisherConfiguration> publisherConfigurationOptions)
        {
            this._serviceBusConfiguration = publisherConfigurationOptions.Value;
        }

        public async Task SendMessageAsync(string messageBody)
        {
            try
            {
                var sbcOptions = new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpTcp,
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        Delay = TimeSpan.FromSeconds(10),
                        MaxDelay = TimeSpan.FromSeconds(30),
                        Mode = ServiceBusRetryMode.Exponential,
                        MaxRetries = 3,
                    },
                };

                // Create a ServiceBus client
                var client = new ServiceBusClient(_serviceBusConfiguration.ConnectionString, sbcOptions);
                var sender = client.CreateSender(_serviceBusConfiguration.TopicName);

                // Create the message
                Console.WriteLine("Preparing the message");
                var user = new User { Guid = Guid.NewGuid(), Name = "Hemanth kumar new" };
                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(user.ToString()));

                // Send the message
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Message sent to topic! {_serviceBusConfiguration.TopicName}");

                // await this.StartListeningAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex}");
            }
        }
    }
}
