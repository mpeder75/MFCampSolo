using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using MFCampShared.Messages.Payment;
using Payment.API.Services;

namespace Payment.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly DaprClient _daprClient;
    private const string PUBSUB_NAME = "pubsub";

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, DaprClient daprClient)
    {
        _paymentService = paymentService;
        _logger = logger;
        _daprClient = daprClient;
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