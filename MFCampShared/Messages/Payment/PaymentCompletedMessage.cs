namespace MFCampShared.Messages.Payment;

public class PaymentCompletedMessage : WorkflowMessage
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public bool Successful { get; set; }
    public string? FailureReason { get; set; }
}