using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Events;
using Order.Infrastructure.Exceptions;

namespace Order.Infrastructure.Repositories;

/// <summary>
/// Repository til at persistere og genindlæse Order aggregates ved hjælp af Event Sourcing.
/// </summary>
/// <remarks>
/// Denne implementering bruger EventStoreDb til at gemme domain events og rekonstruere
/// aggregater fra deres event-historik, hvilket følger Event Sourcing mønsteret.
/// </remarks>
public class OrderRepository : IOrderRepository
{
    private readonly EventStoreDb _eventStoreDb;
    private readonly DomainEventOutboxHandler _outboxHandler;
    
    public OrderRepository(EventStoreDb eventStoreDb, DomainEventOutboxHandler outboxHandler)
    {
        _eventStoreDb = eventStoreDb;
        _outboxHandler = outboxHandler;
    }

   public async Task<Domain.Aggregates.Order> LoadAsync(OrderId orderId)
    {
        var streamName = GetStreamName(orderId);
        var events = await _eventStoreDb.LoadEvents(streamName);

        var order = Domain.Aggregates.Order.Rehydrate(events);
        return order;
    }

    public async Task SaveAsync(Domain.Aggregates.Order order)
    {
        var streamName = GetStreamName(order.Id);
        var expectedVersion = order.Version - 1; 
            
        try
        {
            // Save events to event store
            await _eventStoreDb.SaveEventAsync(streamName, order.GetUncommittedEvents(), expectedVersion);
                
            // Save events to outbox for external publishing
            await _outboxHandler.HandleDomainEventsAsync(order);

            // Clear domain events after successful saving
            order.ClearDomainEvents();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency conflict"))
        {
            throw new OrderConcurrencyException(
                "Competing changes in order. Please reload the order and try again.",
                order.Id, 
                expectedVersion,
                ex);
        }
    }

    private string GetStreamName(OrderId orderId)
    {
        return $"order-{orderId}";
    }
}