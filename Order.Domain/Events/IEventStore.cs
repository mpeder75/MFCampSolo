namespace Order.Domain.Events;

public interface IEventStore
{
    Task SaveEventAsync(string streamName, IEnumerable<DomainEvent> events, int expectedVersion);
    Task<IEnumerable<DomainEvent>> LoadEvents(string streamName);
}