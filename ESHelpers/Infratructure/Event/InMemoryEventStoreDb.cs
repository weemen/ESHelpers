using System;
using System.Collections.Generic;
using ESHelpers.EventSourcing;
using ESHelpers.Infratructure.Event.Exceptions;
using ESHelpers.Infratructure.Event.Helpers;
using EventStore.ClientAPI;

namespace ESHelpers.Infratructure.Event
{
    public class InMemoryEventStoreDb : IEventStore
    {
        private readonly Dictionary<Guid, List<EventData>> _store = new Dictionary<Guid, List<EventData>>();
        private readonly EventConverter _eventConverter;
        
        public InMemoryEventStoreDb(EventConverter eventConverter)
        {
            _eventConverter = eventConverter;
        }

        public Dictionary<Guid, List<EventData>> EventStore => _store;

        public void Store(IDomainEvent domainEvent, Guid aggregateRootId, int version)
        {
            var eventData = _eventConverter.ToEventData(domainEvent);
            var eventStream = new List<EventData>();
            _store.TryGetValue(aggregateRootId, out eventStream);
            if (eventStream is null)
            {
                eventStream = new List<EventData>();
            }
            eventStream.Add(eventData);
            if (!_store.TryAdd(aggregateRootId, eventStream))
            {
                throw new ApplicationException("Could not store eventdata in InMemoryStore");
            }
        }

        public List<IDomainEvent> LoadStream(Guid aggregateRootId)
        {
            var events = new List<IDomainEvent>();
            _store.TryGetValue(aggregateRootId, out var currentAggregate);
            if (currentAggregate == null)
            {
                throw new EventStreamNotExistException("Stream does not exist in InMemoryStore");
            }

            foreach (var eventData in currentAggregate)
            {
                dynamic loadedEvent = this._eventConverter.ToEvent(eventData);
                events.Add(loadedEvent);
            }

            return events;
        }
    }
}