using System.Collections.ObjectModel;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.Exceptions;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates;

/// <summary>
///     Denne class repræsentere en kundes ordre og fungere som aggregate root for order relateret funktionalitet
/// </summary>
/// <remarks>
///     Order aggregate håndtere vare, shipping address, status ændringer, business rules
/// </remarks>
public class Order : AggregateRoot
{
    // Encapsulation af OrderItems, eksterne kan se men ikke modificere
    private readonly List<OrderItem> _items;
    public ReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public int Version { get; private set; } = 1;
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? LastModified { get; private set; }
    public Address ShippingAddress { get; private set; }
    public string PaymentFailureReason { get; private set; }
    public Money TotalAmount => CalculateTotalAmount();


    private Order()
    {
        _items = new List<OrderItem>();
    }

    public Order(CustomerId customerId)
    {
        _items = new List<OrderItem>();
    
        if (customerId == null)
        {
            throw new ArgumentNullException(nameof(customerId));
        }

        Id = OrderId.CreateNew();
        CustomerId = customerId;

        Apply(new OrderCreatedEvent
        {
            OrderId = Id,
            CustomerId = customerId,
            CreatedDate = DateTime.UtcNow
        });
    }

    public Order(OrderId orderId, CustomerId customerId)
    {
        _items = new List<OrderItem>();

        
        if (orderId == null)
        {
            throw new ArgumentNullException(nameof(orderId));
        }

        if (customerId == null)
        {
            throw new ArgumentNullException(nameof(customerId));
        }

        Id = OrderId.CreateNew();
        CustomerId = customerId;
       
        Apply(new OrderCreatedEvent
        {
            OrderId = Id,
            CustomerId = customerId,
            CreatedDate = DateTime.UtcNow
        });
    }

    public void AddItem(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
        {
            throw new NegativeAmountException("Quantity must be greater than zero");
        }

        const int MaxDistinctProducts = 200;
        
        if (_items.Count >= MaxDistinctProducts)
        {
            throw new InvalidOperationException(
                $"Cannot add more than {MaxDistinctProducts} distinct products to an order.");
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId.Value.Equals(productId.Value));

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;

            if (newQuantity > OrderItem.MaxQuantityPerProduct)
            {
                throw new ArgumentException(
                    $"Cannot order more than {OrderItem.MaxQuantityPerProduct} units of a single product.");
            }

            // Consistent event creation - use object initializer syntax for all events
            Apply(new OrderItemQuantityUpdatedEvent
            {
                OrderId = Id,
                ProductId = productId,
                NewQuantity = newQuantity
            });
        }
        else
        {
            if (quantity > OrderItem.MaxQuantityPerProduct)
            {
                throw new ArgumentException(
                    $"Cannot order more than {OrderItem.MaxQuantityPerProduct} units of a single product.");
            }

            Apply(new OrderItemAddedEvent
            {
                OrderId = Id,
                ProductId = productId,
                ProductName = productName,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
        }
    }

    public void RemoveItem(ProductId productId)
    {
        EnsureOrderCanBeModified();

        var item = _items.FirstOrDefault(i => i.ProductId.Value == productId.Value);

        if (item != null)
        {
            Apply(new OrderItemRemovedEvent
            {
                OrderId = Id,
                ProductId = productId
            });
        }
    }

    public void SetShippingAddress(Address address)
    {
        if (address == null)
        {
            throw new ArgumentNullException(nameof(address));
        }

        Apply(new OrderShippingAddressSetEvent
        {
            OrderId = Id,
            Address = address
        });
    }

    /// <summary>
    ///     Validerer ordren før den markeres som placeret.
    /// </summary>
    public void ValidateOrder()
    {
        EnsureOrderCanBeModified();

        if (!_items.Any())
        {
            throw new InvalidOperationException("Cannot validate an order without items");
        }

        if (ShippingAddress == null)
        {
            throw new InvalidOperationException("Shipping address must be set before validating an order");
        }

        var minimumOrderValue = Money.Create(50, "DKK");
        if (TotalAmount.Amount < minimumOrderValue.Amount)
        {
            throw new InvalidOperationException(
                $"Order total must be at least {minimumOrderValue.Amount} {minimumOrderValue.Currency}");
        }

        Apply(new OrderValidatedEvent
        {
            OrderId = Id,
            ValidatedAt = DateTime.UtcNow
        });
    }

    public void MarkPaymentPending()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException("Order must be validated before processing payment");
        }

        Apply(new OrderPaymentPendingEvent
        {
            OrderId = Id
        });
    }

    public void MarkPaymentApproved()
    {
        if (Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOperationException("Cannot approve payment for an order that is not payment pending");
        }

        Apply(new OrderPaymentApprovedEvent
        {
            OrderId = Id
        });
    }

    /// <summary>
    ///     Markerer betalingen som fejlet og gemmer årsagen.
    /// </summary>
    public void MarkPaymentFailed(string reason)
    {
        if (Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOperationException("Can only mark payment as failed for orders with pending payment");
        }

        Apply(new OrderPaymentFailedEvent
        {
            OrderId = Id,
            Reason = reason ?? "Unknown"
        });
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.PaymentApproved)
        {
            throw new InvalidOperationException("Order must have payment approved before processing");
        }

        Apply(new OrderProcessingStartedEvent
        {
            OrderId = Id
        });
    }

    public void ProcessShippingStatusUpdate(string status, string trackingNumber = null)
    {
        if (Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException("Order must be in processing status before shipping");
        }

        Apply(new OrderShippedEvent
        {
            OrderId = Id,
            TrackingNumber = trackingNumber
        });
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOperationException("Order must be shipped before it can be delivered");
        }

        Apply(new OrderDeliveredEvent
        {
            OrderId = Id
        });
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Cannot cancel an order that has been shipped or delivered");
        }

        Apply(new OrderCancelledEvent
        {
            OrderId = Id,
            Reason = reason
        });
    }

    protected override void When(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                Id = e.OrderId;
                CustomerId = e.CustomerId;
                Status = OrderStatus.Created;
                OrderDate = e.CreatedDate;
                LastModified = e.CreatedDate;
                break;

            case OrderItemAddedEvent e:
                var orderItem = new OrderItem(e.ProductId, e.ProductName, e.Quantity, e.UnitPrice);
                _items.Add(orderItem);
                LastModified = DateTime.UtcNow;
                break;

            case OrderItemQuantityUpdatedEvent e:
                var existingItem = _items.FirstOrDefault(i => i.ProductId.Value.Equals(e.ProductId.Value));
                if (existingItem != null)
                {
                    existingItem.UpdateQuantity(e.NewQuantity);
                    LastModified = DateTime.UtcNow;
                }
                break;

            case OrderItemRemovedEvent e:
                var itemToRemove = _items.FirstOrDefault(i => i.ProductId.Value.Equals(e.ProductId.Value));
                if (itemToRemove != null)
                {
                    _items.Remove(itemToRemove);
                    LastModified = DateTime.UtcNow;
                }
                break;

            case OrderShippingAddressSetEvent e:
                ShippingAddress = e.Address;
                LastModified = DateTime.UtcNow;
                break;

            case OrderValidatedEvent e:
                Status = OrderStatus.Placed;
                LastModified = e.ValidatedAt;
                break;

            case OrderPaymentPendingEvent e:
                Status = OrderStatus.PaymentPending;
                LastModified = DateTime.UtcNow;
                break;

            case OrderPaymentApprovedEvent e:
                Status = OrderStatus.PaymentApproved;
                LastModified = DateTime.UtcNow;
                break;

            case OrderPaymentFailedEvent e:
                Status = OrderStatus.PaymentFailed;
                PaymentFailureReason = e.Reason;
                LastModified = DateTime.UtcNow;
                break;

            case OrderProcessingStartedEvent e:
                Status = OrderStatus.Processing;
                LastModified = DateTime.UtcNow;
                break;

            case OrderShippedEvent e:
                Status = OrderStatus.Shipped;
                LastModified = DateTime.UtcNow;
                break;

            case OrderDeliveredEvent e:
                Status = OrderStatus.Delivered;
                LastModified = DateTime.UtcNow;
                break;

            case OrderCancelledEvent e:
                Status = OrderStatus.Cancelled;
                LastModified = DateTime.UtcNow;
                break;
        }
    }

    protected override void EnsureValidState()
    {
        var valid = Id != null && CustomerId != null;

        // Specific invariants based on order status
        switch (Status)
        {
            case OrderStatus.Placed:
                valid = valid && _items.Any() && ShippingAddress != null && TotalAmount.Amount >= 50;
                break;

            case OrderStatus.PaymentFailed:
                valid = valid && !string.IsNullOrEmpty(PaymentFailureReason);
                break;

            case OrderStatus.Shipped:
            case OrderStatus.Delivered:
                valid = valid && _items.Any() && ShippingAddress != null;
                break;
        }

        if (!valid)
        {
            throw new InvalidOperationException($"Order with ID {Id} is in an invalid state.");
        }
    }

    private Money CalculateTotalAmount()
    {
        if (!_items.Any())
        {
            return Money.Zero("DKK"); // Default currency for Danish orders
        }

        var firstItem = _items.First();
        var total = firstItem.GetTotalPrice();

        foreach (var item in _items.Skip(1))
        {
            total = total.Add(item.GetTotalPrice());
        }

        return total;
    }

    private void EnsureOrderCanBeModified()
    {
        if (Status != OrderStatus.Created)
        {
            throw new InvalidOperationException("Order can only be modified when in Created status");
        }
    }

    public override object GetIdentity()
    {
        return Id;
    }

    /// <summary>
    /// Rehydrates et Order aggregate ud fra dets historik af domain events.
    /// </summary>
    /// <param name="events">En samling af domain events der repræsenterer Order'ens komplette historik</param>
    /// <returns>Et fuldt rekonstrueret Order aggregate med den korrekte tilstand</returns>
    /// <remarks>
    /// Denne metode implementerer Event Sourcing mønsteret ved at:
    /// 1. Oprette en tom Order ved hjælp af en privat constructor
    /// 2. Anvende hvert historisk event for at genopbygge aggregatets tilstand
    /// 3. Indstille version baseret på antallet af behandlede events
    /// 
    /// Til forskel fra normal event anvendelse opdaterer denne metode kun tilstand uden 
    /// at tilføje events til uncommitted events samlingen, da events  allerede er blevet gemt.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Kastes når events samlingen er null</exception>
    public static Order Rehydrate(IEnumerable<DomainEvent> events)
    {
        if (events == null)
        {
            throw new ArgumentException(nameof(events));
        }

        var rehydrateOrder = new Order();
        
        foreach (var singleEvent in events)
        {
            rehydrateOrder.When(singleEvent);
        }

        rehydrateOrder.Version = events.Count();

        return rehydrateOrder;
    }
}