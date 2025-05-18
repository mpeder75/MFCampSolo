using Dapr.Client;
using MFCampShared.Messages.Payment;

namespace Payment.API.Services;

public interface IPaymentService
{
    Task<PaymentResultMessage> ProcessPaymentAsync(PaymentMessage paymentMessage);
}

// Når en payment er fejlet skal Workflow kalde  MarkPaymentFailed i Order service
public class PaymentService : IPaymentService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<PaymentService> _logger;
    private const string PUBSUB_NAME = "pubsub";
    private const string TOPIC_NAME = "orders";

    public PaymentService(DaprClient daprClient, ILogger<PaymentService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<PaymentResultMessage> ProcessPaymentAsync(PaymentMessage paymentMessage)
    {
        var stages = new (string status, int duration)[]
        {
            ("payment_validating", 1),
            ("payment_processing", 2),
            ("payment_authorizing", 1)
        };

        var result = new PaymentResultMessage
        {
            OrderId = paymentMessage.OrderId,
            Amount = paymentMessage.Amount,
            Status = "unknown"
        };

        try
        {
            foreach (var (status, duration) in stages)
            {
                result.Status = status;
                _logger.LogInformation("Payment {OrderId} - {Status}", result.OrderId, status);

                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
                await Task.Delay(TimeSpan.FromSeconds(duration));
            }

            // Simuler 80% success rate
            bool paymentSuccess = new Random().Next(10) < 8;

            if (paymentSuccess)
            {
                result.Status = "payment_successful";
                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
                _logger.LogInformation("Payment {OrderId} successful", result.OrderId);
            }
            else
            {
                result.Status = "payment_failed";
                result.Error = "Payment was declined by the payment processor";
                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
                _logger.LogInformation("Payment {OrderId} failed", result.OrderId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for OrderId {OrderId}", result.OrderId);
            result.Status = "payment_error";
            result.Error = ex.Message;
            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
            return result;
        }
    }
}
