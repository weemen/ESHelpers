using System;
using ESHelpers.EventSourcing;
using EventStore.ClientAPI;

namespace ESHelpers.Infratructure.Event.Helpers
{
    public class EventConverter
    {
        private readonly JsonSerializer _jsonSerializer;

        public EventConverter(JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }
    
        public IDomainEvent ToEvent(ResolvedEvent resolvedEvent)
        {
            var data = resolvedEvent.Event.Data;
            var metadata = resolvedEvent.Event.Metadata;
            var eventName = resolvedEvent.Event.EventType;
            var persistableEvent =(dynamic) _jsonSerializer.Deserialize(data, metadata, eventName);
            return persistableEvent;
        }

        public EventData ToEventData(IDomainEvent @event)
        {
            var eventTypeName = @event.GetType().FullName;
            var id = Guid.NewGuid();
            var serializedEvent = _jsonSerializer.Serialize(@event);
            var data = serializedEvent.Data;
            var metadata = serializedEvent.MetaData;
            var eventData = new EventData(id, eventTypeName, serializedEvent.IsJson, data, metadata);
            return eventData;
        }
    }
}