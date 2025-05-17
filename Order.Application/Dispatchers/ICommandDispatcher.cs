using Order.Application.Commands;

namespace Order.Application.Dispatchers;

public interface ICommandDispatcher
{
    Task<CommandResult> DispatchAsync<TCommand>(TCommand command);
}