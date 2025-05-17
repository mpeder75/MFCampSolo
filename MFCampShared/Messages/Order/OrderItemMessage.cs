namespace MFCampShared.Messages.Order;

public class OrderItemMessage
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}