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
    // Denne egenskab bruges af RavenDB til at gemme dokument ID
    public string DocumentId { get; set; }
}