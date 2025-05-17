using Dapr.Client;
using Microsoft.Extensions.Logging;
using MFCampShared.Messages.Order;
using Order.Domain.Entities;
using Order.Application.Services;

namespace Order.Infrastructure.Services
{
    public class OrderEventPublisher : IEventPublisher
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<OrderEventPublisher> _logger;
        private const string PUBSUB_NAME = "pubsub";

        public OrderEventPublisher(DaprClient daprClient, ILogger<OrderEventPublisher> logger)
        {
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishOrderCreatedAsync(Domain.Aggregates.Order order)
        {
            var message = new OrderCreatedMessage
            {
                OrderId = order.Id.Value, // Assuming Id is OrderId value object
                CustomerId = order.CustomerId.Value, // Assuming CustomerId is a value object
                TotalAmount = order.TotalAmount.Amount, // Use the property instead of non-existent method
                Items = order.Items.Select(item => new OrderItemMessage
                {
                    ProductId = item.ProductId.Value,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice.Amount
                }).ToList()
            };

            _logger.LogInformation("Publishing OrderCreated event for OrderId: {OrderId}", order.Id);
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "order-created", message);
        }

        public async Task PublishOrderShippedAsync(Guid orderId, Guid customerId)
        {
            var message = new OrderShippedMessage
            {
                OrderId = orderId,
                CustomerId = customerId
            };

            _logger.LogInformation("Publishing OrderShipped event for OrderId: {OrderId}", orderId);
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "order-shipped", message);
        }
    }
}