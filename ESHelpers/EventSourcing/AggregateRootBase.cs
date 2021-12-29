using System;
using System.Collections.Generic;
using System.Reflection;

namespace ESHelpers.EventSourcing
{
    public abstract class AggregateRootBase : IAggregateRoot
    {
        protected List<IDomainEvent> uncommittedEvents = new List<IDomainEvent>();
        protected int playHead = -1;
        
        public abstract Guid getAggregateRootId();

        public List<IDomainEvent> getUncommittedEvents()
        {
            return uncommittedEvents;
        }

        public int getPlayHead()
        {
            return this.playHead;
        }

        public void setExpectedPlayHead(int expectedPlayHead)
        {
            this.playHead = expectedPlayHead;
        }
        
        protected void Record(IDomainEvent e)
        {
            this.uncommittedEvents.Add(e);
            var methodInfo = HasOverloadForArgument(e);
            methodInfo?.Invoke(this, new[] { e });
            this.playHead++;
        }
        
        protected MethodInfo? HasOverloadForArgument(IDomainEvent e)
        {
            return GetType().GetMethod(name: "Apply", types: new[] { e.GetType() });
        }

    }
}