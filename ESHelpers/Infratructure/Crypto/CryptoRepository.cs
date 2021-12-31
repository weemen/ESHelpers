using System;
using ESHelpers.Encryption;

namespace ESHelpers.Infratructure.Crypto
{
    public class CryptoRepository : ICryptoRepository
    {
        private ICryptoStore _cryptoStore;
        
        public CryptoRepository(ICryptoStore store)
        {
            this._cryptoStore = store;
        }

        public EncryptionKey? GetExistingOrNew(string identifier, Func<EncryptionKey> keyGenerator)
        {
            var encryptionKey = this._cryptoStore.loadKeyFromStore(identifier);
            if (encryptionKey != null) return encryptionKey;
            
            var newEncryptionKey = keyGenerator.Invoke();
            this._cryptoStore.SaveKeyToStore(identifier, newEncryptionKey);
            return newEncryptionKey;
        }

        public EncryptionKey GetExistingOrDefault(string identifier)
        {
            var encryptionKey = this._cryptoStore.loadKeyFromStore(identifier);
            return encryptionKey;
        }

        public void RemoveKeyFromStore(string identifier)
        {
            this._cryptoStore.RemoveKeyFromStore(identifier);
        }
    }
}