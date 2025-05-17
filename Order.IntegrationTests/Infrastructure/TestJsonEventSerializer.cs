// Order.IntegrationTests/Infrastructure/TestJsonEventSerializer.cs
using System;
using System.Text.Json;
using Order.Domain.Events;
using Order.Infrastructure.Events;

namespace Order.IntegrationTests.Infrastructure
{
    public class TestJsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;
        
        public TestJsonEventSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            // Tilføj vores specialiserede converters
            _options.Converters.Add(new OrderIdConverter());
            _options.Converters.Add(new CustomerIdConverter());
            _options.Converters.Add(new ProductIdConverter());
        }
        
        public string Serialize(DomainEvent @event)
        {
            return JsonSerializer.Serialize(@event, @event.GetType(), _options);
        }
        
        public DomainEvent Deserialize(string json, string typeName)
        {
            var eventType = Type.GetType(typeName);
            if (eventType == null)
            {
                // Forsøg at finde typen i det nuværende domæne
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    eventType = assembly.GetType(typeName);
                    if (eventType != null)
                        break;
                }
            }
            
            if (eventType == null)
                throw new Exception($"Cannot find event type '{typeName}'");
                
            return (DomainEvent)JsonSerializer.Deserialize(json, eventType, _options);
        }
    }
}