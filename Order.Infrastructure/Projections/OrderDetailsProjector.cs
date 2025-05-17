using Microsoft.Extensions.Logging;
using Order.Application.ReadModels.ReadDto;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure.Projections;

public class OrderDetailsProjector : IProjection
{
    private readonly RavenDbContext _dbContext;
    private readonly ILogger<OrderDetailsProjector> _logger;

    public OrderDetailsProjector(RavenDbContext dbContext, ILogger<OrderDetailsProjector> logger)
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
                case OrderShippingAddressSetEvent e:
                    await HandleOrderShippingAddressSetAsync(e);
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
                    await HandleOrderPaymentFailedAsync(e);
                    break;
                case OrderShippedEvent e:
                    await HandleOrderShippedAsync(e);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error projecting event {EventType} in OrderDetailsProjector", @event.GetType().Name);
            throw;
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

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
        await session.SaveChangesAsync();
    }

    private async Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            // Create a new list with all existing items plus the new one
            var updatedItems = details.Items.ToList();
            updatedItems.Add(new OrderItemDto(
                @event.ProductId.Value,
                @event.ProductName,
                @event.Quantity,
                @event.UnitPrice.Amount,
                @event.UnitPrice.Currency
            ));

            var updatedDetails = details with
            {
                Items = updatedItems,
                TotalAmount = details.TotalAmount + @event.UnitPrice.Amount * @event.Quantity,
                CreatedDate = DateTime.UtcNow
            };

            await session.StoreAsync(updatedDetails, detailsId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderItemRemovedAsync(OrderItemRemovedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            // Find the item to remove
            var item = details.Items.FirstOrDefault(i => i.ProductId == @event.ProductId.Value);
            if (item != null)
            {
                var updatedItems = details.Items.Where(i => i.ProductId != @event.ProductId.Value).ToList();
                var updatedDetails = details with
                {
                    Items = updatedItems,
                    TotalAmount = details.TotalAmount - item.UnitPrice * item.Quantity,
                    CreatedDate = DateTime.UtcNow
                };

                await session.StoreAsync(updatedDetails, detailsId);
                await session.SaveChangesAsync();
            }
        }
    }

    private async Task HandleOrderShippingAddressSetAsync(OrderShippingAddressSetEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            var address = @event.Address;
            var addressDto = new AddressDto(
                address.Street,
                address.ZipCode,
                address.City
                );

            var updatedDetails = details with
            {
                ShippingAddress = addressDto,
                CreatedDate = DateTime.UtcNow
            };

            await session.StoreAsync(updatedDetails, detailsId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderPaymentFailedAsync(OrderPaymentFailedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            var updatedDetails = details with
            {
                Status = "PaymentFailed",
                PaymentFailureReason = @event.Reason,
                CreatedDate = DateTime.UtcNow
            };

            await session.StoreAsync(updatedDetails, detailsId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderShippedAsync(OrderShippedEvent @event)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{@event.OrderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            var updatedDetails = details with
            {
                Status = "Shipped",
                TrackingNumber = @event.TrackingNumber,
                CreatedDate = DateTime.UtcNow
            };

            await session.StoreAsync(updatedDetails, detailsId);
            await session.SaveChangesAsync();
        }
    }

    private async Task HandleOrderStatusChangeAsync(OrderId orderId, string newStatus, DateTime? modifiedDate = null)
    {
        using var session = _dbContext.OpenAsyncSession();

        var detailsId = $"orders/{orderId.Value}/details";
        var details = await session.LoadAsync<OrderDetailsDto>(detailsId);
        if (details != null)
        {
            var updatedDetails = details with
            {
                Status = newStatus,
                CreatedDate = modifiedDate ?? DateTime.UtcNow
            };

            await session.StoreAsync(updatedDetails, detailsId);
            await session.SaveChangesAsync();
        }
    }
}