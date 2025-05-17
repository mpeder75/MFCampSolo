using Dapr.Client;
using MFCampShared.Messages.Shipping;

namespace Shipping.API.Services
{
    public interface IShippingStateService
    {
        Task<ShippingResultMessage> ProcessShipmentAsync(ShippingMessage shippingMessage);
    }

    public class ShippingStateService : IShippingStateService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<ShippingStateService> _logger;
        private const string PUBSUB_NAME = "pubsub";
        private const string TOPIC_NAME = "orders";


        public ShippingStateService(DaprClient daprClient, ILogger<ShippingStateService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task<ShippingResultMessage> ProcessShipmentAsync(ShippingMessage shippingMessage)
        {
            var stages = new (string status, int duration)[]
            {
                ("shipping_initialized", 1),
                ("shipping_preparing", 1),
                ("shipping_dispatching", 1)
            };

            var result = new ShippingResultMessage
            {
                WorkflowId = shippingMessage.WorkflowId,
                OrderId = shippingMessage.OrderId,
                CustomerId = shippingMessage.CustomerId,
                PlannedPickupDate =
                    shippingMessage.PlannedPickupDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Status = "initialized"
            };

            foreach (var (status, duration) in stages)
            {
                result.Status = status;
                _logger.LogInformation("Shipment {OrderId} - {Status}", result.OrderId, status);

                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);

                await Task.Delay(TimeSpan.FromSeconds(duration));
            }

            result.Status = "shipping_completed";
            result.PickupCompleted = DateOnly.FromDateTime(DateTime.Today);

            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
            _logger.LogInformation("Shipment {OrderId} successfully delivered", result.OrderId);

            return result;
        }
    }
}