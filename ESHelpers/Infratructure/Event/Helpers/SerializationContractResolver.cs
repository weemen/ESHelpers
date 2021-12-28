using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ESHelpers.Attributes;
using ESHelpers.Encryption;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ESHelpers.Infratructure.Event.Helpers
{
    public class SerializationContractResolver : DefaultContractResolver
    {
        private readonly ICryptoTransform _encryptor;
        private readonly FieldEncryptionDecryption _fieldEncryptionDecryption;
        private readonly Dictionary<string, string> _salts;
        
        public SerializationContractResolver(
            ICryptoTransform encryptor,
            FieldEncryptionDecryption fieldEncryptionDecryption,
            Dictionary<string, string> salts)
        {
            _encryptor = encryptor;
            _fieldEncryptionDecryption = fieldEncryptionDecryption;
            _salts = salts;
            
            NamingStrategy = new CamelCaseNamingStrategy();
        }
        
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            foreach (var jsonProperty in properties)
            {
                var isPersonalIdentifiableInformation = IsPersonalIdentifiableInformation(type, jsonProperty);
                if (isPersonalIdentifiableInformation)
                {
                    var serializationJsonConverter = new EncryptionJsonConverter(_encryptor, _fieldEncryptionDecryption);
                    jsonProperty.Converter = serializationJsonConverter;
                    continue;
                }

                var hasValueToBeHashed = this.HasValueThatNeedsToBeHashed(type, jsonProperty);
                if (hasValueToBeHashed)
                {
                    string salt;
                    if (!_salts.TryGetValue(jsonProperty.UnderlyingName, out salt))
                    {
                        throw new KeyNotFoundException($"Salt cannot be found for key: {jsonProperty.PropertyName}");
                    }

                    jsonProperty.Converter = new HashingConverter(salt);
                }
            }
            return properties;
        }
    
        private bool IsPersonalIdentifiableInformation(Type type, JsonProperty jsonProperty)
        {
            var propertyInfo = type.GetProperty(jsonProperty.UnderlyingName);
            if (propertyInfo is null)
            {
                return false;
            }
            var hasPersonalDataAttribute =
                propertyInfo.CustomAttributes
                    .Any(x => x.AttributeType == typeof(PersonalDataAttribute));
            var propertyType = propertyInfo.PropertyType;
            var isSimpleValue = propertyType.IsValueType || propertyType == typeof(string);
            var isSupportedPersonalIdentifiableInformation = isSimpleValue && hasPersonalDataAttribute;
            return isSupportedPersonalIdentifiableInformation;
        }

        private bool HasValueThatNeedsToBeHashed(Type type, JsonProperty jsonProperty)
        {
            var propertyInfo = type.GetProperty(jsonProperty.UnderlyingName);
            if (propertyInfo is null)
            {
                return false;
            }
            
            var hasHashedDataAttribute =
                propertyInfo.CustomAttributes
                    .Any(x => x.AttributeType == typeof(HashedDataAttribute));
            return hasHashedDataAttribute;
        }
    }
}