using Microsoft.Extensions.DependencyInjection;
using Order.Application.Commands;
using Order.Application.Commands.CommandDto;
using Order.Application.Dispatchers;
using Order.Application.Queries;
using Order.Application.Queries.Common;
using Order.Application.Queries.Handlers;
using Order.Application.Queries.QueryDto;
using Order.Application.ReadModels.ReadDto;
using Order.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<SetShippingAddressCommand>, SetShippingAddressCommandHandler>();
        services.AddScoped<ICommandHandler<ValidateOrderCommand>, ValidateOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CreateOrderCommand>, CreateOrderCommandHandler>();
        services.AddScoped<ICommandHandler<AddOrderItemCommand>, AddOrderItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOrderItemQuantityCommand>, UpdateOrderItemQuantityCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveOrderItemCommand>, RemoveOrderItemCommandHandler>();
        services.AddScoped<ICommandHandler<MarkPaymentPendingCommand>, MarkPaymentPendingCommandHandler>();
        services.AddScoped<ICommandHandler<MarkPaymentApprovedCommand>, MarkPaymentApprovedCommandHandler>();
        services.AddScoped<ICommandHandler<MarkPaymentFailedCommand>, MarkPaymentFailedCommandHandler>();
        services.AddScoped<ICommandHandler<StartProcessingOrderCommand>, StartProcessingOrderCommandHandler>();
        services.AddScoped<ICommandHandler<ShipOrderCommand>, ShipOrderCommandHandler>();
        services.AddScoped<ICommandHandler<DeliverOrderCommand>, DeliverOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelOrderCommand>, CancelOrderCommandHandler>();

        // Register command dispatcher
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Register application services
        services.AddScoped<OrderApplicationService>();

        // Register query dispatcher
        services.AddScoped<IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>, GetOrderDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerOrdersQuery, PagedResult<OrderHistoryDto>>, GetCustomerOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrderSummaryQuery, OrderSummaryDto>, GetOrderSummaryQueryHandler>();

        return services;
    }
}        
