using Order.Domain.Aggregates;

namespace Order.Application.Services;

public interface IEventPublisher
{
    Task PublishOrderCreatedAsync(Domain.Aggregates.Order order);
    Task PublishOrderShippedAsync(Guid orderId, Guid customerId);
}