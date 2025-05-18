namespace MFCampShared.Messages.Order;

public class OrderCreatedMessage : WorkflowMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
    public DateTime CreatedDate { get; set; }
}