using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace RyderDisplay.Components.Network
{
    // Source: https://stackoverflow.com/questions/6416017/json-net-deserializing-nested-dictionaries
    class JsonDeserializerToDictionaries : CustomCreationConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }

        public override bool CanConvert(Type objectType)
        {
            // in addition to handling IDictionary<string, object>
            // we want to handle the deserialization of dict value
            // which is of type object
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null)
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                IDictionary<string, object> result = new Dictionary<string, object>();
                JArray data = (JArray)serializer.Deserialize(reader);
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].Type == JTokenType.Object)
                    {
                        result.Add(
                            i.ToString(), 
                            JsonConvert.DeserializeObject<IDictionary<string, object>>(data[i].ToString(), new JsonConverter[] { new JsonDeserializerToDictionaries() })
                        );
                    } else
                    {
                        result.Add(i.ToString(), data[i].ToString());
                    }
                }
                return result;
            }
            else
            {
                return serializer.Deserialize(reader); ;
            }
        }
    }
}
