namespace Order.Application.Commands.CommandDto;

public record MarkPaymentFailedCommand(Guid OrderId, string Reason);
