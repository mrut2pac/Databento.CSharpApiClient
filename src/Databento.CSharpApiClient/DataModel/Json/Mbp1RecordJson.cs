using System;

using System.Text.Json.Serialization;

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
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Order price (display-scaled).</summary>
        [JsonPropertyName("price")]
        public double Price { get; set; }

        /// <summary>Order quantity in lots.</summary>
        [JsonPropertyName("size")]
        public uint Size { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the triggering order.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        [JsonPropertyName("depth")]
        public uint Depth { get; set; }

        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonPropertyName("ts_in_delta")]
        public long TsInDelta { get; set; }

        [JsonPropertyName("sequence")]
        public uint Sequence { get; set; }

        /// <summary>Best bid / best offer after applying this event.</summary>
        [JsonPropertyName("levels")]
        public Mbp1LevelJson[] Levels { get; set; }

        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        public Mbp1LevelJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;
    }
}
