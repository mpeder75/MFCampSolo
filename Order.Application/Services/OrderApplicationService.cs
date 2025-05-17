using Order.Application.Commands;
using Order.Application.Commands.CommandDto;
using Order.Application.Dispatchers;

namespace Order.Application.Services;

public class OrderApplicationService
{
    private readonly ICommandDispatcher _commandDispatcher;

    public OrderApplicationService(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
    }

    public async Task<CommandResult> CreateOrderAsync(CreateOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> AddOrderItemAsync(AddOrderItemCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> SetShippingAddressAsync(SetShippingAddressCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> ValidateOrderAsync(ValidateOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> UpdateOrderItemQuantityAsync(UpdateOrderItemQuantityCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> RemoveOrderItemAsync(RemoveOrderItemCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> MarkPaymentPendingAsync(MarkPaymentPendingCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> MarkPaymentApprovedAsync(MarkPaymentApprovedCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> MarkPaymentFailedAsync(MarkPaymentFailedCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> StartProcessingOrderAsync(StartProcessingOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> ShipOrderAsync(ShipOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> DeliverOrderAsync(DeliverOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }

    public async Task<CommandResult> CancelOrderAsync(CancelOrderCommand command)
    {
        return await _commandDispatcher.DispatchAsync(command);
    }
}