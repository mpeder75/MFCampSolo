using Order.Application.Queries.QueryDto;
using Order.Application.ReadModels;
using Order.Application.ReadModels.ReadDto;

namespace Order.Application.Queries.Handlers;

public class GetOrderDetailsQueryHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly IOrderQueries _orderQueries;

    public GetOrderDetailsQueryHandler(IOrderQueries _orderQueries)
    {
        _orderQueries = _orderQueries ?? throw new ArgumentNullException(nameof(_orderQueries));
    }

    public async Task<OrderDetailsDto> HandleAsync(GetOrderDetailsQuery query)
    {
        if (query == null)
        {
            throw new ArgumentException(nameof(query));
        }

        if (query.OrderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID cannot be empty", nameof(query.OrderId));
        }

        var orderDetails = await _orderQueries.GetOrderDetailsAsync(query.OrderId);

        if (orderDetails == null)
        {
            throw new KeyNotFoundException($"Order with ID {query.OrderId} was not found");
        }
    
        return orderDetails;
    }
}