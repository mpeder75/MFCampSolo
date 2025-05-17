namespace Order.Infrastructure.Exceptions;

public class InfrastructureException : Exception
{
    public InfrastructureException(string message) { }

    public InfrastructureException(string message, Exception innerException) 
        : base(message, innerException) { }
}