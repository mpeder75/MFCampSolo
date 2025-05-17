namespace Order.Application.Commands.CommandDto;

public record ShipOrderCommand(Guid OrderId, string TrackingNumber);
