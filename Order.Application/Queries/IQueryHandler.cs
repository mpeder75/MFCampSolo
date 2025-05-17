namespace Order.Application.Queries;
/// <summary>
/// Interface hvis ansvar er at definere en generisk kontrakt for all QueryHandlers
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface IQueryHandler <TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}