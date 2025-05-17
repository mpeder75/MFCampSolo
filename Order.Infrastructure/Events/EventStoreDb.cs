using EventStore.Client;
using Order.Domain.Events;
using System.Text;
using System.Text.Json;

namespace Order.Infrastructure.Events;

public class EventStoreDb
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly IEventSerializer _eventSerializer;

    public EventStoreDb(EventStoreClient eventStoreClient, IEventSerializer eventSerializer)
    {
        _eventStoreClient = eventStoreClient;
        _eventSerializer = eventSerializer;

    }

   
    public async Task SaveEventAsync(string streamName, IEnumerable<DomainEvent> events, long expectedVersion)
    {
        if (!events.Any())
        {
            return;
        }

        var eventData = events.Select(evt =>
        {
            var data = _eventSerializer.Serialize(evt);
            var eventType = evt.GetType().Name;
                
            return new EventData(
                Uuid.NewUuid(),
                eventType,
                Encoding.UTF8.GetBytes(data),
                Array.Empty<byte>() // No metadata for now
            );
        }).ToArray();

        try
        {
            // Convert from domain version to EventStore expected version
            var eventStoreExpectedVersion = expectedVersion < 0 
                ? StreamRevision.None 
                : StreamRevision.FromInt64(expectedVersion);

            await _eventStoreClient.AppendToStreamAsync(
                streamName,
                eventStoreExpectedVersion,
                eventData);
        }
        catch (WrongExpectedVersionException)
        {
            throw new InvalidOperationException("Concurrency conflict: the stream has been modified.");
        }
    }

    public async Task<IEnumerable<DomainEvent>> LoadEvents(string streamName)
    {
        var events = new List<DomainEvent>();
        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards, 
            streamName, 
            StreamPosition.Start);

        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return events;
        }

        await foreach (var resolvedEvent in result)
        {
            if (resolvedEvent.Event.Data.Length > 0)
            {
                var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
                var eventType = resolvedEvent.Event.EventType;
                var domainEvent = _eventSerializer.Deserialize(eventData, eventType);
                    
                if (domainEvent != null)
                {
                    events.Add(domainEvent);
                }
            }
        }

        return events;
    }
}