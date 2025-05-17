using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderCreatedEvent : DomainEvent
{
    [JsonPropertyName("orderId")] 
    public OrderId OrderId { get; set; }

    [JsonPropertyName("customerId")] 
    public CustomerId CustomerId { get; set; }

    [JsonPropertyName("createdDate")] 
    public DateTime CreatedDate { get; set; }

    
}