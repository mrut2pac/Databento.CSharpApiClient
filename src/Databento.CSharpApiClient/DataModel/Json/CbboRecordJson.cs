using System;
using System.Diagnostics;

using Newtonsoft.Json;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | ({Level1.BidSize}) {Level1.BidPrice} - {Level1.AskPrice} ({Level1.AskSize})")]
    public sealed class CbboRecordJson
    {
        [JsonProperty("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonProperty("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        [JsonProperty("levels")]
        public CbboLevel1RecordJson[] Levels { get; set; }

        [JsonIgnore]
        public CbboLevel1RecordJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;

        // Optional in some feeds
        [JsonProperty("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
