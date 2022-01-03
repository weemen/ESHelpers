# Implementing your own Cryptostore
You problably need an cryptostore but not want to work with MySQL and you're
probably wondering: "Can I easily create my own store"?

The answer should be yes and this is what you do!

You can just create a new class and implement the `ICryptoStore` interface, which can be found [here](https://github.com/weemen/ESHelpers/blob/master/ESHelpers/Infratructure/Crypto/ICryptoStore.cs).

```c#
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
```

Just you implement this interface you can inject your store in the constructor of the [CryptoRepository](https://github.com/weemen/ESHelpers/blob/master/ESHelpers/Infratructure/Crypto/CryptoRepository.cs).  
Everything should running out of the box as long as you adhere to the interface.

If you run into any issues just leave a question on our github issue tracker and I will try to help you out!