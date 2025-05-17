using System.Text;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;
using System.Threading;

namespace Order.Infrastructure.Events;

public class EventStoreSubscriber : BackgroundService
{
    private readonly IEventSerializer _eventSerializer;
    private readonly EventStoreClient _eventStoreClient;
    private readonly ILogger<EventStoreSubscriber> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICheckpointStore _checkpointStore;
    private StreamSubscription _subscription;
    private int _reconnectAttempts = 0;
    private const int MaxReconnectAttempts = 10;

    public EventStoreSubscriber(
        ILogger<EventStoreSubscriber> logger,
        IServiceScopeFactory serviceScopeFactory,
        EventStoreClient eventStoreClient,
        IEventSerializer eventSerializer,
        ICheckpointStore checkpointStore)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _eventStoreClient = eventStoreClient;
        _eventSerializer = eventSerializer;
        _checkpointStore = checkpointStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventStoreSubscriber starting at {time}", DateTimeOffset.Now);

        // Reset reconnect attempts counter
        _reconnectAttempts = 0;
        
        // Try to establish subscription
        await EstablishSubscriptionAsync(stoppingToken);

        // Keep the service alive until cancellation is requested
        try
        {
            await Task.Delay(System.Threading.Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
            _logger.LogInformation("EventStoreSubscriber shutting down");
        }
    }

    private async Task EstablishSubscriptionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get last checkpoint position
            var position = await _checkpointStore.GetCheckpoint();
            var fromAll = position != null ? FromAll.After(position.Value) : FromAll.Start;

            _logger.LogInformation("Subscribing from position: {Position}", 
                position != null ? position.ToString() : "Start");

            // Subscribe
            _subscription = await _eventStoreClient.SubscribeToAllAsync(
                fromAll,
                EventAppeared,
                subscriptionDropped: SubscriptionDropped,
                filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Subscription established");
            _reconnectAttempts = 0; // Reset counter on successful connection
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error establishing EventStore subscription");
            await TryReconnectAsync(cancellationToken);
        }
    }

    private async Task EventAppeared(
        StreamSubscription subscription, 
        ResolvedEvent resolvedEvent, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Skip system events (though we're already filtering them)
            if (resolvedEvent.Event.EventType.StartsWith("$"))
            {
                return;
            }

            _logger.LogDebug("Processing event {EventType}: {EventId}",
                resolvedEvent.Event.EventType, resolvedEvent.Event.EventId);

            // Deserialize event
            var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.ToArray());
            var domainEvent = _eventSerializer.Deserialize(eventData, resolvedEvent.Event.EventType);

            if (domainEvent == null)
            {
                _logger.LogWarning("Could not deserialize event {EventType}",
                    resolvedEvent.Event.EventType);
                return;
            }

            // Process the event by calling the projector
            using var scope = _serviceScopeFactory.CreateScope();
            var projector = scope.ServiceProvider.GetRequiredService<OrderEventProjector>();
            await projector.ProjectAsync(domainEvent);

            // Save checkpoint after successful projection
            if (resolvedEvent.OriginalPosition.HasValue)
            {
                await _checkpointStore.StoreCheckpoint(resolvedEvent.OriginalPosition.Value);
            }

            _logger.LogInformation("Projected event {EventType}", resolvedEvent.Event.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event {EventType}", resolvedEvent.Event.EventType);
        }
    }

    private void SubscriptionDropped(
        StreamSubscription subscription, 
        SubscriptionDroppedReason reason, 
        Exception exception)
    {
        _logger.LogWarning(exception, "Subscription was dropped: {Reason}", reason);
        _subscription = null;
        
        // Try to reconnect unless the service is stopping
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationToken(false));
        
        _ = TryReconnectAsync(linkedTokenSource.Token);
    }

    private async Task TryReconnectAsync(CancellationToken cancellationToken)
    {
        if (_reconnectAttempts >= MaxReconnectAttempts)
        {
            _logger.LogError("Reached maximum number of reconnect attempts ({Max})", MaxReconnectAttempts);
            return;
        }

        _reconnectAttempts++;

        // Implement exponential backoff
        var delayMs = Math.Min(1000 * Math.Pow(2, _reconnectAttempts - 1), 30000);
        _logger.LogInformation("Attempting to reconnect in {Delay}ms (attempt {Attempt}/{Max})", 
            delayMs, _reconnectAttempts, MaxReconnectAttempts);

        try
        {
            await Task.Delay((int)delayMs, cancellationToken);
            await EstablishSubscriptionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Reconnect cancelled due to shutdown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reconnect attempt");
            if (!cancellationToken.IsCancellationRequested)
            {
                _ = TryReconnectAsync(cancellationToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventStoreSubscriber stopping");
        
        // Properly dispose of the subscription
        if (_subscription != null)
        {
            _subscription.Dispose();
            _subscription = null;
        }
        
        await base.StopAsync(cancellationToken);
    }
}