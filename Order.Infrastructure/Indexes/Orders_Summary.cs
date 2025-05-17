using Raven.Client.Documents.Indexes;
using Order.Application.ReadModels.ReadDto;

namespace Order.Infrastructure.Indexes;

public class Orders_Summary : AbstractIndexCreationTask<OrderSummaryDto>
{
    public Orders_Summary()
    {
        Map = orders => from order in orders
            select new
            {
                order.Id,
                order.CustomerId,
                order.Status,
                order.TotalAmount,
                order.CreatedDate,
                order.ItemCount
            };
    }
}