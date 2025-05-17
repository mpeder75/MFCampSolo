using EventStore.Client;

namespace Order.Infrastructure.Events;

public interface ICheckpointStore
{
    Task<Position?> GetCheckpoint();
    Task StoreCheckpoint(Position position);
}