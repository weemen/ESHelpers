using System;
using System.Collections.Generic;
using ESHelpers.EventSourcing;
using ESHelpers.Infratructure.Event.Helpers;
using EventStore.ClientAPI;

namespace ESHelpers.Infratructure.Event
{
    public class EventStoreDb : IEventStore
    {
        private IEventStoreConnection connection;
        private string streamCategory;
        private EventConverter _eventConverter;
        
        public EventStoreDb(IEventStoreConnection connection, string streamCategory, 
            EventConverter eventConverter)
        {
            this.connection = connection;
            this.streamCategory = streamCategory;
            this._eventConverter = eventConverter;
        }

        public void Store(IDomainEvent domainEvent, Guid aggregateRootId, int version)
        {
            this.connection.AppendToStreamAsync($"{this.streamCategory}-{aggregateRootId.ToString()}",
                version, _eventConverter.ToEventData(domainEvent)).Wait();
        }
        
        private IEnumerable<ResolvedEvent> LoadEvents(string streamId) {
            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice =
                    this.connection.ReadStreamEventsForwardAsync(
                        streamId.ToString(), 
                        nextSliceStart, 
                        200, 
                        false).Result;

                nextSliceStart = (int) currentSlice.NextEventNumber;

                foreach(var ev in currentSlice.Events) yield return ev;

            } while (!currentSlice.IsEndOfStream);
        }
        
        public List<IDomainEvent> LoadStream(Guid aggregateRootId)
        {
            var events = new List<IDomainEvent>();
            foreach (var eventFromStream in LoadEvents($"{this.streamCategory}-{aggregateRootId.ToString()}"))
            {
                dynamic loadedEvent = this._eventConverter.ToEvent(eventFromStream);
                events.Add(loadedEvent);
            }

            return events;
        }
    }
}