namespace Order.Domain.Events;

public interface IEventSerializer
{
    string Serialize(DomainEvent @event);
    DomainEvent Deserialize(string json, string typeName);
}