using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;

namespace Consumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Consumer>();
            builder.Configuration.AddUserSecrets<Program>();

            var serviceBusConfigurationPath = builder.Configuration.GetSection("serviceBus");

            builder.Services.AddTransient((serviceProvider) =>
            {
                var sbcOptions = new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        Delay = TimeSpan.FromSeconds(10),
                        MaxDelay = TimeSpan.FromSeconds(30),
                        Mode = ServiceBusRetryMode.Exponential,
                        MaxRetries = 3,
                    },
                };

                var configuration = serviceProvider.GetService<IOptions<ConsumerConfiguration>>().Value;
                return new ServiceBusClient(configuration.Endpoint, new DefaultAzureCredential(), sbcOptions);
            });

            builder.Services.AddOptions<ConsumerConfiguration>().Bind(serviceBusConfigurationPath);

            var host = builder.Build();

            host.Run();
        }
    }
}