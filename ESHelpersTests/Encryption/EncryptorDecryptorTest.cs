using System;
using ESHelpers.Encryption;
using ESHelpers.Infratructure.Crypto;
using Xunit;

namespace ESHelpersTests.Encryption
{
    public class EncryptorDecryptorTest
    {
        [Fact]
        public void it_can_get_an_encryptor_with_new_ecryption_keys()
        {
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var encryptor = encryptorDecryptor.GetEncryptor(new Guid().ToString());
            Assert.NotNull(encryptor);
        }

        [Fact]
        public void it_can_get_an_decryptor_with_existing_identifier()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            encryptorDecryptor.GetEncryptor(identifier);
            
            Assert.NotNull(encryptorDecryptor.GetDecryptor(identifier));
        }

        [Fact]
        public void it_can_return_default_when_encryption_key_is_deleted()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            encryptorDecryptor.GetEncryptor(identifier);
            
            cryptoRepository.RemoveKeyFromStore(identifier);
            Assert.Null(encryptorDecryptor.GetDecryptor(identifier));
        }
    }
}