using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Publisher
{
    public class Startup
    {
        public static async Task Initialze(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Console.WriteLine("Building Configurations");
            var configuration = builder.Build();

            Console.WriteLine("Configuring services");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Console.WriteLine("running main");
            await serviceProvider?.GetService<Publisher>().SendMessageAsync(null);
            await serviceProvider?.GetService<Consumer>().ExecuteAsync(CancellationToken.None);
        }

        private static void ConfigureServices(ServiceCollection serviceCollection, IConfiguration configuration)
        {
            // AppSettings IOptions configuration
            serviceCollection.AddOptions<PublisherConfiguration>().Bind(configuration.GetSection("serviceBusTopic"));
            serviceCollection.AddOptions<ConsumerConfiguration>().Bind(configuration.GetSection("serviceBusTopic"));

            serviceCollection.AddTransient<Publisher>();
            serviceCollection.AddTransient<Consumer>();
        }
    }
}
