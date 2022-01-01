using System.Collections.Generic;
using ESHelpers.Encryption;

namespace ESHelpers.Infratructure.Crypto
{
    public class InMemory : ICryptoStore
    {
        private readonly Dictionary<string, EncryptionKey> _store = new Dictionary<string, EncryptionKey>();

        public Dictionary<string, EncryptionKey> Store => _store;

        public EncryptionKey loadKeyFromStore(string identifier)
        {
            _store.TryGetValue(identifier, out var key);
            return key;
        }

        public void SaveKeyToStore(string identifier, EncryptionKey encryptionKey)
        {
            _store.Add(identifier, encryptionKey);
        }

        public void RemoveKeyFromStore(string identifier)
        {
            _store.Remove(identifier);
        }
    }
}