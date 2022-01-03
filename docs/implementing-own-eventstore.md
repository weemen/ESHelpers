# Implementing your own eventstore
You problably need an eventstore but not want to work with EventStoreDB and you're
probably wondering: "Can I easily create my own store"?

The answer should be yes and this is what you do!

You can just create a new class and implement the `IEventStore` interface, which can be found [here](https://github.com/weemen/ESHelpers/blob/master/ESHelpers/Infratructure/Event/IEventStore.cs).

```c#
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
```

Just you implement this interface you can inject your store in the constructor of your repository class.  
If you run into any issues just leave a question on our github issue tracker and I will try to help you out!
