namespace Order.Domain.ValueObjects;

public record CustomerId
{
    public Guid Value { get; init; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));
        }
        Value = value;
    }

    //protected CustomerId() { }

    public static CustomerId Create(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty", nameof(id));
        }

        return new CustomerId(id);
    }

    public static CustomerId CreateNew()
    {
        return new CustomerId(Guid.NewGuid());
    }

    public static implicit operator Guid(CustomerId customerId)
    {
        return customerId?.Value ?? Guid.Empty;
    }
}