// Order.IntegrationTests/Infrastructure/ValueObjectJsonConverters.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Domain.ValueObjects;

namespace Order.IntegrationTests.Infrastructure
{
    public class ValueObjectConverter<T> : JsonConverter<T> where T : class
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(OrderId) == typeToConvert ||
                   typeof(CustomerId) == typeToConvert ||
                   typeof(ProductId) == typeToConvert;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                if (doc.RootElement.TryGetProperty("Value", out var valueProperty))
                {
                    var guidValue = valueProperty.GetGuid();
                    
                    if (typeToConvert == typeof(OrderId))
                        return OrderId.Create(guidValue) as T;
                    else if (typeToConvert == typeof(CustomerId))
                        return CustomerId.Create(guidValue) as T;
                    else if (typeToConvert == typeof(ProductId))
                        return ProductId.Create(guidValue) as T;
                }
            }
            
            throw new JsonException($"Cannot deserialize {typeof(T).Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Value Objects implementerer generelt en Value egenskab
            var propertyInfo = value.GetType().GetProperty("Value");
            
            writer.WriteStartObject();
            
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(value);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, propertyValue, options);
            }
            
            writer.WriteEndObject();
        }
    }

    public class OrderIdConverter : ValueObjectConverter<OrderId> { }
    public class CustomerIdConverter : ValueObjectConverter<CustomerId> { }
    public class ProductIdConverter : ValueObjectConverter<ProductId> { }
}