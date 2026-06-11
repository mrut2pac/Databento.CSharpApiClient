using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class FlagsConverter : JsonConverter<MessageInfoBits>
    {
        public override MessageInfoBits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Utils.ParseFlags(reader.GetString());

        public override void Write(Utf8JsonWriter writer, MessageInfoBits value, JsonSerializerOptions options)
            => writer.WriteNumberValue((byte)value);
    }
}
