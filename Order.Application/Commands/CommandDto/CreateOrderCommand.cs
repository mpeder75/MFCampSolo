namespace Order.Application.Commands.CommandDto;

public record CreateOrderCommand(Guid CustomerId)
{
    public List<OrderItemDto> Items { get; init; } = new();
}

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);