using Microsoft.Extensions.Logging;
using Order.Application.Commands.CommandDto;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;
        
        public OrderService(IOrderRepository orderRepository, IEventPublisher eventPublisher, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }
        
        public async Task<Guid> CreateOrderAsync(CreateOrderCommand command)
        {
            _logger.LogInformation("Creating order for customer {CustomerId} through Dapr event flow", command.CustomerId);
            
            var customerId = CustomerId.Create(command.CustomerId);
            var order = new Domain.Aggregates.Order(customerId);
            
            await _orderRepository.SaveAsync(order);

            await _eventPublisher.PublishOrderCreatedAsync(order);
            
            return order.Id.Value;
        }
        
        public async Task ProcessPaymentCompletedAsync(Guid orderId)
        {
            _logger.LogInformation("Processing payment completed event for order {OrderId}", orderId);
            
            var order = await _orderRepository.LoadAsync(OrderId.Create(orderId));
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when processing payment", orderId);
                return;
            }

            // Only approve payment if the order is in PaymentPending status
            if (order.Status == Domain.Enums.OrderStatus.PaymentPending)
            {
                order.MarkPaymentApproved();
                await _orderRepository.SaveAsync(order);
                _logger.LogInformation("Order {OrderId} payment marked as approved", orderId);
            }
            else
            {
                _logger.LogWarning("Order {OrderId} not in correct state for payment approval. Current status: {Status}", 
                    orderId, order.Status);
            }
        }

       
        public async Task ProcessInventoryReservedAsync(Guid orderId)
        {
            _logger.LogInformation("Processing inventory reserved event for order {OrderId}", orderId);
            
            var order = await _orderRepository.LoadAsync(OrderId.Create(orderId));
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when processing inventory reservation", orderId);
                return;
            }

            // Only start processing if payment is approved
            if (order.Status == Domain.Enums.OrderStatus.PaymentApproved)
            {
                order.StartProcessing();
                await _orderRepository.SaveAsync(order);
                _logger.LogInformation("Order {OrderId} processing started after inventory reservation", orderId);
            }
            else
            {
                _logger.LogWarning("Order {OrderId} not in correct state for processing. Current status: {Status}",
                    orderId, order.Status);
            }
        }

        public async Task UpdateShipmentStatusAsync(Guid orderId, string status, string trackingNumber)
        {
            _logger.LogInformation("Updating shipment status for order {OrderId} to {Status}", orderId, status);
            
            var order = await _orderRepository.LoadAsync(OrderId.Create(orderId));
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when updating shipment status", orderId);
                return;
            }

            if (status.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
            {
                if (order.Status == Domain.Enums.OrderStatus.Processing)
                {
                    order.ProcessShippingStatusUpdate(status, trackingNumber);
                    await _orderRepository.SaveAsync(order);
                    
                    // Publish event
                    await _eventPublisher.PublishOrderShippedAsync(orderId, order.CustomerId.Value);
                    _logger.LogInformation("Order {OrderId} marked as shipped", orderId);
                }
                else
                {
                    _logger.LogWarning("Order {OrderId} not in correct state for shipping. Current status: {Status}",
                        orderId, order.Status);
                }
            }
            else if (status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
            {
                if (order.Status == Domain.Enums.OrderStatus.Shipped)
                {
                    order.MarkAsDelivered();
                    await _orderRepository.SaveAsync(order);
                    _logger.LogInformation("Order {OrderId} marked as delivered", orderId);
                }
                else
                {
                    _logger.LogWarning("Order {OrderId} not in correct state for delivery. Current status: {Status}",
                        orderId, order.Status);
                }
            }
            else
            {
                _logger.LogInformation("Received unknown shipment status '{Status}' for order {OrderId}", 
                    status, orderId);
            }
        }
    }
}