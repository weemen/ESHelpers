using System;
using Xunit;

namespace ESHelpersTests.EventSourcing
{
    public class AggregateRootBaseTest
    {
        [Fact]
        public void it_can_have_uncommitted_events()
        {
            var fooId = Guid.NewGuid().ToString();
            var command = new FooCommand(fooId, "Foo", "hash", "encrypt");
            var aggregate = FooAggregate.InitiateFooAggregate(Guid.Parse(command.FooId), command.Name, command.Hashme,
                command.Encryptme);
            
            Assert.Single(aggregate.getUncommittedEvents());
            Assert.Equal(fooId, aggregate.getAggregateRootId().ToString());
            
        }
    }
}