using System;
using ESHelpers.EventSourcing;

namespace ESHelpers.Infratructure.Event
{
    public interface IEventStoreRepository
    {
        public void Save(IAggregateRoot aggregateRoot);
        public IAggregateRoot Load(Guid streamId);
    }
}