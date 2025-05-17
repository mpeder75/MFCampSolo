using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Exceptions;

public class OrderConcurrencyException : InfrastructureException
{
    public OrderId OrderId { get; }
    public int ExpectedVersion { get; }
    
    public OrderConcurrencyException(string message, OrderId orderId, int expectedVersion) 
        : base(message)
    {
        OrderId = orderId;
        ExpectedVersion = expectedVersion;
    }
    
    public OrderConcurrencyException(string message, OrderId orderId, int expectedVersion, Exception innerException)
        : base(message, innerException)
    {
        OrderId = orderId;
        ExpectedVersion = expectedVersion;
    }
}