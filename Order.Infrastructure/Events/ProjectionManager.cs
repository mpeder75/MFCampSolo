using System.Text;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;
using Order.Infrastructure.Exceptions;

namespace Order.Infrastructure.Events;

public class ProjectionManager
{
    private readonly ICheckpointStore _checkpointStore;
    private readonly IEventSerializer _eventSerializer;
    private readonly EventStoreClient _eventStoreClient;
    private readonly ILogger<ProjectionManager> _logger;
    private readonly List<IProjection> _projections = new();
    private StreamSubscription _subscription;

    // Constructor takes EventStoreClient and checkpoint store
    public ProjectionManager(
        EventStoreClient eventStoreClient,
        ICheckpointStore checkpointStore,
        IEventSerializer eventSerializer,
        ILogger<ProjectionManager> logger)
    {
        _eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Method to register a projection
    public void RegisterProjection(IProjection projection)
    {
        if (projection == null) throw new ArgumentNullException(nameof(projection));
        _projections.Add(projection);
    }

    // Method to start the subscription
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the last checkpoint position
            var position = await _checkpointStore.GetCheckpoint();

            // Create FromAll with the position or start from beginning
            FromAll fromAll;
            if (position != null)
            {
                fromAll = FromAll.After(position.Value);
            }
            else
            {
                fromAll = FromAll.Start;
            }

            // Subscribe to all streams
            _subscription = await _eventStoreClient.SubscribeToAllAsync(
                fromAll, 
                EventAppeared,
                subscriptionDropped: SubscriptionDropped,
                filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Projection manager started from position {Position}", 
                position != null ? position.ToString() : "Start");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start projection subscription");
            throw new InfrastructureException("Failed to start projection subscription", ex);
        }
    }

    // Method to handle incoming events
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

            // Deserialize the event
            var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var domainEvent = _eventSerializer.Deserialize(eventData, resolvedEvent.Event.EventType);

            if (domainEvent == null)
            {
                _logger.LogWarning("Failed to deserialize event of type {EventType}", resolvedEvent.Event.EventType);
                return;
            }

            _logger.LogDebug("Processing event {EventType} for projections", domainEvent.GetType().Name);

            // Project the event through all registered projections
            foreach (var projection in _projections)
            {
                await projection.ProjectAsync(domainEvent);
            }

            // Store the checkpoint after successful projection
            if (resolvedEvent.OriginalPosition.HasValue)
            {
                await _checkpointStore.StoreCheckpoint(resolvedEvent.OriginalPosition.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error projecting event {EventType}", resolvedEvent.Event.EventType);
            // Consider adding retry logic or error handling strategy here
        }
    }

    // Method to handle subscription drops
    private void SubscriptionDropped(
        StreamSubscription subscription,
        SubscriptionDroppedReason reason,
        Exception exception)
    {
        _logger.LogWarning(exception, "Projection subscription was dropped: {Reason}", reason);

        // Depending on your application's resilience strategy, you might want to
        // automatically reconnect after a brief delay
        Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => StartAsync())
            .ConfigureAwait(false);
    }

    // Method to stop the subscription
    public Task StopAsync()
    {
        _subscription?.Dispose();
        _logger.LogInformation("Projection manager stopped");
        return Task.CompletedTask;
    }
}