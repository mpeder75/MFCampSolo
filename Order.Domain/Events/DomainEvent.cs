namespace Order.Domain.Events;

public abstract class DomainEvent
{
    public DateTime Timestamp { get; init; }
    public Guid EventId { get; init; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
}