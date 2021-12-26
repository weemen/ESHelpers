using ESHelpers.Encryption;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ESHelpers.Infratructure.Event.Helpers
{
    public class JsonSerializerSettingsFactory
    {
        private readonly EncryptorDecryptor _encryptorDecryptor;

        public JsonSerializerSettingsFactory(EncryptorDecryptor encryptorDecryptor)
        {
            _encryptorDecryptor = encryptorDecryptor;
        }
    
        public JsonSerializerSettings CreateDefault()
        {
            var defaultContractResolver = new CamelCasePropertyNamesContractResolver();
            var defaultSettings = GetSettings(defaultContractResolver);
            return defaultSettings;
        }
    
        public JsonSerializerSettings CreateForEncryption(string identifier)
        {
            var encryptor = _encryptorDecryptor.GetEncryptor(identifier);
            var fieldEncryptionDecryption = new FieldEncryptionDecryption();
            var serializationContractResolver = 
                new SerializationContractResolver(encryptor, fieldEncryptionDecryption);
            var jsonSerializerSettings = GetSettings(serializationContractResolver);
            return jsonSerializerSettings;
        }
    
        private JsonSerializerSettings GetSettings(IContractResolver contractResolver)
        {
            var settings =
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                };
            return settings;
        }
        
        public JsonSerializerSettings CreateForDecryption(string dataSubjectId)
        {
            var decryptor = _encryptorDecryptor.GetDecryptor(dataSubjectId);
            var fieldEncryptionDecryption = new FieldEncryptionDecryption();
            var deserializationContractResolver = 
                new DeserializationContractResolver(decryptor, fieldEncryptionDecryption);
            var jsonDeserializerSettings = GetSettings(deserializationContractResolver);
            return jsonDeserializerSettings;
        }
    }
}