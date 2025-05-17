using Order.Application.ReadModels.ReadDto;
using Raven.Client.Documents.Indexes;

namespace Order.Infrastructure.Indexes;

public class Orders_ByStatus : AbstractIndexCreationTask<OrderSummaryDto>
{
    public Orders_ByStatus()
    {
        Map = orders => from order in orders
            select new
            {
                order.Status,
                order.CreatedDate,
                order.CustomerId,
                order.TotalAmount,
                order.ItemCount,
                order.Id,
                // Include calculated fields for business analytics
                DaysSinceOrder = (DateTime.UtcNow - order.CreatedDate).Days,
                IsHighValue = order.TotalAmount > 500
            };

        // Add indexes for efficient filtering
        Index(x => x.Status, FieldIndexing.Exact);
        Index(x => x.CreatedDate, FieldIndexing.Default);
        Index(x => x.CustomerId, FieldIndexing.Exact);
        Index(x => x.TotalAmount, FieldIndexing.Default);
        
        // Store fields for projection optimization
        Store(x => x.Status, FieldStorage.Yes);
        Store(x => x.CreatedDate, FieldStorage.Yes);
        Store(x => x.TotalAmount, FieldStorage.Yes);
        Store(x => x.ItemCount, FieldStorage.Yes);
    }
}
    