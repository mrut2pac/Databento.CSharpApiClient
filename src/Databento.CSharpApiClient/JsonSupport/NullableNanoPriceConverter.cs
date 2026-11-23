using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads an optional price as <see cref="NanoPriceConverter"/> reads a price, in either form (a display decimal with
    /// <c>pretty_px</c>, an integer of 1e-9 without), except that an undefined price reads as <see langword="null"/>, as the pretty
    /// encoding sends it, and a string that is no price throws <see cref="JsonException"/>. Writes a price as a JSON number and an
    /// undefined one as <see langword="null"/>.
    /// </summary>
    internal sealed class NullableNanoPriceConverter : JsonConverter<double?>
    {
        public override bool HandleNull => true;

        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if(reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            string text = reader.GetString();
            if(long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long nano))
            {
                // the undefined price, i64::MAX, reads as NaN
                double price = Utils.NanoToDouble(nano);
                return double.IsNaN(price) ? null : price;
            }

            if(double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double display))
            {
                return display;
            }

            throw new JsonException("Not a price: \"" + text + "\".");
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if(value.HasValue && !double.IsNaN(value.Value))
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
