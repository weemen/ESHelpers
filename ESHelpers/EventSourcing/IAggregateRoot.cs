using System;
using System.Collections.Generic;

namespace ESHelpers.EventSourcing
{
    public interface IAggregateRoot
    {
        Guid getAggregateRootId();
        List<IDomainEvent> getUncommittedEvents();
        int getPlayHead();

        public void setExpectedPlayHead(int expectedPlayHead);
    }
}