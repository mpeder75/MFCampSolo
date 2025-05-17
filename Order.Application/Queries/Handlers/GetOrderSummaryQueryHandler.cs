using Order.Application.Queries.QueryDto;
using Order.Application.ReadModels;
using Order.Application.ReadModels.ReadDto;

namespace Order.Application.Queries.Handlers;

public class GetOrderSummaryQueryHandler : IQueryHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    private readonly IOrderQueries _orderQueries;

    public GetOrderSummaryQueryHandler(IOrderQueries orderQueries)
    {
        _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
    }


    public async Task<OrderSummaryDto> HandleAsync(GetOrderSummaryQuery query)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (query.OrderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID cannot be empty.", nameof(query.OrderId));
        }

        var orderSummary = await _orderQueries.GetOrderSummaryByIdAsync(query.OrderId);

        return orderSummary;
    }
}