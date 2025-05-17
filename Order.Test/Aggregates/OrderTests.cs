using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.Exceptions;
using Order.Domain.ValueObjects;

namespace Order.Test.Aggregates;

public class OrderTests
{
    private readonly CustomerId _customerId;
    private readonly OrderId _orderId;

    public OrderTests()
    {
        _orderId = OrderId.CreateNew();
        _customerId = CustomerId.CreateNew();
    }

    [Fact]
    public void Constructor_ValidParameters_ShouldCreateOrder()
    {
        // Act
        var order = new Domain.Aggregates.Order(_orderId, _customerId);

        // Assert
        Assert.Equal(_orderId, order.Id);
        Assert.Equal(_customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(order.Items);
        Assert.NotEqual(default, order.OrderDate);
        Assert.Equal(order.OrderDate, order.LastModified);
    }

    [Fact]
    public void Constructor_NullOrderId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Domain.Aggregates.Order(null, _customerId));
    }

    [Fact]
    public void Constructor_NullCustomerId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Domain.Aggregates.Order(_orderId, null));
        Assert.Equal("customerId", exception.ParamName);
    }

    [Fact]
    public void AddItem_ValidParameters_ShouldAddItemToOrder()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var quantity = 5;
        var unitPrice = Money.Create(100m, "DKK");

        // Act
        order.AddItem(productId, productName, quantity, unitPrice);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(productId, order.Items[0].ProductId);
        Assert.Equal(productName, order.Items[0].ProductName);
        Assert.Equal(quantity, order.Items[0].Quantity);
        Assert.Equal(unitPrice, order.Items[0].UnitPrice);
        Assert.Equal(Money.Create(500m, "DKK").Amount, order.TotalAmount.Amount);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var quantity = 0;
        var unitPrice = Money.Create(100m, "DKK");

        // Act & Assert
        Assert.Throws<NegativeAmountException>(() =>
            order.AddItem(productId, productName, quantity, unitPrice));
    }

    [Fact]
    public void AddItem_ExistingProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var initialQuantity = 5;
        var additionalQuantity = 3;
        var unitPrice = Money.Create(100m, "DKK");

        // Act
        order.AddItem(productId, productName, initialQuantity, unitPrice);
        order.AddItem(productId, productName, additionalQuantity, unitPrice);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(initialQuantity + additionalQuantity, order.Items[0].Quantity);
        Assert.Equal(Money.Create(800m, "DKK").Amount, order.TotalAmount.Amount);
    }

    [Fact]
    public void SetShippingAddress_ValidAddress_ShouldSetAddress()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var address = Address.Create("Hovedgaden 1", "2800", "Lyngby");

        // Act
        order.SetShippingAddress(address);

        // Assert
        Assert.Equal(address, order.ShippingAddress);
    }

    [Fact]
    public void SetShippingAddress_NullAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => order.SetShippingAddress(null));
    }

    [Fact]
    public void ValidateOrder_EmptyOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => order.ValidateOrder());
        Assert.Contains("Cannot validate an order without items", exception.Message);
    }

    [Fact]
    public void ValidateOrder_NoShippingAddress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var quantity = 5;
        var unitPrice = Money.Create(100m, "DKK");
        order.AddItem(productId, productName, quantity, unitPrice);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => order.ValidateOrder());
        Assert.Contains("Shipping address must be set", exception.Message);
    }

    [Fact]
    public void ValidateOrder_LowOrderValue_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var quantity = 1;
        var unitPrice = Money.Create(40m, "DKK"); // Below minimum of 50 DKK
        order.AddItem(productId, productName, quantity, unitPrice);
        order.SetShippingAddress(Address.Create("Hovedgaden 1", "2800", "Lyngby"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => order.ValidateOrder());
        Assert.Contains("Order total must be at least 50", exception.Message);
    }

    [Fact]
    public void ValidateOrder_ValidOrder_ShouldChangeToPendingState()
    {
        // Arrange
        var order = new Domain.Aggregates.Order(_orderId, _customerId);
        var productId = ProductId.CreateNew();
        var productName = "Test Product";
        var quantity = 1;
        var unitPrice = Money.Create(100m, "DKK");
        order.AddItem(productId, productName, quantity, unitPrice);
        order.SetShippingAddress(Address.Create("Hovedgaden 1", "2800", "Lyngby"));

        // Act
        order.ValidateOrder();

        // Assert
        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public void Rehydrate_EventHistory_ShouldRecreateOrderState()
    {
        // Arrange
        var events = new List<DomainEvent>
        {
            new OrderCreatedEvent
            {
                OrderId = _orderId,
                CustomerId = _customerId,
                CreatedDate = DateTime.UtcNow
            },
            new OrderItemAddedEvent
            {
                OrderId = _orderId,
                ProductId = ProductId.CreateNew(),
                ProductName = "Test Product",
                Quantity = 2,
                UnitPrice = Money.Create(100m, "DKK")
            },
            new OrderShippingAddressSetEvent
            {
                OrderId = _orderId,
                Address = Address.Create("Hovedgaden 1", "2800", "Lyngby")
            },
            new OrderValidatedEvent
            {
                OrderId = _orderId,
                ValidatedAt = DateTime.UtcNow
            }
        };

        // Act
        var order = Domain.Aggregates.Order.Rehydrate(events);

        // Assert
        Assert.Equal(_orderId, order.Id);
        Assert.Equal(_customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(200m, order.TotalAmount.Amount);
        Assert.NotNull(order.ShippingAddress);
        Assert.Equal(4, order.Version);
    }
}