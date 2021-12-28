using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ESHelpers.Encryption
{
    public class HashingConverter: JsonConverter
    {
        private readonly string _salt;
        public HashingConverter(string salt)
        {
            _salt = salt;
        }
        public string ComputeSha512Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA512 sha512Hash = SHA512.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        }
        
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var result = this.ComputeSha512Hash(_salt + value);
            writer.WriteValue(result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;
    }
}