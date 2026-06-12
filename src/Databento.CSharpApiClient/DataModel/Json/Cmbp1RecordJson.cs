using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Consolidated MBP depth-1 record from the <c>cmbp-1</c> schema (JSON encoding).
    /// Each message reflects a single order-book event plus the resulting consolidated
    /// (national) best bid and offer. Corresponds to DBN rtype <c>Cmbp1</c>.
    /// </summary>
    public sealed class Cmbp1RecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Order price (display-scaled) of the triggering event.</summary>
        [JsonPropertyName("price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Price { get; set; }

        /// <summary>Order quantity in lots of the triggering event.</summary>
        [JsonPropertyName("size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Size { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the triggering order.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the event occurred (0 = top of book).</summary>
        [JsonPropertyName("depth")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        [JsonPropertyName("ts_in_delta")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        [JsonPropertyName("sequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Sequence { get; set; }

        /// <summary>Consolidated best bid / best offer after applying this event. Always length 1.</summary>
        [JsonPropertyName("levels")]
        public CbboLevel1RecordJson[] Levels { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>Convenience accessor for the single consolidated BBO level. <see langword="null"/> if <see cref="Levels"/> is empty.</summary>
        [JsonIgnore]
        public CbboLevel1RecordJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;
    }
}
