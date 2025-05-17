namespace Order.Application.Commands.CommandDto;

public record UpdateOrderItemQuantityCommand(Guid OrderId, Guid ProductId, int NewQuantity);
