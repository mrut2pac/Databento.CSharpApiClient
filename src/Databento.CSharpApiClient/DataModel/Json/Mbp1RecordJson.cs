using System;

using Newtonsoft.Json;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Market-by-Price depth-1 (top-of-book) record from the <c>mbp-1</c> schema (JSON encoding).
    /// Each message reflects a single order-book event (add/modify/delete/trade) plus the resulting
    /// best bid and offer.  Corresponds to DBN rtype <c>Mbp1</c>.
    /// </summary>
    public sealed class Mbp1RecordJson
    {
        [JsonProperty("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Order price (display-scaled).</summary>
        [JsonProperty("price")]
        public double Price { get; set; }

        /// <summary>Order quantity in lots.</summary>
        [JsonProperty("size")]
        public uint Size { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        [JsonProperty("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the triggering order.</summary>
        [JsonProperty("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        [JsonProperty("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        [JsonProperty("depth")]
        public uint Depth { get; set; }

        [JsonProperty("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonProperty("ts_in_delta")]
        public long TsInDelta { get; set; }

        [JsonProperty("sequence")]
        public uint Sequence { get; set; }

        /// <summary>Best bid / best offer after applying this event.</summary>
        [JsonProperty("levels")]
        public Mbp1LevelJson[] Levels { get; set; }

        [JsonProperty("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        public Mbp1LevelJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;
    }
}
