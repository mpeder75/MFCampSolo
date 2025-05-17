using Order.Application.ReadModels.ReadDto;
using Order.Domain.Events;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure.Events;

public class OrderEventProjector
{
    private readonly RavenDbContext _dbContext;

    public OrderEventProjector(RavenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ProjectAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                await HandleOrderCreatedAsync(e);
                break;
            case OrderItemAddedEvent e:
                await HandleOrderItemAddedAsync(e);
                break;
            // Håndter andre event typer...
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        // Opret OrderSummaryDto
        var summary = new OrderSummaryDto(
            @event.OrderId.Value,
            @event.CustomerId.Value,
            "Created",
            0m,
            @event.CreatedDate,
            0);

        await session.StoreAsync(summary, $"orders/{@event.OrderId.Value}");

        // Opret OrderDetailsDto
        var details = new OrderDetailsDto(
            @event.OrderId.Value,
            @event.CustomerId.Value,
            "Created",
            0m,
            @event.CreatedDate,
            null,
            new List<OrderItemDto>(),
            "None",
            null);

        await session.StoreAsync(details, $"orders/{@event.OrderId.Value}/details");

        // Gem begge ændringer i én transaktion
        await session.SaveChangesAsync();
    }

    private async Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();
    
        // Opdater OrderSummaryDto
        var summaryId = $"orders/{@event.OrderId.Value}";
        var summary = await session.LoadAsync<OrderSummaryDto>(summaryId);
        if (summary != null)
        {
            // Lav en kopi med opdaterede værdier
            var updatedSummary = summary with
            {
                TotalAmount = summary.TotalAmount + (@event.UnitPrice.Amount * @event.Quantity),
                ItemCount = summary.ItemCount + 1
            };
        
            // Fjern det gamle objekt fra tracking og tilføj det nye
            session.Advanced.Evict(summary);
            await session.StoreAsync(updatedSummary, summaryId);
        }
    
        // Opdater OrderDetailsDto
        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            // Opret en ny liste med det tilføjede item
            var updatedItems = details.Items.ToList();
            updatedItems.Add(new OrderItemDto(
                @event.ProductId.Value,
                @event.ProductName,
                @event.Quantity,
                @event.UnitPrice.Amount,
                @event.UnitPrice.Currency
            ));
        
            // Lav en kopi med opdaterede værdier
            var updatedDetails = details with
            {
                Items = updatedItems,
                TotalAmount = details.TotalAmount + (@event.UnitPrice.Amount * @event.Quantity)
            };
        
            // Fjern det gamle objekt fra tracking og tilføj det nye
            session.Advanced.Evict(details);
            await session.StoreAsync(updatedDetails, detailsId);
        }
    
        await session.SaveChangesAsync();
    }
}