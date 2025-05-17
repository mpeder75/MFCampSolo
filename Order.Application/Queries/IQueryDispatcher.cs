namespace Order.Application.Queries;

public interface IQueryDispatcher {

    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query);
}