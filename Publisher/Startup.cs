using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Publisher
{
    public class Startup
    {
        public static async Task Initialze(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            builder.AddUserSecrets<Program>();

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var (deliveryType, message) = DisplayPrompt();
            await serviceProvider?.GetService<Publisher>().SendMessageAsync(deliveryType, message);
        }

        private static void ConfigureServices(ServiceCollection serviceCollection, IConfiguration configuration)
        {
            // AppSettings IOptions configuration
            serviceCollection.AddOptions<PublisherConfiguration>().Bind(configuration.GetSection("serviceBus"));

            serviceCollection.AddTransient((serviceProvider) =>
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

                var configuration = serviceProvider.GetService<IOptions<PublisherConfiguration>>().Value;
                return new ServiceBusClient(configuration.Endpoint, new DefaultAzureCredential(), sbcOptions);
            });
            serviceCollection.AddTransient<Publisher>();
        }

        private static (string deliveryType, string message) DisplayPrompt()
        {
            Console.WriteLine("Delivery type?");
            Console.Write("1.Topic\n2.Queue\n");
            var deliveryType = Console.ReadLine();
            Console.Write("Enter you short message: ");
            var message = Console.ReadLine();
            return (deliveryType, message);
        }
    }
}
