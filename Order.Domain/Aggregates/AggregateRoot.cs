using Order.Domain.Events;

namespace Order.Domain.Aggregates;

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents;

    /// <summary>
    ///     Versions styring af aggregate for optimistic concurrency control
    /// </summary>
    public int Version { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public AggregateRoot()
    {
        _domainEvents = new List<DomainEvent>();
    }

    /// <summary>
    ///     Applies an event to the aggregate, updates state via When method, and adds to uncommitted events
    /// </summary>
    /// <param name="event">The domain event to apply</param>
    protected void Apply(DomainEvent @event)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // First perform the state change
        When(@event);

        // Ensure the aggregate is valid after state change
        EnsureValidState();

        // Add to uncommitted events list
        _domainEvents.Add(@event);

        // Increment version
        Version++;
    }

    protected abstract void When(DomainEvent @event);

    protected abstract void EnsureValidState();

    public IReadOnlyCollection<DomainEvent> GetUncommittedEvents()
    {
        return DomainEvents;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }


    public abstract override object GetIdentity();
}