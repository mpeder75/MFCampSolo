namespace Order.Application.Commands.CommandDto;

public record MarkPaymentPendingCommand(Guid OrderId);