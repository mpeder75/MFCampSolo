using Order.Domain.ValueObjects;
using Order.Domain.Exceptions;
using Xunit;

namespace Order.Test.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ValidMoney_ShouldSucceed()
    {
        // Arrange
        decimal amount = 100m;
        string currency = "DKK";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Create_NegativeAmount_ShouldThrowNegativeAmountException()
    {
        // Arrange
        decimal amount = -100m;
        string currency = "DKK";

        // Act & Assert
        Assert.Throws<NegativeAmountException>(() => Money.Create(amount, currency));
    }

    [Fact]
    public void Create_EmptyCurrency_ShouldThrowInvalidCurrencyException()
    {
        // Arrange
        decimal amount = 100m;
        string currency = "";

        // Act & Assert
        Assert.Throws<InvalidCurrencyException>(() => Money.Create(amount, currency));
    }

    [Fact]
    public void Create_NullCurrency_ShouldThrowInvalidCurrencyException()
    {
        // Arrange
        decimal amount = 100m;
        string currency = null!;

        // Act & Assert
        Assert.Throws<InvalidCurrencyException>(() => Money.Create(amount, currency));
    }

    [Fact]
    public void Add_SameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100m, "DKK");
        var money2 = Money.Create(50m, "DKK");

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150m, result.Amount);
        Assert.Equal("DKK", result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrowCurrencyMismatchException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "DKK");

        // Act & Assert
        Assert.Throws<CurrencyMismatchException>(() => money1.Add(money2));
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100m, "DKK");
        var money2 = Money.Create(50m, "DKK");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(50m, result.Amount);
        Assert.Equal("DKK", result.Currency);
    }

    [Fact]
    public void Subtract_DifferentCurrencies_ShouldThrowCurrencyMismatchException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "DKK");

        // Act & Assert
        Assert.Throws<CurrencyMismatchException>(() => money1.Subtract(money2));
    }

    [Fact]
    public void Multiply_PositiveMultiplier_ShouldSucceed()
    {
        // Arrange
        var money = Money.Create(100m, "DKK");

        // Act
        var result = money.Multiply(2);

        // Assert
        Assert.Equal(200m, result.Amount);
        Assert.Equal("DKK", result.Currency);
    }

    [Fact]
    public void Multiply_ZeroMultiplier_ShouldThrowZeroMultiplierException()
    {
        // Arrange
        var money = Money.Create(100m, "DKK");

        // Act & Assert
        Assert.Throws<ZeroMultiplierException>(() => money.Multiply(0));
    }

    [Fact]
    public void Multiply_NegativeMultiplier_ShouldThrowNegativeAmountException()
    {
        // Arrange
        var money = Money.Create(100m, "DKK");

        // Act & Assert
        Assert.Throws<NegativeAmountException>(() => money.Multiply(-1));
    }

    [Fact]
    public void Create_LargeAmount_ShouldSucceed()
    {
        // Arrange
        decimal amount = decimal.MaxValue;
        string currency = "DKK";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Create_SmallAmount_ShouldSucceed()
    {
        // Arrange
        decimal amount = 0.01m;
        string currency = "DKK";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }
}