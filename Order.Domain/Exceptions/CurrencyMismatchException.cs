namespace Order.Domain.Exceptions;

public class CurrencyMismatchException : Exception
{
    public CurrencyMismatchException(string message) : base(message) { }
}