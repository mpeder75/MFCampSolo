using Order.Application.Commands.CommandDto;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands
{
    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(CreateOrderCommand command)
        {
            // Opret CustomerId fra kommandoen
            var customerId = CustomerId.Create(command.CustomerId);

            // Opret en ny ordre - OrderId genereres internt i domænelaget
            var order = new Domain.Aggregates.Order(customerId);

            // Gem ordren i repository
            await _orderRepository.SaveAsync(order);
        }
    }

    public class AddOrderItemCommandHandler : ICommandHandler<AddOrderItemCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public AddOrderItemCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(AddOrderItemCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            var productId = ProductId.Create(command.ProductId);
            var unitPrice = Money.Create(command.UnitPrice, command.Currency);
            
            order.AddItem(productId, command.ProductName, command.Quantity, unitPrice);
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class SetShippingAddressCommandHandler : ICommandHandler<SetShippingAddressCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public SetShippingAddressCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(SetShippingAddressCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            var address = Address.Create(command.Street, command.ZipCode, command.City);
            
            order.SetShippingAddress(address);
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class ValidateOrderCommandHandler : ICommandHandler<ValidateOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public ValidateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository 
                               ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(ValidateOrderCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.ValidateOrder();
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class UpdateOrderItemQuantityCommandHandler : ICommandHandler<UpdateOrderItemQuantityCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderItemQuantityCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(UpdateOrderItemQuantityCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            var productId = ProductId.Create(command.ProductId);
            
            // Find og opdater vare-mængden
            // Vi bruger OrderItem's UpdateQuantity metode gennem domain metoder
            // Dette håndteres af domain-laget gennem events
            
            // Direkte opdatering er ikke mulig her, da Items er en ReadOnly collection
            // og item.UpdateQuantity() er private. Vi må bruge domæne-metoder i stedet.
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class RemoveOrderItemCommandHandler : ICommandHandler<RemoveOrderItemCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public RemoveOrderItemCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(RemoveOrderItemCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            var productId = ProductId.Create(command.ProductId);
            
            order.RemoveItem(productId);
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class MarkPaymentPendingCommandHandler : ICommandHandler<MarkPaymentPendingCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public MarkPaymentPendingCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(MarkPaymentPendingCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.MarkPaymentPending();
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class MarkPaymentApprovedCommandHandler : ICommandHandler<MarkPaymentApprovedCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public MarkPaymentApprovedCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(MarkPaymentApprovedCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.MarkPaymentApproved();
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class MarkPaymentFailedCommandHandler : ICommandHandler<MarkPaymentFailedCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public MarkPaymentFailedCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(MarkPaymentFailedCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.MarkPaymentFailed(command.Reason);
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class StartProcessingOrderCommandHandler : ICommandHandler<StartProcessingOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public StartProcessingOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(StartProcessingOrderCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.StartProcessing();
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public ShipOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(ShipOrderCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            // Status og trackingNumber sendes til ProcessShippingStatusUpdate
            order.ProcessShippingStatusUpdate("Shipped", command.TrackingNumber);
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class DeliverOrderCommandHandler : ICommandHandler<DeliverOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public DeliverOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(DeliverOrderCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.MarkAsDelivered();
            
            await _orderRepository.SaveAsync(order);
        }
    }

    public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public CancelOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task HandleAsync(CancelOrderCommand command)
        {
            var orderId = OrderId.Create(command.OrderId);
            var order = await _orderRepository.LoadAsync(orderId);
            
            order.Cancel(command.Reason);
            
            await _orderRepository.SaveAsync(order);
        }
    }
}