using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESHelpers.Attributes;
using ESHelpers.EventSourcing;
using Newtonsoft.Json;

namespace ESHelpers.Infratructure.Event.Helpers
{
    public class JsonSerializer
    {
        private const string MetadataSubjectIdKey = "dataSubjectId";
        private readonly JsonSerializerSettingsFactory _jsonSerializerSettingsFactory;
        private readonly IEnumerable<Type> _supportedEvents;

        public JsonSerializer(
            JsonSerializerSettingsFactory jsonSerializerSettingsFactory,
            IEnumerable<Type> supportedEvents)
        {
            _jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
            _supportedEvents = supportedEvents;
        }

        public SerializedEvent Serialize(IDomainEvent @event)
        {
            var dataSubjectId = GetDataSubjectId(@event);
            var metadataValues =
                new Dictionary<string, string>
                {
                    { MetadataSubjectIdKey, dataSubjectId }
                };

            var hasPersonalData = dataSubjectId != null;
            var dataJsonSerializerSettings =
                hasPersonalData
                    ? _jsonSerializerSettingsFactory.CreateForEncryption(dataSubjectId)
                    : _jsonSerializerSettingsFactory.CreateDefault();

            var dataJson = JsonConvert.SerializeObject(@event, dataJsonSerializerSettings);
            var dataBytes = Encoding.UTF8.GetBytes(dataJson);

            var defaultJsonSettings = _jsonSerializerSettingsFactory.CreateDefault();
            var metadataJson = JsonConvert.SerializeObject(metadataValues, defaultJsonSettings);
            var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            var serializedEvent = new SerializedEvent(dataBytes, metadataBytes, true);
            return serializedEvent;
        }

        private string GetDataSubjectId(IDomainEvent @event)
        {
            var eventType = @event.GetType();
            var properties = eventType.GetProperties();
            var dataSubjectIdPropertyInfo =
                properties
                    .FirstOrDefault(x => x.GetCustomAttributes(typeof(DataSubjectIdAttribute), false)
                        .Any(y => y is DataSubjectIdAttribute));

            if (dataSubjectIdPropertyInfo is null)
            {
                return null;
            }

            var value = dataSubjectIdPropertyInfo.GetValue(@event);
            var dataSubjectId = value.ToString();
            return dataSubjectId;
        }
        
        public object Deserialize(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> metadata, string eventName)
        {
            var metadataJson = Encoding.UTF8.GetString(metadata.Span);
            var defaultJsonSettings = _jsonSerializerSettingsFactory.CreateDefault();
            var values =
                JsonConvert.DeserializeObject<IDictionary<string, string>>(metadataJson, defaultJsonSettings);
    
            var hasKey = values.TryGetValue(MetadataSubjectIdKey, out var dataSubjectId);
            var hasPersonalData = hasKey && !string.IsNullOrEmpty(dataSubjectId);

            var dataJsonDeserializerSettings =
                hasPersonalData
                    ? _jsonSerializerSettingsFactory.CreateForDecryption(dataSubjectId)
                    : _jsonSerializerSettingsFactory.CreateDefault();
            
            var dataJson = Encoding.UTF8.GetString(data.Span);
            var persistableEvent = 
                JsonConvert.DeserializeObject(dataJson, Type.GetType(eventName), dataJsonDeserializerSettings);
            return persistableEvent;
        }
    }
}