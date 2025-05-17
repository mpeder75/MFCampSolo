namespace Order.Domain.Exceptions;

public class ZeroMultiplierException : Exception
{
    public ZeroMultiplierException(string message) : base(message) { }
}