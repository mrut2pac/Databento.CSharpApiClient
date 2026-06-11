using System;

using Newtonsoft.Json;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single trade tick record from the <c>trades</c> schema (JSON encoding).
    /// Corresponds to DBN rtype <c>Mbp0</c>.
    /// </summary>
    public sealed class TradeRecordJson
    {
        [JsonProperty("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Trade price (display-scaled, i.e. pretty_px=true).</summary>
        [JsonProperty("price")]
        public double Price { get; set; }

        /// <summary>Trade quantity in lots.</summary>
        [JsonProperty("size")]
        public uint Size { get; set; }

        /// <summary>
        /// Order-book action for this message; always "T" for trade ticks.
        /// </summary>
        [JsonProperty("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side: Buyer, Seller, or None.</summary>
        [JsonProperty("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        [JsonProperty("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>Level at which the trade occurred (0-indexed).</summary>
        [JsonProperty("depth")]
        public uint Depth { get; set; }

        [JsonProperty("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond delta from venue receipt to gateway receipt.</summary>
        [JsonProperty("ts_in_delta")]
        public long TsInDelta { get; set; }

        [JsonProperty("sequence")]
        public uint Sequence { get; set; }

        /// <summary>Optional send timestamp from the gateway. Present when ts_out was requested.</summary>
        [JsonProperty("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
