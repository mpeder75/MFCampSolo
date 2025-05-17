namespace Order.Domain.Repositories;

public interface IEventStoreRepository
{
    Task AppendEventAsync(string streamName, object @event);
    Task<IEnumerable<object>> ReadStreamAsync(string streamName);
}

