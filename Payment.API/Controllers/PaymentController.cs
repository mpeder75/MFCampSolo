using Dapr;
using Dapr.Client;
using MFCampShared.Messages.Order;
using MFCampShared.Messages.Payment;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Services;

namespace Payment.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";
    private readonly DaprClient _daprClient;
    private readonly ILogger<PaymentController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, DaprClient daprClient)
    {
        _paymentService = paymentService;
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pubsub", "order-created")]
    [HttpPost("order-created")]
    public async Task<IActionResult> HandleOrderCreated(OrderCreatedMessage orderCreatedMessage)
    {
        _logger.LogInformation("Received order created event for OrderId: {OrderId}", orderCreatedMessage.OrderId);

        var paymentMessage = new PaymentMessage
        {
            WorkflowId = Guid.NewGuid().ToString(),
            OrderId = orderCreatedMessage.OrderId.ToString(),
            Amount = orderCreatedMessage.TotalAmount.ToString()
        };

        var result = await _paymentService.ProcessPaymentAsync(paymentMessage);

        // Publish the result back
        await _daprClient.PublishEventAsync(PUBSUB_NAME, "workflow-payment", result);

        return Ok();
    }

    [Topic("pubsub", "payment")]
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment(PaymentMessage paymentMessage)
    {
        _logger.LogInformation("Received payment request for OrderId: {OrderId}", paymentMessage.OrderId);

        var result = await _paymentService.ProcessPaymentAsync(paymentMessage);

        await _daprClient.PublishEventAsync(PUBSUB_NAME, "workflow-payment", result);

        return Ok();
    }

    [HttpPost("test")]
    public async Task<ActionResult<PaymentResultMessage>> TestPayment(PaymentMessage paymentMessage)
    {
        _logger.LogInformation("Test payment for OrderId: {OrderId}", paymentMessage.OrderId);
        var result = await _paymentService.ProcessPaymentAsync(paymentMessage);
        return Ok(result);
    }
}