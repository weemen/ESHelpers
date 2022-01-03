# Dealing with sensitive information: Encryption & Hashing
In the previous tutorial we made the start of a User domain, however if you
bring that "domain" in production you will problems with passwords stored
in plaintext and failing to adhere GDPR compliancy.

In this tutorial we are going to look more closely on those problems and
implement crypto shredding to mitigate GDPR problems.

## What is crypto shredding?
You might be wondering what crypto actually is. In short it means that 
encrypt `Person X` and store the encryption key where `Person X` 
is encrypted with in a crypto keystore. A `crypto keystore` can be anything.
ESHelpers is shipped with an InMemory keystore and MySQL. 

Decrypting `Person X` will work great untill the encryption is deleted
from the `crypto keystore`. `Person X` can never be decrypted and therefore
directly anonymized forever. The upside with the approach is that we will 
never have to delete events related to this person and therfore and it's
actions can still be used business analytics.

## Applying Cryptoshredding
In the previous tutorial we already did the setup events converter 
with the crypto but we didn't use any feature of it so it was a bit
pointless but tha'ts going to change now.

### Setting up the events converter & the repository for the aggregate
Let's revisit some basics: setting up the repository:

Setting up the events converter:
``` c#
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

Setting up the Repository:
```c#
var streamCategory = Configuration["EventStore:UsersStreamCategory"]
var settings = ConnectionSettings.Create()
                .DisableTls()
                .Build();

var esHost = new IPEndPoint(IPAddress.Parse(Configuration["EventStore:Host"]), 
    int.Parse(Configuration["EventStore:Port"]));
var conn = EventStoreConnection.Create(settings, esHost);
conn.ConnectAsync().Wait();

var store = new EventStoreDb(conn, streamCategory, CreateEventConverter());
var eventsRepository = new EventStoreRepository<User>(store);
```

Setting up the MySQL crypto store:
```
SET NAMES utf8mb4;
DROP TABLE IF EXISTS `keystore`;

CREATE TABLE `keystore` (
  `identifier` varchar(36) NOT NULL,
  `key` blob NOT NULL,
  `vector` blob NOT NULL,
  PRIMARY KEY (`identifier`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
```
This will create a table `keystore` where we will store all keys and 
it's vector. The identifier field will we be used to retrieve the keys
and it's vector.

### Updating the event
This is the original event before we update it:

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

When applying encryption on fields in events, we will need to do 2 things:
1. Identify which field holds the key for the crypto keystore. This will
be done attribute `[DataSubjectId]`.
2. Identify which fields must be encrypted. This is done with the attribute `[PersonalData]`

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
        
        [DataSubjectId]
        public string UserId => _userId;

        [PersonalData]
        public string Name => _name;

        [PersonalData]
        public string Email => _email;

        public string Password => _password;
    }
```

So the net difference with the previous tutorials is that we add those
add attributes to our event.

The output in the eventstore might look this
```json
Data
{
  "userId": "08073dcc-1113-42a0-a5aa-63214b342cef",
  "name": "crypto.kIHrtWlanPn2ef+aGNxV5w==",
  "email": "crypto.r/hoefWUpkqJYMbbsKo1Vx8717NvVGWaRKUJe58Lsao=",
  "password": "some_random_password"
}
				
Metadata
{
  "dataSubjectId": "08073dcc-1113-42a0-a5aa-63214b342cef"
}
```

> NOTE: To be clear, encryption will be play a field level and not on 
> event level. If you need to encrypt full events than this might not
> be solution for the more you need to encrypt and especially decrypt
> the slower your system will be!!

### Deleting the key from the store
If we would delete `08073dcc-1113-42a0-a5aa-63214b342cefz` from the crypto
keystore then everytime when this event will be deserialized the encrypted
field values will converted to `***`. 

## Applying Hashing on fields
The big difference between hashing and encryption is that hashing is a
one way action and cannot be undone. This great for password like 
usecases.

Assigning a field to hash works in a similar ways as encryption. There
are 2 differences.

1. Hashing does not require a DataSubjectID attribute.
2. Hashing is per field different.

> Note: SHA-512 is used for hashing

If we update the password field to enable hashing then it should look like
this:

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
        
        [DataSubjectId]
        public string UserId => _userId;

        [PersonalData]
        public string Name => _name;

        [PersonalData]
        public string Email => _email;

        [HashedData]
        public string Password => _password;
    }
```

In the when stored in the eventstore it looks like this:

```json
Data
{
  "userId": "08073dcc-1113-42a0-a5aa-63214b342cef",
  "name": "crypto.os36USh4OSSPaLQDN98QwQ==",
  "email": "crypto.r/hoefWUpkqJYMbbsKo1Vx8717NvVGWaRKUJe58Lsao=",
  "password": "d5bb9c1b99ee10195c84a79842ad1bf812eb82d1b93faedd8978f42a86cd38e7f6ae80b8b72161bc9e0164c5294bd4b2d0b2cbcaa24346231cc723f5e77c592d"
}
				
Metadata
{
  "password": "M6qZN46EImjrWZUiZmi93SLIbPxzvYCa1uCtNgW25kxAw8auXHRvMDwnNWDEx7KNWtvT01A3ZnaLanHZQmVDUQ==",
  "dataSubjectId": "08073dcc-1113-42a0-a5aa-63214b342cef"
}
```

