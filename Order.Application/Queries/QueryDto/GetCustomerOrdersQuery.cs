namespace Order.Application.Queries.QueryDto;

public record GetCustomerOrdersQuery
{
    public Guid CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "OrderDate";
    public bool Descending { get; init; } = true;
}