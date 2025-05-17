namespace Order.Application.Queries.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
        }

        Items = new List<T>(items).AsReadOnly();
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}