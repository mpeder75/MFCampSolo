using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderItemRemovedEvent : DomainEvent
{
    

    [JsonPropertyName("orderId")]
    public OrderId OrderId { get; set; }
    [JsonPropertyName("productId")]
    public ProductId ProductId { get; set; }

   
}