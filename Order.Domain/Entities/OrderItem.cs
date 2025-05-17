using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public class OrderItem : Entity
{
    private readonly Guid _id;
    public const int MaxQuantityPerProduct = 30;
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; }

    private OrderItem()
    {
        _id = Guid.NewGuid();
    }

    public OrderItem(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (productId == null)
        {
            throw new ArgumentNullException(nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name cannot be empty", nameof(productName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        if (unitPrice == null)
        {
            throw new ArgumentNullException(nameof(unitPrice));
        }

        _id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Money GetTotalPrice()
    {
        return UnitPrice.Multiply(Quantity);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity == Quantity)
        {
            return; 
        }
     
        if (newQuantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
        }

        if (newQuantity > MaxQuantityPerProduct)
        {
            throw new ArgumentException($"Quantity cannot exceed {MaxQuantityPerProduct}", nameof(newQuantity));
        }

        Quantity = newQuantity;
    }

    public override object GetIdentity()
    {
        return _id;
    }
}