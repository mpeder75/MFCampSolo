namespace MFCampShared.Messages.Shipping;

public class ShippingMessage : WorkflowMessage
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public string? Error { get; set; }
    public DateOnly? PlannedPickupDate { get; set; }
    public DateOnly? PickupCompleted { get; set; }
}

public class ShippingResultMessage : ShippingMessage
{
    public string Status { get; set; }
}