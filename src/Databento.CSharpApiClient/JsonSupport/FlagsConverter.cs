using System;

using Newtonsoft.Json;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class FlagsConverter : JsonConverter<MessageInfoBits>
    {
        public override void WriteJson(JsonWriter writer, MessageInfoBits value, JsonSerializer serializer)
        {
            writer.WriteValue((byte)value);
        }

        public override MessageInfoBits ReadJson(JsonReader reader, Type objectType, MessageInfoBits existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Utils.ParseFlags(reader.Value.ToString());
        }
    }
}
