namespace Order.Application.Queries;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query)
    {
        var handlerType = typeof(IQueryHandler<TQuery, TResult>);
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for query type {typeof(TQuery).Name}");
        }

        try
        {
            var method = handlerType.GetMethod("HandleAsync");
            var task = (Task<TResult>)method.Invoke(handler, new object?[] { query });
            return await task;
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                throw ex.InnerException;
            }

            throw;
        }
    }
}

