using System;
using System.Collections.Generic;
using ESHelpers.Encryption;
using ESHelpers.EventSourcing;
using ESHelpers.Infratructure.Crypto;
using ESHelpers.Infratructure.Event;
using ESHelpers.Infratructure.Event.Exceptions;
using ESHelpers.Infratructure.Event.Helpers;
using Xunit;

namespace ESHelpersTests.Infrastructure.Event
{
    public class InMemoryEventStoreDbTest
    {
        [Fact]
        public void it_can_store_an_event()
        {
            var aggregateRootId = Guid.NewGuid();
            var evStore = new InMemoryEventStoreDb(GenerateEventConverter());
            var fooEvent = new FooEvent(Guid.NewGuid().ToString(), "Leon", "hashme", "encryptme");
            
            Assert.Empty(evStore.EventStore);
            evStore.Store(fooEvent, aggregateRootId, 1);
            Assert.Single(evStore.EventStore);
        }

        [Fact]
        public void it_can_load_an_event_from_store()
        {
            var aggregateRootId = Guid.NewGuid();
            var evStore = new InMemoryEventStoreDb(GenerateEventConverter());
            var fooEvent = new FooEvent(Guid.NewGuid().ToString(), "Leon", "hashme", "encryptme");
            evStore.Store(fooEvent, aggregateRootId, 1);
            Assert.Single(evStore.EventStore);

            var stream = evStore.LoadStream(aggregateRootId);
            Assert.Single(stream);
            Assert.Equal(typeof(FooEvent), stream[0].GetType());
        }

        [Fact]
        public void it_cannot_load_non_existing_stream()
        {
            var aggregateRootId = Guid.NewGuid();
            var evStore = new InMemoryEventStoreDb(GenerateEventConverter());
            Assert.Throws<EventStreamNotExistException>(() => evStore.LoadStream(aggregateRootId));
        }
        
        private EventConverter GenerateEventConverter()
        {
            return new EventConverter(
                new JsonSerializer(
                    new JsonSerializerSettingsFactory(
                        new EncryptorDecryptor(
                            new CryptoRepository(new ESHelpers.Infratructure.Crypto.InMemory())
                        )
                    ),
                    new List<Type>{ typeof(IDomainEvent)}
                )
            );
        }
    }
}