using System.Text.Json;
using EventStore.Client;
using Order.Domain.Repositories;

namespace Order.Infrastructure.Repositories;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly EventStoreClient _eventStoreClient;

    public EventStoreRepository(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task AppendEventAsync(string streamName, object @event)
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            @event.GetType().Name,
            JsonSerializer.SerializeToUtf8Bytes(@event)
        );

        await _eventStoreClient.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
    }

    public async Task<IEnumerable<object>> ReadStreamAsync(string streamName)
    {
        var events = new List<object>();
        var result = _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

        await foreach (var resolvedEvent in result)
        {
            var eventType = Type.GetType(resolvedEvent.Event.EventType);
            if (eventType != null)
            {
                var @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, eventType);
                if (@event != null)
                {
                    events.Add(@event);
                }
            }
        }

        return events;
    }
}