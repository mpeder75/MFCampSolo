using Order.Application.ReadModels.ReadDto;

namespace Order.Application.ReadModels;

public interface IOrderQueries
{
    Task<OrderSummaryDto> GetOrderSummaryAsync(Guid orderId);
    Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId);
    Task<OrderSummaryDto> GetOrderSummaryByIdAsync(Guid orderId);
    Task<int> GetCustomerOrdersCountAsync(Guid customerId);
    Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        string sortBy,
        bool descending);
}