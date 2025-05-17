namespace MFCampShared.Messages.Payment;

public class PaymentMessage : WorkflowMessage
{
    public string OrderId { get; set; }
    public string Amount { get; set; }
}


public class PaymentResultMessage : PaymentMessage
{
    public string Status { get; set; }
    public string? Error { get; set; }
}