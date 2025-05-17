using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderShippingAddressSetEvent : DomainEvent
{
    

    [JsonPropertyName("orderId")]
    public OrderId OrderId { get; set; }
    [JsonPropertyName("address")]
    public Address Address { get; set; }

    
}