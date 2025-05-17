namespace Order.Application.Commands;

public interface ICommandHandler<TCommand>
{
    Task HandleAsync(TCommand command);
}