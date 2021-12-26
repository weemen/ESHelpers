namespace ESHelpers.Encryption
{
    public class EncryptionKey
    {
        public byte[] Key { get; }
        public byte[] Vector { get; }
    
        public EncryptionKey(
            byte[] key,
            byte[] vector)
        {
            Key = key;
            Vector = vector;
        }
    }
}