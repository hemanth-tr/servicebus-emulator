using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Consumer
{
    public class Consumer : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ConsumerConfiguration _appSettings;
        private string _connectionString;
        private string _topicName;
        private string _queue;
        private string _subscriptionName;

        private ServiceBusProcessor _topicProcessor;
        private ServiceBusProcessor _topicProcessor2;
        private ServiceBusProcessor _queueProcessor;

        public Consumer(IOptions<ConsumerConfiguration> options, ServiceBusClient client)
        {
            _appSettings = options.Value;
            _client = client;
            Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.StartListeningToTopics().ConfigureAwait(false);
            await this.StartListeningToTopics2().ConfigureAwait(false);
            await this.StartListeningToQueues().ConfigureAwait(false);

            Console.WriteLine($"Listening for messages... on: {_queue}");
            Console.WriteLine("Press any key to stop listening...");

            Console.ReadKey();
            await _topicProcessor.StopProcessorAsync();
            await _topicProcessor2.StopProcessorAsync();
            await _queueProcessor.StopProcessorAsync();
        }

        private async Task StartListeningToQueues()
        {
            if (string.IsNullOrWhiteSpace(this._queue))
            {
                Console.WriteLine("queue item is empty. hence not listening to queue.");
                return;
            }

            _queueProcessor = _client.CreateProcessor(_queue);

            _queueProcessor.ProcessMessageAsync += HandleMessageAsync;
            _queueProcessor.ProcessErrorAsync += HandleErrorAsync;
            await _queueProcessor.StartProcessingAsync();
        }

        private async Task StartListeningToTopics()
        {
            if (string.IsNullOrWhiteSpace(this._topicName) || string.IsNullOrWhiteSpace(this._subscriptionName))
            {
                Console.WriteLine("topic/subscription item is empty. hence not listening to topic.");
                return;
            }

            var serviceBusProcessOptions = new ServiceBusProcessorOptions();
            serviceBusProcessOptions.ReceiveMode = ServiceBusReceiveMode.PeekLock;
            _topicProcessor = _client.CreateProcessor(_topicName, _subscriptionName);

            _topicProcessor.ProcessMessageAsync += HandleMessageAsync;
            _topicProcessor.ProcessErrorAsync += HandleErrorAsync;
            await _topicProcessor.StartProcessingAsync();
        }

        private async Task StartListeningToTopics2()
        {
            if (string.IsNullOrWhiteSpace(this._topicName) || string.IsNullOrWhiteSpace(this._subscriptionName))
            {
                Console.WriteLine("topic/subscription item is empty. hence not listening to topic.");
                return;
            }

            var serviceBusProcessOptions = new ServiceBusProcessorOptions();
            serviceBusProcessOptions.ReceiveMode = ServiceBusReceiveMode.PeekLock;
            _topicProcessor2 = _client.CreateProcessor(_topicName, "sub-2");

            _topicProcessor2.ProcessMessageAsync += HandleMessageAsync2;
            _topicProcessor2.ProcessErrorAsync += HandleErrorAsync;
            await _topicProcessor2.StartProcessingAsync();
        }

        // Handles the message received from the subscription
        private async Task HandleMessageAsync(ProcessMessageEventArgs messageArgs)
        {
            var message = messageArgs.Message;
            var body = message.Body.ToString();

            Console.WriteLine($"Received message HANDLER1: {body}");

            // Complete the message to remove it from the subscription queue
            await messageArgs.CompleteMessageAsync(messageArgs.Message);
        }

        // Handles the message received from the subscription
        private async Task HandleMessageAsync2(ProcessMessageEventArgs messageArgs)
        {
            var message = messageArgs.Message;
            var body = message.Body.ToString();

            Console.WriteLine($"Received message HANDLER2: {body}");

            // Complete the message to remove it from the subscription queue
            await messageArgs.CompleteMessageAsync(messageArgs.Message);
        }

        // Handles any errors that occur while processing messages
        private async Task HandleErrorAsync(ProcessErrorEventArgs errorArgs)
        {
            Console.WriteLine($"Error: {errorArgs.Exception.Message}");
            await Task.CompletedTask;
        }

        private void Initialize()
        {
            _connectionString = _appSettings.ConnectionString;
            _topicName = _appSettings.Topic;
            _queue = _appSettings.Queue;
            _subscriptionName = _appSettings.Subscription;
        }
    }
}
