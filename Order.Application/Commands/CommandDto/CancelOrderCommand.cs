namespace Order.Application.Commands.CommandDto;

public record CancelOrderCommand(Guid OrderId, string Reason);