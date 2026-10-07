using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads a 32-bit integer field into a <see cref="double"/>, with the venue's "undefined" value
    /// (<see cref="int.MaxValue"/>) or <c>null</c> as <see cref="double.NaN"/>. Unlike a price, the value is never fixed-point scaled.
    /// </summary>
    internal sealed class UndefinedInt32Converter : JsonConverter<double>
    {
        // states the intent: a null token is a value this converter reads, as NaN
        public override bool HandleNull => true;

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return double.NaN;
            }

            // read as the wire type, so a value outside 32 bits fails instead of passing as a multiplier
            int value;
            if(reader.TokenType == JsonTokenType.String)
            {
                if(!int.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    throw new JsonException("\"" + reader.GetString() + "\" isn't a 32-bit integer.");
                }
            }
            else
            {
                value = reader.GetInt32();
            }

            return value == int.MaxValue ? double.NaN : value;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            // write the undefined value back as the sentinel the API sends, so it reads back as NaN
            if(double.IsNaN(value))
            {
                writer.WriteNumberValue(int.MaxValue);
                return;
            }

            if(value != Math.Floor(value) || value < int.MinValue || value >= int.MaxValue)
            {
                throw new JsonException("A 32-bit integer field can't hold " + value.ToString(CultureInfo.InvariantCulture) + ".");
            }

            writer.WriteNumberValue((int)value);
        }
    }
}
