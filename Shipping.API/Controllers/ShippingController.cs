using Dapr;
using Dapr.Client;
using MFCampShared.Messages.Payment;
using MFCampShared.Messages.Shipping;
using Microsoft.AspNetCore.Mvc;
using Shipping.API.Services;

namespace Shipping.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingStateService _shippingService;
        private readonly ILogger<ShippingController> _logger;
        private readonly DaprClient _daprClient;
        private const string PUBSUB_NAME = "pubsub";

        public ShippingController(IShippingStateService shippingStateService, ILogger<ShippingController> logger, DaprClient daprClient)
        {
            _shippingService = shippingStateService;
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic("pubsub", "payment-successful")]
        [HttpPost("payment-completed")]
        public async Task<IActionResult> HandlePaymentCompleted(PaymentResultMessage paymentResult)
        {
            if (paymentResult.Status != "payment_successful")
            {
                _logger.LogInformation("Payment failed for OrderId: {OrderId}, no shipping needed", paymentResult.OrderId);
                return Ok();
            }

            _logger.LogInformation("Payment successful for OrderId: {OrderId}, initiating shipping", paymentResult.OrderId);
            
            var shippingMessage = new ShippingMessage
            {
                OrderId = paymentResult.OrderId,
                CustomerId = "customer_from_payment", // In a real system, you'd get this from the order
                PlannedPickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };
            
            var result = await _shippingService.ProcessShipmentAsync(shippingMessage);
            
            // Publish shipping result
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "workflow-shipping", result);
            
            return Ok();
        }

        [Topic("pubsub", "shipping")]
        [HttpPost("process")]
        public async Task<IActionResult> ProcessShipment(ShippingMessage shippingMessage)
        {
            _logger.LogInformation("Received shipping request for OrderId: {OrderId}", shippingMessage.OrderId);
            
            var result = await _shippingService.ProcessShipmentAsync(shippingMessage);
            
            // Publish completion event to workflow
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "workflow-shipping", result);
            
            return Ok(result);
        }

        [HttpPost("test")]
        public async Task<ActionResult<ShippingResultMessage>> TestShipment(ShippingMessage shippingMessage)
        {
            _logger.LogInformation("Test shipping for OrderId: {OrderId}", shippingMessage.OrderId);
            var result = await _shippingService.ProcessShipmentAsync(shippingMessage);
            return Ok(result);
        }
    }
}
