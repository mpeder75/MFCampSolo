using Order.Application.Commands.CommandDto;

namespace Order.Application.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrderCommand command);
    Task ProcessPaymentCompletedAsync(Guid orderId);
    Task ProcessInventoryReservedAsync(Guid orderId);
    Task UpdateShipmentStatusAsync(Guid orderId, string status, string trackingNumber);
}