namespace Order.Domain.ValueObjects;

public record OrderId
{
    public Guid Value { get; init; }

    private OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OrderId cannot be empty", nameof(value));
        }
        Value = value;
    }

    public static OrderId Create(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("OrderId cannot be empty", nameof(id));
        }

        return new OrderId(id);
    }

    public static OrderId CreateNew()
    {
        return new OrderId(Guid.NewGuid());
    }

    public static implicit operator Guid(OrderId orderId)
    {
        return orderId.Value;
    }
}