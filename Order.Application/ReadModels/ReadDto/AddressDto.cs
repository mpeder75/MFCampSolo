namespace Order.Application.ReadModels.ReadDto;

public record AddressDto(
    string Street,
    string ZipCode,
    string City
)
{
    // Denne egenskab bruges af RavenDB til at gemme dokument ID
    public string DocumentId { get; set; }
}