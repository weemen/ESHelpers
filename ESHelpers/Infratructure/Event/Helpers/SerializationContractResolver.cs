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
        
        public SerializationContractResolver(
            ICryptoTransform encryptor,
            FieldEncryptionDecryption fieldEncryptionDecryption)
        {
            _encryptor = encryptor;
            _fieldEncryptionDecryption = fieldEncryptionDecryption;
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
    }
}