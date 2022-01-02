using System;
using ESHelpers.EventSourcing;
using ESHelpers.Internal;
using EventStore.ClientAPI;

namespace ESHelpers.Infratructure.Event.Helpers
{
    public class EventConverter
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly ClassMappingCache _classMappingCache;

        public EventConverter(JsonSerializer jsonSerializer, ClassMappingCache classMappingCache)
        {
            _jsonSerializer = jsonSerializer;
            _classMappingCache = classMappingCache;
        }
        
        public IDomainEvent ToEvent(ResolvedEvent resolvedEvent)
        {
            var data = resolvedEvent.Event.Data;
            var metadata = resolvedEvent.Event.Metadata;
            var eventName = _classMappingCache.lookup(resolvedEvent.Event.EventType);
            var persistableEvent =(dynamic) _jsonSerializer.Deserialize(data, metadata, eventName);
            return persistableEvent;
        }
        
        public IDomainEvent ToEvent(EventData eventData)
        {
            var data = eventData.Data;
            var metadata = eventData.Metadata;
            var eventName = _classMappingCache.lookup(eventData.Type);
            var persistableEvent =(dynamic) _jsonSerializer.Deserialize(data, metadata, eventName);
            return persistableEvent;
        }

        public EventData ToEventData(IDomainEvent @event)
        {
            var eventTypeName = @event.GetType().Name;
            var id = Guid.NewGuid();
            var serializedEvent = _jsonSerializer.Serialize(@event);
            var data = serializedEvent.Data;
            var metadata = serializedEvent.MetaData;
            var eventData = new EventData(id, eventTypeName, serializedEvent.IsJson, data, metadata);
            return eventData;
        }
    }
}