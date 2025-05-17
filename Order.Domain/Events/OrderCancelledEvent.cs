using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderCancelledEvent : DomainEvent
{
    [JsonPropertyName("orderId")] 
    public OrderId OrderId { get; set; }
    
    [JsonPropertyName("reason")] 
    public string Reason { get; set; }

    
}