namespace MFCampShared.Messages.Order;

public class OrderShippedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime ShippedDate { get; set; } = DateTime.UtcNow;
}