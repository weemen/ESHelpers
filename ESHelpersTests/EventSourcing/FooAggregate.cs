using System;
using ESHelpers.EventSourcing;
using ESHelpersTests.Infrastructure.Event;

namespace ESHelpersTests.EventSourcing
{
    public class FooAggregate : AggregateRootBase
    {
        private Guid _fooId;
        
        public override Guid getAggregateRootId()
        {
            return _fooId;
        }

        public static FooAggregate InitiateFooAggregate(Guid fooId, string name, string hashme, string encryptme)
        {
            var aggregate = new FooAggregate();
            aggregate.Record(new FooEvent(fooId.ToString(), name, hashme, encryptme));
            return aggregate;
        }

        public void Apply(FooEvent e)
        {
            _fooId = Guid.Parse(e.Id);
        }
    }
}