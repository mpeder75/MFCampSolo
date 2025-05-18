using System.Text.Json;
using Microsoft.Extensions.Logging;
using Order.Infrastructure.Outbox;

namespace Order.Infrastructure.Events;

public class DomainEventOutboxHandler
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<DomainEventOutboxHandler> _logger;
    private readonly IOutboxService _outboxService;

    public DomainEventOutboxHandler(IOutboxService outboxService, ILogger<DomainEventOutboxHandler> logger)
    {
        _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task HandleDomainEventsAsync(Domain.Aggregates.Order order)
    {
        var events = order.GetUncommittedEvents().ToList();

        if (!events.Any())
        {
            return;
        }

        _logger.LogInformation("Handling {Count} domain events for order {OrderId}",
            events.Count, order.Id);

        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions);
            var aggregateId = order.Id.ToString();

            await _outboxService.SaveMessageAsync(aggregateId, eventType, payload);
        }
    }
}