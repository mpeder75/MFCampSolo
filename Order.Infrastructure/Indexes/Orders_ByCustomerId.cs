using Order.Application.ReadModels.ReadDto;
using Raven.Client.Documents.Indexes;

namespace Order.Infrastructure.Indexes;

public class Orders_ByCustomerId : AbstractIndexCreationTask<OrderHistoryDto>
{
    public Orders_ByCustomerId()
    {
        Map = orders => from order in orders
            where order.CustomerId != Guid.Empty
            select new
            {
                order.CustomerId,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                order.Currency,
                order.ItemCount,
                order.Id,
                // Include calculated fields if needed
                OrderMonth = order.OrderDate.Month,
                OrderYear = order.OrderDate.Year,
                IsRecent = order.OrderDate > DateTime.UtcNow.AddDays(-30)
            };

        // Add indexes for efficient searching/filtering
        Index(x => x.CustomerId, FieldIndexing.Exact);
        Index(x => x.Status, FieldIndexing.Exact);
        Index(x => x.OrderDate, FieldIndexing.Exact);
        Index(x => x.TotalAmount, FieldIndexing.Default);

        // Store fields for projection optimization
        Store(x => x.OrderDate, FieldStorage.Yes);
        Store(x => x.Status, FieldStorage.Yes);
        Store(x => x.TotalAmount, FieldStorage.Yes);
        Store(x => x.Currency, FieldStorage.Yes);
        Store(x => x.ItemCount, FieldStorage.Yes);
    }
}