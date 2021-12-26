using System;
using ESHelpers.Encryption;

namespace ESHelpers.Infratructure.Crypto
{
    public interface ICryptoRepository
    {
        public EncryptionKey GetExistingOrNew(string identifier, Func<EncryptionKey> keyGenerator);
        public EncryptionKey GetExistingOrDefault(string identifier);
        public void RemoveKeyFromStore(string identifier);
    }
}