using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderShippedEvent : DomainEvent
{
    

    [JsonPropertyName("orderId")]
    public OrderId OrderId { get; set; }
    [JsonPropertyName("trackingNumber")]
    public string TrackingNumber { get; set; }

    
    }
