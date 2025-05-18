using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Outbox
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DaprClient _daprClient;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
        private const string PUBSUB_NAME = "pubsub";


        public OutboxProcessorService(IServiceProvider serviceProvider, DaprClient daprClient, ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }

                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();

            // Get a batch of pending messages
            var messages = await outboxService.GetPendingMessagesAsync(20, stoppingToken);
            
            if (messages.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Found {Count} outbox messages to process", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    // Deserialize the payload to dynamic object for Dapr publishing
                    var payload = JsonSerializer.Deserialize<object>(message.Payload, _jsonOptions);
                    
                    // Convert event type to topic name (kebab-case convention)
                    var topicName = message.EventType.Replace("Event", string.Empty)
                        .ToLowerInvariant()
                        .Replace("_", "-");
                    
                    // Publish the event through Dapr
                    await _daprClient.PublishEventAsync(
                        PUBSUB_NAME,
                        topicName,
                        payload,
                        stoppingToken);
                    
                    _logger.LogInformation("Published event {MessageId} of type {EventType} to topic {TopicName}", 
                        message.Id, message.EventType, topicName);
                    
                    // Mark as processed
                    await outboxService.MarkAsProcessedAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                    await outboxService.MarkAsFailedAsync(message, ex.Message, stoppingToken);
                }
            }
        }
    }
}
