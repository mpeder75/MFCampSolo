using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public class OrderItemAddedEvent : DomainEvent
{
    [JsonPropertyName("orderId")] public OrderId OrderId { get; set; }

    [JsonPropertyName("productId")] public ProductId ProductId { get; set; }

    [JsonPropertyName("productName")] public string ProductName { get; set; }

    [JsonPropertyName("quantity")] public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")] public Money UnitPrice { get; set; }
}