using Order.Domain.Entities;
using Order.Domain.ValueObjects;

namespace Order.Test.Entities;

public class OrderItemTests
{
    private readonly ProductId _productId;
    private readonly string _productName;
    private readonly Money _unitPrice;

    public OrderItemTests()
    {
        _productId = ProductId.CreateNew();
        _productName = "Test Product";
        _unitPrice = Money.Create(100m, "DKK");
    }

    [Fact]
    public void Constructor_ValidParameters_ShouldCreateOrderItem()
    {
        // Arrange
        var quantity = 5;

        // Act
        var orderItem = new OrderItem(_productId, _productName, quantity, _unitPrice);

        // Assert
        Assert.Equal(_productId, orderItem.ProductId);
        Assert.Equal(_productName, orderItem.ProductName);
        Assert.Equal(quantity, orderItem.Quantity);
        Assert.Equal(_unitPrice, orderItem.UnitPrice);
    }

    [Fact]
    public void Constructor_NullProductId_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProductId nullProductId = null;
        var quantity = 5;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderItem(nullProductId, _productName, quantity, _unitPrice));
    }

    [Fact]
    public void Constructor_EmptyProductName_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyProductName = "";
        var quantity = 5;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentException>(() => new OrderItem(_productId, emptyProductName, quantity, _unitPrice));
        Assert.Contains("Product name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var zeroQuantity = 0;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentException>(() => new OrderItem(_productId, _productName, zeroQuantity, _unitPrice));
        Assert.Contains("Quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_NegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var negativeQuantity = -1;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentException>(() =>
                new OrderItem(_productId, _productName, negativeQuantity, _unitPrice));
        Assert.Contains("Quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_NullUnitPrice_ShouldThrowArgumentNullException()
    {
        // Arrange
        Money nullUnitPrice = null;
        var quantity = 5;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderItem(_productId, _productName, quantity, nullUnitPrice));
    }

    [Fact]
    public void GetTotalPrice_ShouldReturnCorrectResult()
    {
        // Arrange
        var quantity = 5;
        var orderItem = new OrderItem(_productId, _productName, quantity, _unitPrice);
        var expectedTotal = Money.Create(500m, "DKK");

        // Act
        var totalPrice = orderItem.GetTotalPrice();

        // Assert
        Assert.Equal(expectedTotal.Amount, totalPrice.Amount);
        Assert.Equal(expectedTotal.Currency, totalPrice.Currency);
    }

    [Fact]
    public void UpdateQuantity_ValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var initialQuantity = 5;
        var newQuantity = 10;
        var orderItem = new OrderItem(_productId, _productName, initialQuantity, _unitPrice);

        // Act
        orderItem.UpdateQuantity(newQuantity);

        // Assert
        Assert.Equal(newQuantity, orderItem.Quantity);
    }

    [Fact]
    public void UpdateQuantity_SameQuantity_ShouldNotChange()
    {
        // Arrange
        var quantity = 5;
        var orderItem = new OrderItem(_productId, _productName, quantity, _unitPrice);

        // Act
        orderItem.UpdateQuantity(quantity);

        // Assert
        Assert.Equal(quantity, orderItem.Quantity);
    }

    [Fact]
    public void UpdateQuantity_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var initialQuantity = 5;
        var newQuantity = 0;
        var orderItem = new OrderItem(_productId, _productName, initialQuantity, _unitPrice);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => orderItem.UpdateQuantity(newQuantity));
        Assert.Contains("Quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public void UpdateQuantity_ExceedMaxQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var initialQuantity = 5;
        var exceedMaxQuantity = OrderItem.MaxQuantityPerProduct + 1;
        var orderItem = new OrderItem(_productId, _productName, initialQuantity, _unitPrice);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => orderItem.UpdateQuantity(exceedMaxQuantity));
        Assert.Contains($"Quantity cannot exceed {OrderItem.MaxQuantityPerProduct}", exception.Message);
    }
}