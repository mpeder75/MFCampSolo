using Dapr;
using Microsoft.AspNetCore.Mvc;
using MFCampShared.Messages.Payment;
using MFCampShared.Messages.Warehouse;
using Order.Application.Services;
using ShipmentStatusMessage = MFCampShared.Messages.Shipping.ShipmentStatusMessage;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IOrderService orderService, ILogger<EventsController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("payment-completed")]
        [Topic("pubsub", "payment-completed")]
        public async Task<IActionResult> HandlePaymentCompleted(PaymentCompletedMessage message)
        {
            _logger.LogInformation("Received payment completed for order {OrderId}", message.OrderId);
        
            try
            {
                await _orderService.ProcessPaymentCompletedAsync(message.OrderId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment completed for order {OrderId}", message.OrderId);
                return StatusCode(500);
            }
        }

        
        [HttpPost("inventory-reserved")]
        [Topic("pubsub", "inventory-reserved")]
        public async Task<IActionResult> HandleInventoryReserved(InventoryReservedMessage message)
        {
            _logger.LogInformation("Received inventory reserved for order {OrderId}", message.OrderId);
            
            try
            {
                // Update your IOrderService interface and implementation with this method
                await _orderService.ProcessInventoryReservedAsync(message.OrderId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory reserved for order {OrderId}", message.OrderId);
                return StatusCode(500);
            }
        }




        [HttpPost("shipment-status")]
        [Topic("pubsub", "shipment-status")]
        public async Task<IActionResult> HandleShipmentStatusUpdate(ShipmentStatusMessage message)
        {
            _logger.LogInformation("Received shipment status update for order {OrderId}: {Status}", 
                message.OrderId, message.Status);
            
            try
            {
                // Update your IOrderService interface and implementation with this method
                await _orderService.UpdateShipmentStatusAsync(message.OrderId, message.Status, message.TrackingNumber);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shipment status for order {OrderId}", message.OrderId);
                return StatusCode(500);
            }
        }






    }
}
