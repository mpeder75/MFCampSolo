namespace Order.Application.Commands;

public class CommandResult
{
    public bool Success { get; }
    public string ErrorMessage { get; }
    public Exception Exception { get; }

    protected CommandResult(bool success, string errorMessage = null, Exception exception = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static CommandResult Ok()
    {
        return new CommandResult(true);
    }

    public static CommandResult Failure(string errorMessage, Exception exception = null)
    {
        return new CommandResult(false, errorMessage, exception);
    }
}

public class CommandResult<T> : CommandResult
{
    public T Data { get; }

    private CommandResult(bool success, T data, string errorMessage = null, Exception exception = null)
        : base(success, errorMessage, exception)
    {
        Data = data;
    }

    public static CommandResult<T> Ok(T data)
    {
        return new CommandResult<T>(true, data);
    }

    public new static CommandResult<T> Failure(string errorMessage, Exception exception = null)
    {
        return new CommandResult<T>(false, default, errorMessage, exception);
    }
}