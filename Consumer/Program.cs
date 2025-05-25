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

            builder.Services.AddOptions<ConsumerConfiguration>().Bind(serviceBusConfigurationPath);

            var host = builder.Build();

            host.Run();
        }
    }
}