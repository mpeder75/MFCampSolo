using FluentAssertions;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Events;
using Order.IntegrationTests.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.IntegrationTests.EventStore;

public class EventStoreIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public EventStoreIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveAndLoadEvents_ShouldWorkCorrectly()
    {
        // Arrange
        var eventStoreDb = _fixture.GetService<EventStoreDb>();
        var orderId = OrderId.Create(Guid.NewGuid());
        var customerId = CustomerId.Create(Guid.NewGuid());
        var orderCreated = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        await eventStoreDb.SaveEventAsync($"order-{orderId.Value}",
            new[] { orderCreated },
            -1);

        var events = await eventStoreDb.LoadEvents($"order-{orderId.Value}");

        // Assert
        events.Should().NotBeNull();
        events.Should().HaveCount(1);

        var @event = events.First() as OrderCreatedEvent;
        @event.Should().NotBeNull();
        @event!.OrderId.Value.Should().Be(orderId.Value);
        @event.CustomerId.Value.Should().Be(customerId.Value);
    }
}