using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class UnixNanoEpochConverter : JsonConverter<DateTime>
    {
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            // Ensure the DateTime is in UTC or convert it to UTC if its Kind is Unspecified or Local
            DateTimeOffset dateTimeOffset = new DateTimeOffset(value.ToUniversalTime());

            // Get milliseconds since Unix Epoch
            long unixMilliseconds = dateTimeOffset.ToUnixTimeMilliseconds();

            // Convert milliseconds to nanoseconds
            long unixNanoseconds = unixMilliseconds * 1_000_000;

            writer.WriteValue(unixNanoseconds);
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Custom parsing logic for DateTime
            return Utils.ParseUnixNs(reader.Value?.ToString());
        }
    }
}
