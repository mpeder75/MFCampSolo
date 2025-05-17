namespace MFCampShared.Messages.Warehouse;

public class ShipmentStatusMessage : WorkflowMessage
{
    public Guid OrderId { get; set; }
    public bool Success { get; set; }
    public List<string> UnavailableItems { get; set; } = new();
}