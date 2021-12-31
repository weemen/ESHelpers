using System;
using System.Security.Cryptography;
using ESHelpers.Encryption;
using ESHelpers.Infratructure.Crypto;
using Xunit;

namespace ESHelpersTests.Infrastructure.Crypto
{
    public class InMemoryStoreTest
    {
        [Fact]
        public void it_can_store_an_encryption_key()
        {
            var store = new InMemory();
            var identifier = new Guid().ToString();
            
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            var encryptionKey = new EncryptionKey(aes.Key, aes.IV);

            var storeContents = store.Store;
            Assert.Empty(storeContents);
            
            store.SaveKeyToStore(identifier, encryptionKey);
            
            Assert.Single(storeContents);
            Assert.True(storeContents.ContainsKey(identifier));
            Assert.True(storeContents.ContainsValue(encryptionKey));
        }

        [Fact]
        public void it_can_return_null_if_key_not_found()
        {
            var store = new InMemory();
            var identifier = new Guid().ToString();
            
            Assert.Null(store.loadKeyFromStore(identifier));
        }

        [Fact]
        public void it_can_load_a_key_from_the_store()
        {
            var store = new InMemory();
            var identifier = new Guid().ToString();
            
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            var encryptionKey = new EncryptionKey(aes.Key, aes.IV);
            store.SaveKeyToStore(identifier, encryptionKey);
            
            Assert.Equal(encryptionKey, store.loadKeyFromStore(identifier));
        }

        [Fact]
        public void it_can_remove_a_key()
        {
            var store = new InMemory();
            var identifier = new Guid().ToString();
            
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            var encryptionKey = new EncryptionKey(aes.Key, aes.IV);
            store.SaveKeyToStore(identifier, encryptionKey);
            Assert.Single(store.Store);
            store.RemoveKeyFromStore(identifier);
            Assert.Empty(store.Store);
        }
    }
}