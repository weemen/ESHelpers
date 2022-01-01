using System;
using System.Security.Cryptography;
using ESHelpers.Encryption;
using ESHelpers.Infratructure.Crypto;
using Xunit;

namespace ESHelpersTests.Infrastructure.Crypto
{
    public class CryptoRepositoryTest
    {
        [Fact]
        public void it_can_load_new_encryption_key_when_key_not_found()
        {
            var store = new InMemory();
            var repository = new CryptoRepository(store);
            var identifier = new Guid().ToString();
            Assert.Empty(store.Store);
            
            var key = repository.GetExistingOrNew(identifier, this.CreateNewEncryptionKey);
            Assert.Equal(typeof(EncryptionKey), key.GetType());
            Assert.Single(store.Store);
            Assert.True(store.Store.ContainsKey(identifier));
            Assert.True(store.Store.ContainsValue(key));
        }

        [Fact]
        public void it_can_load_existing_key_from_the_store()
        {
            var store = new InMemory();
            var repository = new CryptoRepository(store);
            var identifier = new Guid().ToString();
            var encryptionKey = this.CreateNewEncryptionKey();
            
            store.SaveKeyToStore(identifier, encryptionKey);
            var loadedKey = repository.GetExistingOrNew(identifier, this.CreateNewEncryptionKey);
            Assert.Equal(encryptionKey, loadedKey);
        }

        [Fact]
        public void it_can_load_existing_key_from_store_part_2()
        {
            var store = new InMemory();
            var repository = new CryptoRepository(store);
            var identifier = new Guid().ToString();
            var encryptionKey = this.CreateNewEncryptionKey();
            
            store.SaveKeyToStore(identifier, encryptionKey);
            var loadedKey = repository.GetExistingOrDefault(identifier);
            Assert.Equal(encryptionKey, loadedKey);
        }

        [Fact]
        public void it_can_load_default_value()
        {
            var store = new InMemory();
            var repository = new CryptoRepository(store);
            var identifier = new Guid().ToString();
            
            var loadedKey = repository.GetExistingOrDefault(identifier);
            Assert.Null(loadedKey);
        }

        [Fact]
        public void it_can_remove_a_key_from_the_store()
        {
            var store = new InMemory();
            var repository = new CryptoRepository(store);
            var identifier = new Guid().ToString();
            Assert.Empty(store.Store);
            
            var key = repository.GetExistingOrNew(identifier, this.CreateNewEncryptionKey);
            Assert.Equal(typeof(EncryptionKey), key.GetType());
            Assert.Single(store.Store);
            
            repository.RemoveKeyFromStore(identifier);
            Assert.Empty(store.Store);
        }
        
        private EncryptionKey CreateNewEncryptionKey()
        {
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            return new EncryptionKey(aes.Key, aes.IV);
        }
    }
}