namespace Order.Application.Queries.QueryDto;

public record GetOrdersQuery
{
    public int PageNumber { get; }
    public int PageSize { get; }
    
    public GetOrdersQuery(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1", nameof(pageNumber));
        
        if (pageSize < 1)
            throw new ArgumentException("Page size must be at least 1", nameof(pageSize));
        
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}