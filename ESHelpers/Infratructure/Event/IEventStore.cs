using System;
using System.Collections.Generic;
using ESHelpers.EventSourcing;

namespace ESHelpers.Infratructure.Event
{
    public interface IEventStore
    {
        public void Store(IDomainEvent domainEvent, Guid aggregateRootId, int version);
        public List<IDomainEvent> LoadStream(Guid aggregateRootId);

    }
}