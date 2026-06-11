using System;
using System.Diagnostics;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | ({Level1.BidSize}) {Level1.BidPrice} - {Level1.AskPrice} ({Level1.AskSize})")]
    public sealed class CbboRecordJson
    {
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        [JsonPropertyName("levels")]
        public CbboLevel1RecordJson[] Levels { get; set; }

        [JsonIgnore]
        public CbboLevel1RecordJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;

        // Optional in some feeds
        [JsonPropertyName("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
