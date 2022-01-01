using System;
using ESHelpers.Encryption;
using ESHelpers.Infratructure.Crypto;
using Xunit;

namespace ESHelpersTests.Encryption
{
    public class FieldEncryptionDecryptionTest
    {
        [Fact]
        public void it_can_encrypt_an_text_value()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var encryptor = encryptorDecryptor.GetEncryptor(identifier);

            var fieldEncryptor = new FieldEncryptionDecryption();
            var encryptedValue = (string) fieldEncryptor.GetEncryptedOrDefault("hello world", encryptor);
            Assert.StartsWith("crypto.",encryptedValue);
        }

        [Fact]
        public void it_will_throw_when_encryptor_not_supplied()
        {
            var fieldEncryptor = new FieldEncryptionDecryption();
            var ex = Assert.Throws<ArgumentNullException>(() => fieldEncryptor.GetEncryptedOrDefault("hello world", null));
            Assert.Equal("Value cannot be null. (Parameter 'encryptor')",ex.Message);
        }

        [Fact]
        public void it_can_decrypt_an_encrypted_value()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var encryptor = encryptorDecryptor.GetEncryptor(identifier);
            
            var fieldEncryptorDecryptor = new FieldEncryptionDecryption();
            var encryptedValue = (string) fieldEncryptorDecryptor.GetEncryptedOrDefault("hello world", encryptor);

            var decryptor = encryptorDecryptor.GetDecryptor(identifier);

            var decryptedValue = fieldEncryptorDecryptor.GetDecryptedOrDefault(encryptedValue, decryptor, typeof(String));
            Assert.Equal("hello world", decryptedValue);
        }

        [Fact]
        public void it_can_return_a_masked_value_when_encryption_key_is_removed()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var encryptor = encryptorDecryptor.GetEncryptor(identifier);
            
            var fieldEncryptorDecryptor = new FieldEncryptionDecryption();
            var encryptedValue = (string) fieldEncryptorDecryptor.GetEncryptedOrDefault("hello world", encryptor);
            
            cryptoRepository.RemoveKeyFromStore(identifier);
            
            var decryptor = encryptorDecryptor.GetDecryptor(identifier);
            var decryptedValue = fieldEncryptorDecryptor.GetDecryptedOrDefault(encryptedValue, decryptor, typeof(String));
            Assert.Equal("***", decryptedValue);
        }
        
        [Fact]
        public void it_can_return_non_encrypted_values()
        {
            var identifier = new Guid().ToString();
            var cryptoStore = new InMemory();
            var cryptoRepository = new CryptoRepository(cryptoStore);
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var fieldEncryptorDecryptor = new FieldEncryptionDecryption();

            cryptoRepository.RemoveKeyFromStore(identifier);
            
            var decryptor = encryptorDecryptor.GetDecryptor(identifier);
            var decryptedValue = fieldEncryptorDecryptor.GetDecryptedOrDefault("hello world", decryptor, typeof(String));
            Assert.Equal("hello world", decryptedValue);
        }
    }
}