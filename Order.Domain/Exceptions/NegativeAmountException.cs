namespace Order.Domain.Exceptions;

public class NegativeAmountException : Exception
{
    public NegativeAmountException(string message) : base(message) { }
}