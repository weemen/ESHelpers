using ESHelpers.Encryption;

namespace ESHelpers.Infratructure.Crypto
{
    public interface ICryptoStore
    {
        public EncryptionKey? loadKeyFromStore(string identifier);
        
        public void SaveKeyToStore(string identifier, EncryptionKey encryptionKey);

        public void RemoveKeyFromStore(string identifier);
    }
}