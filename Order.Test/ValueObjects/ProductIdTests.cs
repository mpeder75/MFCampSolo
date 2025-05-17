using Order.Domain.ValueObjects;
using Xunit;

namespace Order.Test.ValueObjects
{
    public class ProductIdTests
    {
        [Fact]
        public void Create_ValidGuid_ShouldSucceed()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var productId = ProductId.Create(guid);

            // Assert
            Assert.Equal(guid, productId.Value);
        }

        [Fact]
        public void Create_EmptyGuid_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ProductId.Create(emptyGuid));
            Assert.Contains("ProductId cannot be empty", exception.Message);
        }

        [Fact]
        public void CreateNew_ShouldReturnNewProductId()
        {
            // Act
            var productId = ProductId.CreateNew();

            // Assert
            Assert.NotEqual(Guid.Empty, productId.Value);
        }

        [Fact]
        public void ImplicitConversion_ShouldConvertToGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var productId = ProductId.Create(guid);

            // Act
            Guid result = productId;

            // Assert
            Assert.Equal(guid, result);
        }
    }

    public class CustomerIdTests
    {
        [Fact]
        public void Create_ValidGuid_ShouldSucceed()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var customerId = CustomerId.Create(guid);

            // Assert
            Assert.Equal(guid, customerId.Value);
        }

        [Fact]
        public void Create_EmptyGuid_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CustomerId.Create(emptyGuid));
            Assert.Contains("CustomerId cannot be empty", exception.Message);
        }

        [Fact]
        public void CreateNew_ShouldReturnNewCustomerId()
        {
            // Act
            var customerId = CustomerId.CreateNew();

            // Assert
            Assert.NotEqual(Guid.Empty, customerId.Value);
        }

        [Fact]
        public void ImplicitConversion_ShouldConvertToGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var customerId = CustomerId.Create(guid);

            // Act
            Guid result = customerId;

            // Assert
            Assert.Equal(guid, result);
        }
    }
}