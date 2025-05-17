using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Order.Domain.Events;

namespace Order.Infrastructure.Events
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonEventSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        }

        public string Serialize(DomainEvent @event)
        {
            return JsonSerializer.Serialize(@event, @event.GetType(), _options);
        }

        public DomainEvent Deserialize(string json, string typeName)
        {
            Type eventType = GetEventTypeByName(typeName);
            if (eventType == null)
            {
                throw new ArgumentException($"Event type '{typeName}' not found.");
            }

            return (DomainEvent)JsonSerializer.Deserialize(json, eventType, _options);
        }

        private Type GetEventTypeByName(string typeName)
        {
            // Find all types in the domain events namespace
            var eventTypes = Assembly.GetAssembly(typeof(DomainEvent))
                .GetTypes()
                .Where(t => typeof(DomainEvent).IsAssignableFrom(t) && !t.IsAbstract);

            // Look for exact match first
            var eventType = eventTypes.FirstOrDefault(t => t.Name == typeName);
            
            // If no exact match, try with Event suffix
            if (eventType == null && !typeName.EndsWith("Event"))
            {
                eventType = eventTypes.FirstOrDefault(t => t.Name == $"{typeName}Event");
            }

            return eventType;
        }
    }
}