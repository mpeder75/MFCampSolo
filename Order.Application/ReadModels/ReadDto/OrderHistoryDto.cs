namespace Order.Application.ReadModels.ReadDto;

public record OrderHistoryDto(
    Guid Id,
    Guid CustomerId,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount)
{
    // Denne egenskab bruges af RavenDB til at gemme dokument ID
    public string DocumentId { get; set; }
}