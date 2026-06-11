using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class UnixNanoEpochConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Utils.ParseUnixNs(reader.TokenType == JsonTokenType.Null ? null : reader.GetString());

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            long unixNs = new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeMilliseconds() * 1_000_000L;
            writer.WriteNumberValue(unixNs);
        }
    }
}
