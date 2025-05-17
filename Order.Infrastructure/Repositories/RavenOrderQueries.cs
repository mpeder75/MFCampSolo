using Order.Application.Queries.Common;
using Order.Application.ReadModels;
using Order.Application.ReadModels.ReadDto;
using Order.Infrastructure.Indexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Order.Infrastructure.Repositories;

public class RavenOrderQueries : IOrderQueries
{
    private readonly RavenDbContext _dbContext;

    public RavenOrderQueries(RavenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(Guid orderId)
    {
        using var session = _dbContext.OpenAsyncSession();
        var key = $"orders/{orderId}";

        return await session.LoadAsync<OrderSummaryDto>(key);
    }

    public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId)
    {
        using var session = _dbContext.OpenAsyncSession();
        return await session.LoadAsync<OrderDetailsDto>($"orders/{orderId}/details");
    }

    public async Task<OrderSummaryDto> GetOrderSummaryByIdAsync(Guid orderId)
    {
        using var session = _dbContext.OpenAsyncSession();
        return await session.LoadAsync<OrderSummaryDto>($"orders/{orderId}");
    }

    public async Task<int> GetCustomerOrdersCountAsync(Guid customerId)
    {
        using var session = _dbContext.OpenAsyncSession();
        return await session.Query<OrderSummaryDto, Orders_ByCustomerId>()
            .Where(o => o.CustomerId == customerId)
            .CountAsync();
    }

    public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(
        Guid customerId,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "OrderDate",
        bool descending = true)
    {
        using var session = _dbContext.OpenAsyncSession();
        var query = session.Query<OrderSummaryDto, Orders_ByCustomerId>()
            .Where(o => o.CustomerId == customerId);

        // Apply sorting
        switch (sortBy.ToLowerInvariant())
        {
            case "orderdate":
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
            case "totalamount":
                query = descending
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount);
                break;
            case "status":
                query = descending
                    ? query.OrderByDescending(o => o.Status)
                    : query.OrderBy(o => o.Status);
                break;
            default:
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
        }

        // Apply pagination
        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectInto<OrderHistoryDto>()
            .ToListAsync();

        return results;
    }

    public async Task<PagedResult<OrderSummaryDto>> GetOrdersByStatusAsync(
        string status,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "OrderDate",
        bool descending = true)
    {
        using var session = _dbContext.OpenAsyncSession();
        var query = session.Query<OrderSummaryDto, Orders_ByStatus>()
            .Where(o => o.Status == status);

        // Get total count for pagination
        var totalItems = await query.CountAsync();

        // Apply sorting
        switch (sortBy.ToLowerInvariant())
        {
            case "orderdate":
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
            case "totalamount":
                query = descending
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount);
                break;
            case "lastmodified":
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
            default:
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
        }

        // Apply pagination
        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderSummaryDto>(results, totalItems, pageNumber, pageSize);
    }

    public async Task<PagedResult<OrderSummaryDto>> SearchOrdersAsync(
        string searchTerm,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "OrderDate",
        bool descending = true)
    {
        using var session = _dbContext.OpenAsyncSession();

        // Parse search term to see if it could be a GUID
        bool isGuid = Guid.TryParse(searchTerm, out var guidValue);

        // Start with a query that includes all orders
        IQueryable<OrderSummaryDto> query;

        if (isGuid)
        {
            // If the search term is a GUID, try to match it against order ID or customer ID
            query = session.Query<OrderSummaryDto>()
                .Where(o => o.Id == guidValue || o.CustomerId == guidValue);
        }
        else
        {
            // If the search term is not a GUID, try to match it against status
            // In a more complete implementation, you might use full-text search
            query = session.Query<OrderSummaryDto>()
                .Where(o => o.Status.Contains(searchTerm));
        }

        // Get total count for pagination
        var totalItems = await query.CountAsync();

        // Apply sorting
        switch (sortBy.ToLowerInvariant())
        {
            case "orderdate":
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
            case "totalamount":
                query = descending
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount);
                break;
            case "status":
                query = descending
                    ? query.OrderByDescending(o => o.Status)
                    : query.OrderBy(o => o.Status);
                break;
            default:
                query = descending
                    ? query.OrderByDescending(o => o.CreatedDate)
                    : query.OrderBy(o => o.CreatedDate);
                break;
        }

        // Apply pagination
        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderSummaryDto>(results, totalItems, pageNumber, pageSize);
    }

    // Method to demonstrate efficient batch loading
    public async Task<IDictionary<Guid, OrderDetailsDto>> GetOrderDetailsBatchAsync(IEnumerable<Guid> orderIds)
    {
        using var session = _dbContext.OpenAsyncSession();

        // Convert IDs to document IDs
        var documentIds = orderIds.Select(id => $"orders/{id}/details").ToList();

        // Load all documents in a single request
        var orderDetails = await session.LoadAsync<OrderDetailsDto>(documentIds);

        // Convert back to a dictionary with order IDs as keys
        return orderDetails
            .Where(kvp => kvp.Value != null)
            .ToDictionary(
                kvp => Guid.Parse(kvp.Key.Split('/')[1]),
                kvp => kvp.Value);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync(int pageSize, int pageNumber)
    {
        using var session = _dbContext.OpenAsyncSession();

        return await session.Query<OrderSummaryDto, Orders_Summary>()
            .OrderByDescending(o => o.CreatedDate)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
    }
}
