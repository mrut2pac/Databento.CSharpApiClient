using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class NanoPriceConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // pretty_px=true  → JSON number (e.g. 4.01)  → read directly
            // pretty_px=false → JSON string nano integer  → parse via Utils
            // null            → undefined/sentinel price  → NaN
            if(reader.TokenType == JsonTokenType.Null)
            {
                return double.NaN;
            }

            if(reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            return Utils.ParseNanoPrice(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
            => writer.WriteNumberValue(Utils.DoubleToNano(value));
    }
}
