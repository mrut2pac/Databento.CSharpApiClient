using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class FlagsConverter : JsonConverter<MessageInfoBits>
    {
        public override MessageInfoBits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Number)
            {
                return (MessageInfoBits)reader.GetByte();
            }

            return Utils.ParseFlags(reader.TokenType == JsonTokenType.Null ? null : reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, MessageInfoBits value, JsonSerializerOptions options)
            => writer.WriteNumberValue((byte)value);
    }
}
