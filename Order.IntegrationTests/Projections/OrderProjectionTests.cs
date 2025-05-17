using FluentAssertions;
using Order.Application.ReadModels;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Events;
using Order.IntegrationTests.Infrastructure;

namespace Order.IntegrationTests.Projections;

public class OrderProjectionTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly OrderEventProjector _projector;

    public OrderProjectionTests(TestFixture fixture)
    {
        _fixture = fixture;
        _projector = fixture.GetService<OrderEventProjector>();
    }

    [Fact]
    public async Task ProjectOrderCreatedEvent_ShouldCreateReadModel()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid());
        var customerId = CustomerId.Create(Guid.NewGuid());
        var orderCreated = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        await _projector.ProjectAsync(orderCreated);

        // Assert - Undersøg at læsmodellen blev oprettet
        var orderQueries = _fixture.GetService<IOrderQueries>();
        var summary = await orderQueries.GetOrderSummaryAsync(orderId.Value);

        summary.Should().NotBeNull();
        summary.Id.Should().Be(orderId.Value);
        summary.CustomerId.Should().Be(customerId.Value);
        summary.Status.Should().Be("Created");
    }

    [Fact]
    public async Task ProjectOrderItemAddedEvent_ShouldUpdateReadModel()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid());
        var customerId = CustomerId.Create(Guid.NewGuid());
        var orderCreated = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            CreatedDate = DateTime.UtcNow
        };
        await _projector.ProjectAsync(orderCreated);

        var productId = ProductId.Create(Guid.NewGuid());
        var unitPrice = Money.Create(100m, "DKK");
        var orderItemAdded = new OrderItemAddedEvent
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = unitPrice
        };

        // Act
        await _projector.ProjectAsync(orderItemAdded);

        // Assert
        var orderQueries = _fixture.GetService<IOrderQueries>();
        var details = await orderQueries.GetOrderDetailsAsync(orderId.Value);

        details.Should().NotBeNull();
        details.Items.Should().HaveCount(1);
        details.TotalAmount.Should().Be(200m);
    }
}