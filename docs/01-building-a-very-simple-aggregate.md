# Building a very simple Aggregate
This tutorial is here to help you with modelling a very simple 
domain. This will show how these helpers are usefull and will
speed up development.

## Table of contents
- [Our simple User registration domain](our-simple-user-registration-domain)
- [Creating our first command](creating-our-first-command)
- [Creating our first event](creating-our-first-event)
- [Creating our AggregateRoot](creating-our-aggregate-root)
- [Connecting the dots](connecting-the-dots)

In the next tutorial we are going to improve the model because obviously
we can't leave personal data stored plain in our eventstore. 
We're going to tackle this by applying Crypto Schredding!

## Our simple User registration domain
In this tutorial we will start setting up a very simple User 
registration domain. This domain consists of a few events.

### UserRegistrationInitiated
This is the starting event, we this event we initiate the registration.
- UserRegistrationInitiated 
    - registrationId (Guid)
    - name (string)
    - email (string)
    - password (string)

### UserRegistrationConfirmed  
The end user will need to confirm his registration before it's 
will become active.
- UserRegistrationConfirmed
    - email (string)
    - confirmationString (string)

### UserUnregistered
The end user will have the ability to unregister him / her self.
With this event this will be possible.
- UserUnregistered
    - email (string)
    - confirmationString (string)

## Creating our first command
Time to create our first command, as a base rule I'll make them immutable at all times.

```c#
public class InitiateUserRegistration
    {
        private string _name;
        private string _email;
        private string _password;
        
        public InitiateRegistrationFyreUser(string name, string email, string password)
        {
            _name = name;
            _email = email;
            _password = password;
        }

        public string Name => _name;

        public string Email => _email;

        public string Password => _password;
    }
```

## Creating our first event
Creating an event is straight forward as well, the most important thing to 
know is to implement `IDomainEvent` interface for every event that you will create.

```c#
public class UserRegistrationInitiated: IDomainEvent
    {
        private string _userId;
        private string _name;
        private string _email;
        private string _password;
        
        public UserRegistrationInitiated(string userId, string name, string email, string password)
        {
            _userId = userId;
            _name = name;
            _email = email;
            _password = password;
        }
        
        public string UserId => _userId;

        public string Name => _name;

        public string Email => _email;

        public string Password => _password;
    }
```

## Creating our aggregate root
Since we have an command and an event it's time to create our aggregate.

There a a few important things:

1. Every aggregate should extend from `AggregateRootBase` which give you 
some niceties like tracking the uncommitted events, some magic for applying 
events and tracking aggregate versions for free.
2. For every event that we apply on the aggregate we will create apply 
method (see example below).
3. Constructors for the aggregate are static methods and will return the aggregate root.
4. Override the `getAggregateRootId()` to return the aggregate root id

Knowing this we will get something like this:

```c#

[Flags]
public enum RegistrationState
{
    PENDING,
    ACTIVE,
    DELETED,
}
    
public class User : AggregateRootBase
    {
        private Guid _userId;
        private string _name;
        private string _email;
        private string _password;
        private string _confirmationString;
        private RegistrationState _registrationState;

        public override Guid getAggregateRootId()
        {
            // return the aggregate root id
            return _userId;
        }

        public static User InitiateUserRegistration(Guid userId, string name, 
            string email, string password)
        {
            
            // create the aggregate
            var aggregate = new User();
            
            // record the event
            aggregate.Record(new UserRegistrationInitiated(userId.ToString(), name, email, password));
            
            // return the aggregate
            return aggregate;
        }

        // the apply method
        public void Apply(UserRegistrationInitiated e)
        {
            _userId = new Guid(e.UserId);
            _name = e.Name;
            _email = e.Email;
            _password = e.Password;
            _registrationState = RegistrationState.PENDING;
            _confirmationString = this.ComputeSha256Hash(name + email);
        }
        
        private string ComputeSha256Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        } 
    }
```

You now know that static method `InitiateUserRegistration` creates the aggregate,
you also know that if you want to record an event that you can use the `Record` method
of the aggregate. When events are recorded they are not stored yet! So let's connect
the last few dots to make this happen.

## Connecting the dots
To make storage and loading aggregates from the eventstore happen we will only need to
one thing... creating a repository. EShelpers gets shipped with a default repository to
make your life much easier. The repository requires a store as input this is the most 
difficult that we will do on this trip. For this example I choose to use [EventStoreDB](https://www.eventstore.com/eventstoredb).

### Setting up the EventStoreDB
The EventStoreDB support shipped with ESHelpers requires 3 things:
1. A connection to EventStoreDB
2. A stream name / category to write to
3. An event converter. The event converter will deal with more difficult things like:
    - Converting from Event object to JSON and back
    - Encrypting and Decrypting (if possible) values
    - Hashing values


#### Setting up the EventStoreDB connection
```c#
var streamCategory = Configuration["EventStore:UsersStreamCategory"]
var settings = ConnectionSettings.Create()
                .DisableTls()
                .Build();

var esHost = new IPEndPoint(IPAddress.Parse(Configuration["EventStore:Host"]), 
    int.Parse(Configuration["EventStore:Port"]));
var conn = EventStoreConnection.Create(settings, esHost);
conn.ConnectAsync().Wait();
```

#### Setting up the event converter
```c#
private EventConverter CreateEventConverter()
{
    // this a MySQL connection string to support Crypto Shredding
    // see next tutorial for more information about this.
    string cs = @$"server={Configuration["CryptoStore:Host"]};userid={Configuration["CryptoStore:Username"]};password={Configuration["CryptoStore:Password"]};database={Configuration["CryptoStore:Database"]}";
    
    return new EventConverter(
        // this is needed for json (de)serialization
        new JsonSerializer(
            new JsonSerializerSettingsFactory(
                new EncryptorDecryptor(
                    new CryptoRepository(new ESHelpers.Infratructure.Crypto.MySql(cs))
                )
            ),
            new List<Type>{ typeof(IDomainEvent)}
        ),
        // ClassMappingCache enables support a cross assemblies, you can just copy paste it
        ClassMappingCache.Instance
    );
}
```

#### Create the repository
```c#
public class EventStoreRepository<TAggregate> where TAggregate : IAggregateRoot
{
    private IEventStore _store;

    public EventStoreRepository(IEventStore store)
    {
        _store = store;
    }

    public void Save(TAggregate aggregateRoot)
    {
        var uncommittedEvents = aggregateRoot.getUncommittedEvents();
        var originalPlayHead = aggregateRoot.getPlayHead() - uncommittedEvents.Count;

        foreach (var domainEvent in uncommittedEvents)
        {
            _store.Store(domainEvent, aggregateRoot.getAggregateRootId(),originalPlayHead);
            originalPlayHead++;
            Console.WriteLine("playhead upped");
        }
    }

    public IAggregateRoot Load(Guid streamId)
    {
        Console.WriteLine($"[{streamId.ToString()}] Loading aggregate root");
        dynamic aggregate = Activator.CreateInstance<TAggregate>();
        var eventStream = this._store.LoadStream(streamId);
        foreach (var eventFromStream in eventStream)
        {
            aggregate.Apply(eventFromStream);
            aggregate.setExpectedPlayHead(aggregate.getPlayHead()+1);
        }
        return aggregate;
    }
}

...

var store = new EventStoreDb(conn, streamCategory, CreateEventConverter());
var eventsRepository = new EventStoreRepository<User>(store);
```

#### Saving and Loading with repository
```c#

public void InitiateUserRegistration(Guid userId, InitiateUserRegistration cmd)
{
    _repository.Save(
       User.InitiateUserRegistration(userId, cmd.Name, cmd.Email, cmd.Password)
    );
}
```

Loading from the repository is just as easy

```c#
public void ConfirmRegistration(Guid userId, ConfirmRegistration cmd)
{
    var aggregate = repository.Load(userId) as User;
    // this below is not created yet, you can do that yourself :)
    aggregate.ConfirmRegistration(cmd.ConfirmationString);
    repository.Save(aggregate);
}
```


