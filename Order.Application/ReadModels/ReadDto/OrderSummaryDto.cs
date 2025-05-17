namespace Order.Application.ReadModels.ReadDto;

public record OrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedDate,
    int ItemCount)
{
    // Denne egenskab bruges af RavenDB til at gemme dokument ID
    public string DocumentId { get; set; }
}