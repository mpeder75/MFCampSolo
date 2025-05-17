namespace Order.Application.Commands.CommandDto;

public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);