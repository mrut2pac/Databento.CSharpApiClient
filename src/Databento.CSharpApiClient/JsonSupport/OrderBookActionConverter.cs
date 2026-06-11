using System;

using Newtonsoft.Json;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal sealed class OrderBookActionConverter : JsonConverter<OrderBookAction>
    {
        public override void WriteJson(JsonWriter writer, OrderBookAction value, JsonSerializer serializer)
        {
            writer.WriteValue(((char)value).ToString());
        }

        public override OrderBookAction ReadJson(JsonReader reader, Type objectType, OrderBookAction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.Value == null)
            {
                return OrderBookAction.Unknown;
            }

            // Most encodings send the action as a single-character string ("A", "T", ...), but some emit
            // the raw character code as a JSON integer — map that back to its char before matching.
            char c;
            if(reader.TokenType == JsonToken.Integer)
            {
                c = (char)Convert.ToInt64(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                string s = reader.Value.ToString();
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
    }
}
