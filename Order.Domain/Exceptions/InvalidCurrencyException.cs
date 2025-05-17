namespace Order.Domain.Exceptions;

public class InvalidCurrencyException : Exception
{
    public InvalidCurrencyException(string message) : base(message) { }
}