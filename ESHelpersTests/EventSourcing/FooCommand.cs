
namespace ESHelpersTests.EventSourcing
{
    public class FooCommand
    {
        private string _fooId;
        private string _name;
        private string _hashme;
        private string _encryptme;

        public FooCommand(string fooId, string name, string hashme, string encryptme)
        {
            _fooId = fooId;
            _name = name;
            _hashme = hashme;
            _encryptme = encryptme;
        }

        public string FooId => _fooId;

        public string Name => _name;

        public string Hashme => _hashme;

        public string Encryptme => _encryptme;
    }
}