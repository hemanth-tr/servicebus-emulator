using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Publisher
{
    internal class Consumer
    {
        private readonly ConsumerConfiguration _appSettings;
        private string _connectionString;
        private string _topicName;
        private string _subscriptionName;

        public Consumer(IOptions<ConsumerConfiguration> options)
        {
            _appSettings = options.Value;
            Initialize();
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.StartListening().ConfigureAwait(false);
        }

        private async Task StartListening()
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
            var client = new ServiceBusClient(_connectionString);

            var serviceBusProcessOptions = new ServiceBusProcessorOptions();
            serviceBusProcessOptions.ReceiveMode = ServiceBusReceiveMode.PeekLock;
            var processor = client.CreateProcessor(_topicName, _subscriptionName, serviceBusProcessOptions);

            processor.ProcessMessageAsync += HandleMessageAsync;
            processor.ProcessErrorAsync += HandleErrorAsync;
            await processor.StartProcessingAsync();

            Console.WriteLine($"Listening for messages... on topic: {_topicName}, subscription: {_subscriptionName}");
            Console.WriteLine("Press any key to stop listening...");
            Console.ReadKey();

            await processor.StopProcessingAsync();

        }

        // Handles the message received from the subscription
        private async Task HandleMessageAsync(ProcessMessageEventArgs messageArgs)
        {
            var message = messageArgs.Message;
            var body = message.Body.ToString();

            Console.WriteLine($"Received message: {body}");

            // Complete the message to remove it from the subscription queue
            await messageArgs.CompleteMessageAsync(messageArgs.Message);
        }

        // Handles any errors that occur while processing messages
        private async Task HandleErrorAsync(ProcessErrorEventArgs errorArgs)
        {
            Console.WriteLine($"Error: {errorArgs.Exception.Message}");
        }

        private void Initialize()
        {
            _connectionString = _appSettings.ConnectionString;
            _topicName = _appSettings.TopicName;
            _subscriptionName = _appSettings.Subscription;
        }
    }
}
