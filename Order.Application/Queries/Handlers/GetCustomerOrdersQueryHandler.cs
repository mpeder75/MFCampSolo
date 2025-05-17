using Order.Application.Queries.Common;
using Order.Application.Queries.QueryDto;
using Order.Application.ReadModels;
using Order.Application.ReadModels.ReadDto;

namespace Order.Application.Queries.Handlers;

public class GetCustomerOrdersQueryHandler : IQueryHandler<GetCustomerOrdersQuery, PagedResult<OrderHistoryDto>>
{
    private readonly IOrderQueries _orderQueries;

    public GetCustomerOrdersQueryHandler(IOrderQueries orderQueries)
    {
        _orderQueries = orderQueries;
    }

    public async Task<PagedResult<OrderHistoryDto>> HandleAsync(GetCustomerOrdersQuery query)
    {
        if (query.CustomerId == null)
        {
            throw new ArgumentException("CustomerId cannot be null");
        }

        if (query.PageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(query.PageNumber));
        }

        if (query.PageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0", nameof(query.PageSize));
        }

        var totalItems = await _orderQueries.GetCustomerOrdersCountAsync(query.CustomerId);

        var orders = await _orderQueries.GetOrderHistoryAsync(
            query.CustomerId,
            query.PageNumber,
            query.PageSize,
            query.SortBy,
            query.Descending);

        return new PagedResult<OrderHistoryDto>(orders, totalItems, query.PageNumber, query.PageSize);
    }
}