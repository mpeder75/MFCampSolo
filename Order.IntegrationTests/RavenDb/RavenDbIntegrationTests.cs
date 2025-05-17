using FluentAssertions;
using Order.Application.ReadModels;
using Order.Application.ReadModels.ReadDto;
using Order.IntegrationTests.Infrastructure;
using Order.Infrastructure.Repositories;

namespace Order.IntegrationTests.RavenDb;

public class RavenDbIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IOrderQueries _orderQueries;

    public RavenDbIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _orderQueries = fixture.GetService<IOrderQueries>();
    }

    [Fact]
    public async Task QueryOrderSummary_WhenOrderExists_ReturnsCorrectOrder()
    {
        // Arrange - Indsæt test data direkte i RavenDB
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var session = _fixture.GetService<Order.Infrastructure.Repositories.RavenDbContext>().OpenAsyncSession())
        {
            var summary = new OrderSummaryDto(
                orderId,
                customerId,
                "Created",
                100m,
                DateTime.UtcNow,
                2);

            await session.StoreAsync(summary, $"orders/{orderId}");
            await session.SaveChangesAsync();
        }

        // Act
        var result = await _orderQueries.GetOrderSummaryAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be("Created");
    }
}