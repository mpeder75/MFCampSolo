namespace Order.Application.Commands.CommandDto;

public record SetShippingAddressCommand(Guid OrderId, string Street, string ZipCode, string City);
