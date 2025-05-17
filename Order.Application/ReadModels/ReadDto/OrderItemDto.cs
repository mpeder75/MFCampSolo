namespace Order.Application.ReadModels.ReadDto;

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency)
{
    public string DocumentId { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
