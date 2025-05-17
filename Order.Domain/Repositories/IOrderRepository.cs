using Order.Domain.Aggregates;
using Order.Domain.ValueObjects;

namespace Order.Domain.Repositories;

public interface IOrderRepository
{
    Task SaveAsync(Aggregates.Order order);
    Task<Aggregates.Order> LoadAsync(OrderId orderId);
}