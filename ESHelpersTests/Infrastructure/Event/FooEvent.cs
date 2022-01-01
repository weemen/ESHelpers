using ESHelpers.Attributes;
using ESHelpers.EventSourcing;

namespace ESHelpersTests.Infrastructure.Event
{
    public class FooEvent : IDomainEvent
    {
        private readonly string _id;
        private readonly string _name;
        private readonly string _hashme;
        private readonly string _encryptme;
        
        public FooEvent(string id, string name, string hashme, string encryptme)
        {
            _id = id;
            _name = name;
            _hashme = hashme;
            _encryptme = encryptme;
        }

        [DataSubjectId]
        public string Id => _id;

        public string Name => _name;

        [HashedData]
        public string Hashme => _hashme;
        
        [PersonalData]
        public string Encryptme => _encryptme;
    }
}