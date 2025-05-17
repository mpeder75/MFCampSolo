using Microsoft.Extensions.DependencyInjection; 
using Order.Application.Commands;

namespace Order.Application.Dispatchers;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<CommandResult> DispatchAsync<TCommand>(TCommand command)
    {
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (handler == null)
        {
            return CommandResult.Failure($"No handler registered for command type {typeof(TCommand).Name}");
        }

        try
        {
            await handler.HandleAsync(command);
            return CommandResult.Ok();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, ex);
        }
    }
}