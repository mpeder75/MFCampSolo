namespace Order.Domain.ValueObjects;

public record ProductId
{
    public Guid Value { get; init; }

    private ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty", nameof(value));
        }
        Value = value;
    }

    //protected ProductId() { }

    public static ProductId Create(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty", nameof(id));
        }

        return new ProductId(id);
    }

    public static ProductId CreateNew()
    {
        return new ProductId(Guid.NewGuid());
    }

    public static implicit operator Guid(ProductId productId)
    {
        return productId.Value;
    }
}