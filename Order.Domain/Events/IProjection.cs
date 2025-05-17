namespace Order.Domain.Events;

public interface IProjection
{
    Task ProjectAsync(DomainEvent @event);
}