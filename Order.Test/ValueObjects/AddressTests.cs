using Order.Domain.ValueObjects;

namespace Order.Test.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_ValidAddress_ShouldSucceed()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "2800";
        var city = "Lyngby";

        // Act
        var address = Address.Create(street, zipCode, city);

        // Assert
        Assert.Equal(street, address.Street);
        Assert.Equal(zipCode, address.ZipCode);
        Assert.Equal(city, address.City);
    }

    [Fact]
    public void Create_EmptyStreet_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "";
        var zipCode = "2800";
        var city = "Lyngby";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("Street cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_EmptyZipCode_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "";
        var city = "Lyngby";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("Zip code cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_NonDigitZipCode_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "28AB";
        var city = "Lyngby";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("Danish zip code must be exactly 4 digits", exception.Message);
    }

    [Fact]
    public void Create_InvalidZipCodeLength_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "280";
        var city = "Lyngby";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("Danish zip code must be exactly 4 digits", exception.Message);
    }

    [Fact]
    public void Create_ZipCodeOutOfRange_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "0999";
        var city = "Lyngby";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("Invalid Danish zip code range", exception.Message);
    }

    [Fact]
    public void Create_EmptyCity_ShouldThrowArgumentException()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "2800";
        var city = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Address.Create(street, zipCode, city));
        Assert.Contains("City cannot be empty", exception.Message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var street = "Hovedgaden 1";
        var zipCode = "2800";
        var city = "Lyngby";
        var address = Address.Create(street, zipCode, city);

        // Act
        var result = address.ToString();

        // Assert
        Assert.Contains(street, result);
        Assert.Contains(zipCode, result);
        Assert.Contains(city, result);
    }
}