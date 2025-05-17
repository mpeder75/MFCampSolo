using Order.Domain.ValueObjects;

namespace Order.Test.ValueObjects;

public class OrderIdTests
{
    [Fact]
    public void Create_ValidGuid_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var orderId = OrderId.Create(guid);

        // Assert
        Assert.Equal(guid, orderId.Value);
    }

    [Fact]
    public void Create_EmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => OrderId.Create(emptyGuid));
        Assert.Contains("OrderId cannot be empty", exception.Message);
    }

    [Fact]
    public void CreateNew_ShouldReturnNewOrderId()
    {
        // Act
        var orderId = OrderId.CreateNew();

        // Assert
        Assert.NotEqual(Guid.Empty, orderId.Value);
    }

    [Fact]
    public void ImplicitConversion_ShouldConvertToGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var orderId = OrderId.Create(guid);

        // Act
        Guid result = orderId;

        // Assert
        Assert.Equal(guid, result);
    }
}