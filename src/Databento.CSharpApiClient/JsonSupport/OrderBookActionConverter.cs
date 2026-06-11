using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal sealed class OrderBookActionConverter : JsonConverter<OrderBookAction>
    {
        public override OrderBookAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return OrderBookAction.Unknown;
            }

            // Most encodings send the action as a single-character string ("A", "T", ...), but some emit
            // the raw character code as a JSON integer — map that back to its char before matching.
            char c;
            if(reader.TokenType == JsonTokenType.Number)
            {
                c = (char)reader.GetInt64();
            }
            else
            {
                string s = reader.GetString();
                if(string.IsNullOrEmpty(s))
                {
                    return OrderBookAction.Unknown;
                }

                c = s[0];
            }

            switch(c)
            {
                case 'A': return OrderBookAction.Add;
                case 'M': return OrderBookAction.Modify;
                case 'D': return OrderBookAction.Delete;
                case 'R': return OrderBookAction.Reset;
                case 'U': return OrderBookAction.Update;
                case 'F': return OrderBookAction.Fill;
                case 'T': return OrderBookAction.Trade;
                default: return OrderBookAction.Unknown;
            }
        }

        public override void Write(Utf8JsonWriter writer, OrderBookAction value, JsonSerializerOptions options)
            => writer.WriteStringValue(((char)value).ToString());
    }
}
