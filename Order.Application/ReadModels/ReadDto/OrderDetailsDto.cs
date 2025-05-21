namespace Order.Application.ReadModels.ReadDto;

public record OrderDetailsDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedDate,
    AddressDto ShippingAddress,
    IReadOnlyList<OrderItemDto> Items,
    string PaymentFailureReason,
    string TrackingNumber)
{
    public string DocumentId { get; set; }
}