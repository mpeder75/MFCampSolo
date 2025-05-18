using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands.CommandDto;
using Order.Application.Services;

namespace Order.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly OrderApplicationService _orderService;
    private readonly ILogger<TestController> _logger;

    public TestController(OrderApplicationService orderService, ILogger<TestController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("complete-flow")]
    public async Task<IActionResult> TestCompleteFlow()
    {
        _logger.LogInformation("Testing complete order flow");

        // 1. Create an order
        var customerId = Guid.NewGuid();
        var createOrderCommand = new CreateOrderCommand(customerId);
        var createResult = await _orderService.CreateOrderAsync(createOrderCommand);

        if (!createResult.Success)
        {
            return BadRequest(new { error = "Failed to create order", details = createResult.ErrorMessage });
        }

        // Get the order ID - you'd need to modify your createResult to return this
        var orderId = Guid.Parse(createResult.Data.ToString()); 

        // 2. Add an item to the order
        var addItemCommand = new AddOrderItemCommand(
            orderId,
            Guid.NewGuid(),
            "Test Product",
            2,
            100.00m, 
            "DKK");
        var addItemResult = await _orderService.AddOrderItemAsync(addItemCommand);

        if (!addItemResult.Success)
        {
            return BadRequest(new { error = "Failed to add item to order", details = addItemResult.ErrorMessage });
        }

        // 3. Set shipping address
        var addressCommand = new SetShippingAddressCommand(
            orderId,
            "123 Test Street",
            "2800",
            "Lyngby");
        var addressResult = await _orderService.SetShippingAddressAsync(addressCommand);

        if (!addressResult.Success)
        {
            return BadRequest(new { error = "Failed to set address", details = addressResult.ErrorMessage });
        }

        // 4. Validate the order
        var validateCommand = new ValidateOrderCommand(orderId);
        var validateResult = await _orderService.ValidateOrderAsync(validateCommand);

        if (!validateResult.Success)
        {
            return BadRequest(new { error = "Failed to validate order", details = validateResult.ErrorMessage });
        }

        return Ok(new
        {
            message = "Complete flow initiated",
            orderId = orderId,
            note = "The system will now process payment and shipping asynchronously via the outbox pattern"
        });
    }
}