using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class NanoPriceConverter : JsonConverter<double>
    {
        public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
        {
            writer.WriteValue(Utils.DoubleToNano(value));
        }

        public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Custom parsing logic for price
            return Utils.ParseNanoPrice(reader.Value.ToString());
        }
    }
}
