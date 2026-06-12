using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class NanoPriceConverter : JsonConverter<double>
    {
        // Without this, System.Text.Json short-circuits null tokens for the non-nullable
        // double target and throws before Read runs, so the null → NaN branch never executes.
        public override bool HandleNull => true;

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
        {
            // Mirror the Read contract so serialized output round-trips: a JSON number is a
            // display-scaled price and null is the undefined-price sentinel. Writing the
            // nano-integer form as a number would be re-read as a display-scaled price.
            if(double.IsNaN(value))
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(value);
        }
    }
}
