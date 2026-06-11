using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    internal class TradeAggressorConverter : JsonConverter<TradeAggressor>
    {
        public override TradeAggressor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Utils.ParseTradeAggressor(reader.GetString());

        public override void Write(Utf8JsonWriter writer, TradeAggressor value, JsonSerializerOptions options)
            => writer.WriteStringValue(((char)value).ToString());
    }
}
