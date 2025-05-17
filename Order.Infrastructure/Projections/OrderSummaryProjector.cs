// Order.Infrastructure.Projections/OrderSummaryProjector.cs
using Microsoft.Extensions.Logging;
using Order.Application.ReadModels.ReadDto;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Events;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure.Projections;

public class OrderSummaryProjector : IProjection
{
    private readonly RavenDbContext _dbContext;
    private readonly ILogger<OrderSummaryProjector> _logger;

    public OrderSummaryProjector(RavenDbContext dbContext, ILogger<OrderSummaryProjector> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProjectAsync(DomainEvent @event)
    {
        try
        {
            switch (@event)
            {
                case OrderCreatedEvent e:
                    await HandleOrderCreatedAsync(e);
                    break;
                case OrderItemAddedEvent e:
                    await HandleOrderItemAddedAsync(e);
                    break;
                case OrderItemRemovedEvent e:
                    await HandleOrderItemRemovedAsync(e);
                    break;
                case OrderItemQuantityUpdatedEvent e:
                    await HandleOrderItemQuantityUpdatedAsync(e);
                    break;
                case OrderValidatedEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "Placed", e.ValidatedAt);
                    break;
                case OrderPaymentPendingEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "PaymentPending");
                    break;
                case OrderPaymentApprovedEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "PaymentApproved");
                    break;
                case OrderPaymentFailedEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "PaymentFailed");
                    break;
                case OrderProcessingStartedEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "Processing");
                    break;
                case OrderShippedEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "Shipped");
                    break;
                case OrderDeliveredEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "Delivered");
                    break;
                case OrderCancelledEvent e:
                    await HandleOrderStatusChangeAsync(e.OrderId, "Cancelled");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error projecting event {EventType} in OrderSummaryProjector", @event.GetType().Name);
            throw;
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();
        
        var summary = new OrderSummaryDto(
            @event.OrderId.Value,
            @event.CustomerId.Value,
            "Created",
            0m,
            @event.CreatedDate,
            0);

        await session.StoreAsync(summary, $"orders/{@event.OrderId.Value}");
        await session.SaveChangesAsync();
    }

    private async Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();
        
        var summaryId = $"orders/{@event.OrderId.Value}";
        var summary = await session.LoadAsync<OrderSummaryDto>(summaryId);
        if (summary != null)
        {
            var updatedSummary = summary with
            {
                TotalAmount = summary.TotalAmount + (@event.UnitPrice.Amount * @event.Quantity),
                ItemCount = summary.ItemCount + 1,
                CreatedDate = DateTime.UtcNow
            };
        
            await session.StoreAsync(updatedSummary, summaryId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderItemRemovedAsync(OrderItemRemovedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();
        
        var summaryId = $"orders/{@event.OrderId.Value}";
        var summary = await session.LoadAsync<OrderSummaryDto>(summaryId);
        if (summary != null)
        {
            var updatedSummary = summary with
            {
                ItemCount = Math.Max(0, summary.ItemCount - 1),
                CreatedDate = DateTime.UtcNow
            };
            
            await session.StoreAsync(updatedSummary, summaryId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderItemQuantityUpdatedAsync(OrderItemQuantityUpdatedEvent @event)
    {
        throw new NotImplementedException("Jeg har ikke tid");
    }

    private async Task HandleOrderStatusChangeAsync(OrderId orderId, string newStatus, DateTime? modifiedDate = null)
    {
        using var session = _dbContext.OpenAsyncSession();
        
        var summaryId = $"orders/{orderId.Value}";
        var summary = await session.LoadAsync<OrderSummaryDto>(summaryId);
        if (summary != null)
        {
            var updatedSummary = summary with
            {
                Status = newStatus,
                CreatedDate = modifiedDate ?? DateTime.UtcNow
            };
            
            await session.StoreAsync(updatedSummary, summaryId);
            await session.SaveChangesAsync(); }
    }
}