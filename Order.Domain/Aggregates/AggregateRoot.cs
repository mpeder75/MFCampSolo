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

    /// <summary>
    ///     Handles the state change logic for each event type
    ///     Must be implemented by derived classes
    /// </summary>
    /// <param name="event">The event to process</param>
    protected abstract void When(DomainEvent @event);

    /// <summary>
    ///     Validates the aggregate's invariants
    ///     Must be implemented by derived classes to enforce business rules
    /// </summary>
    protected abstract void EnsureValidState();

    /// <summary>
    ///     Returns all uncommitted events
    /// </summary>
    /// <returns>Collection of domain events</returns>
    public IReadOnlyCollection<DomainEvent> GetUncommittedEvents()
    {
        return DomainEvents;
    }

    /// <summary>
    ///     Clears the list of uncommitted events after they've been persisted
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    ///     Returns the identity of the aggregate
    /// </summary>
    /// <returns>The unique identifier for this aggregate</returns>
    public abstract override object GetIdentity();
}