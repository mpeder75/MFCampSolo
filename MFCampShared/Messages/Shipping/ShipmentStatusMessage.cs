namespace MFCampShared.Messages.Shipping;

public class ShipmentStatusMessage : WorkflowMessage
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
}