using System;

using Newtonsoft.Json;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class TradeAggressorConverter : JsonConverter<TradeAggressor>
    {
        public override void WriteJson(JsonWriter writer, TradeAggressor value, JsonSerializer serializer)
        {
            writer.WriteValue((char)value);
        }

        public override TradeAggressor ReadJson(JsonReader reader, Type objectType, TradeAggressor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Utils.ParseTradeAggressor(reader.Value.ToString());
        }
    }
}
