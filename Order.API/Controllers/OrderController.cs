using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.Application.Commands.CommandDto;
using Order.Application.Services;

namespace Order.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly OrderApplicationService _orderApplicationService;

    public OrderController(OrderApplicationService orderApplicationService, ILogger<OrderController> logger)
    {
        _orderApplicationService = orderApplicationService;
        _logger = logger;
    }

   /*
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   */
    [HttpPost]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            _logger.LogInformation("Received new Order request with {CustomerId}", command.CustomerId);
            var result = await _orderApplicationService.CreateOrderAsync(command);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to create order: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new
            {
                orderId = command.CustomerId,
                message = "Order created successfully"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
    /*
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    [HttpPut]
    [Route("{orderId}/items")]
    public async Task<ActionResult> AddItemToOrder(Guid orderId, [FromBody] AddOrderItemCommand command)
    {
        try
        {
            _logger.LogInformation("Adding item to order {OrderId}", orderId);
            var result = await _orderApplicationService.AddOrderItemAsync(command);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to add item to order: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new
                { message = "Item added successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding item to order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    /*
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    /// <summary>
    ///     Opdaterer leveringsadressen for en specifik ordre.
    ///     Bemærk: Dette opdaterer KUN leveringsadressen for en specifikke ordre
    ///     og ikke kundens standardadresse i Customer.ShippingAddress er en del af ordrens
    ///     aggregat og kan være forskellig fra kundens standardadresse (f.eks. ved
    ///     gavelevering eller midlertidige adresser).
    ///     Ansvaret for Address ligger i Customer servicen, mens
    ///     ordrespecifikke leveringsadresser håndteres af Order-servicen for at
    ///     respektere domænegrænser i DDD-arkitekturen.
    /// </summary>
    /// <param name="orderId">ID for ordren der skal opdateres</param>
    /// <param name="command">Kommando med leveringsadressedata</param>
    [HttpPut("{orderId}/address")]
    public async Task<ActionResult> SetShippingAddress(Guid orderId, [FromBody] SetShippingAddressCommand command)
    {
        try
        {
            if (command.OrderId != orderId)
            {
                return BadRequest(new { error = "Order ID in route must match order ID in command" });
            }

            _logger.LogInformation("Setting shipping address for order {OrderId}", orderId);
            var result = await _orderApplicationService.SetShippingAddressAsync(command);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to set shipping address: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Shipping address set successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting shipping address");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    /*
     ProducesResponseType(StatusCodes.Status200OK)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    [HttpPut("{orderId}/validate")]
    public async Task<ActionResult> ValidateOrder(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Validating order {OrderId}", orderId);

            var command = new ValidateOrderCommand(orderId);
            var result = await _orderApplicationService.ValidateOrderAsync(command);

            if (!result.Success)
            {
                _logger.LogWarning("Order validation failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Order validated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    /*
    [ProducesResponseType(StatusCodes.Status200OK)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   */
    [HttpPut("{orderId}/payment/pending")]
    public async Task<ActionResult> MarkPaymentPending(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Marking payment as pending for order {OrderId}", orderId);

            var command = new MarkPaymentPendingCommand(orderId);
            var result = await _orderApplicationService.MarkPaymentPendingAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to mark payment as pending: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Payment marked as pending successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error marking payment as pending");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/payment/approved")]
    public async Task<ActionResult> MarkPaymentApproved(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Marking payment as approved for order {OrderId}", orderId);

            var command = new MarkPaymentApprovedCommand(orderId);
            var result = await _orderApplicationService.MarkPaymentApprovedAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to mark payment as approved: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Payment marked as approved successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error marking payment as approved");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/payment/failed")]
    public async Task<ActionResult> MarkPaymentFailed(Guid orderId, [FromBody] MarkPaymentFailedRequest request)
    {
        try
        {
            _logger.LogInformation("Marking payment as failed for order {OrderId}", orderId);

            var command = new MarkPaymentFailedCommand(orderId, request.Reason);
            var result = await _orderApplicationService.MarkPaymentFailedAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to mark payment as failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Payment marked as failed successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error marking payment as failed");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/process")]
    public async Task<ActionResult> ProcessOrder(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Processing order {OrderId}", orderId);

            var command = new StartProcessingOrderCommand(orderId);
            var result = await _orderApplicationService.StartProcessingOrderAsync(command);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to process order: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Order processed successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/ship")]
    public async Task<ActionResult> ShipOrder(Guid orderId, [FromBody] ShipOrderRequest request)
    {
        try
        {
            _logger.LogInformation("Shipping order {OrderId} with tracking number", orderId, request.TrackingNumber);

            var command = new ShipOrderCommand(orderId, request.TrackingNumber);
            var result = await _orderApplicationService.ShipOrderAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to ship order: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Order shipped successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error shipping order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/deliver")]
    public async Task<ActionResult> DeliverOrder(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Marking order {OrderId} as delivered", orderId);
        
            var command = new DeliverOrderCommand(orderId);
            var result = await _orderApplicationService.DeliverOrderAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to mark order as delivered: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }
        
            return Ok(new { message = "Order delivered successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error marking order as delivered");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpDelete("{orderId}/items/{productId}")]
    public async Task<ActionResult> RemoveOrderItem(Guid orderId, Guid productId)
    {
        try
        {
            _logger.LogInformation("Removing product {ProductId} from order {OrderId}", productId, orderId);
        
            var command = new RemoveOrderItemCommand(orderId, productId);
            var result = await _orderApplicationService.RemoveOrderItemAsync(command);
        
            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        
            return Ok(new { message = "Item removed successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing item from order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{orderId}/items/{productId}")]
    public async Task<ActionResult> UpdateOrderItemQuantity(Guid orderId, Guid productId, [FromBody] UpdateQuantityRequest request)
    {
        try
        {
            var command = new UpdateOrderItemQuantityCommand(orderId, productId, request.Quantity);
            var result = await _orderApplicationService.UpdateOrderItemQuantityAsync(command);
        
            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        
            return Ok(new { message = "Item quantity updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating item quantity");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }


    [HttpPut("{orderId}/cancel")]
    public async Task<ActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderRequest request)
    {
        try
        {
            _logger.LogInformation("Cancelling order {OrderId} because {CancelOrderRequest}", orderId, request.Reason);
            var command = new CancelOrderCommand(orderId, request.Reason);
            var result = await _orderApplicationService.CancelOrderAsync(command);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to cancel order: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }
            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cancelling order");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
}