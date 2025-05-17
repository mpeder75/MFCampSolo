namespace Order.Application.Commands.CommandDto;

public record RemoveOrderItemCommand(Guid OrderId, Guid ProductId);
