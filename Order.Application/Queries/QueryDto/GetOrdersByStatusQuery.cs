namespace Order.Application.Queries.QueryDto;

public record GetOrdersByStatusQuery
{
    public string Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}