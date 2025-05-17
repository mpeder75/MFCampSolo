using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderProcessingStartedEvent : DomainEvent
{
    

    [JsonPropertyName("orderId")]
    public OrderId OrderId { get; set; }

   
}