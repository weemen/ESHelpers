using System.Security.Cryptography;
using ESHelpers.Infratructure.Crypto;

namespace ESHelpers.Encryption
{
    public class EncryptorDecryptor
    {
        private readonly ICryptoRepository _cryptoRepository;

        public EncryptorDecryptor(ICryptoRepository cryptoRepository)
        {
            _cryptoRepository = cryptoRepository;
        }
    
        public ICryptoTransform GetEncryptor(string identifier)
        {
            var encryptionKey = _cryptoRepository.GetExistingOrNew(identifier, CreateNewEncryptionKey);
            var aes = GetAes(encryptionKey);
            var encryptor = aes.CreateEncryptor();
            return encryptor;
        }

        public ICryptoTransform GetDecryptor(string identifier)
        {
            var encryptionKey = _cryptoRepository.GetExistingOrDefault(identifier);
            if (encryptionKey is null)
            {
                // encryption key was deleted
                return default;
            }
        
            var aes = GetAes(encryptionKey);
            var decryptor = aes.CreateDecryptor();
            return decryptor;
        }
    
        private EncryptionKey CreateNewEncryptionKey()
        {
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
        
            var key = aes.Key;
            var vector = aes.IV;
            var encryptionKey = new EncryptionKey(key, vector);
            return encryptionKey;
        }
    
        private Aes GetAes(EncryptionKey encryptionKey)
        {
            var aes = Aes.Create();
        
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = encryptionKey.Key;
            aes.IV = encryptionKey.Vector;
        
            return aes;
        }
    }
}