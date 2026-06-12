using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Market-by-Price depth-10 record from the <c>mbp-10</c> schema (JSON encoding).
    /// Each message reflects a single order-book event plus the resulting top-10 bid/ask levels.
    /// Corresponds to DBN rtype <c>Mbp10</c>.
    /// </summary>
    public sealed class Mbp10RecordJson
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

        /// <summary>Price level at which the event occurred (0-indexed).</summary>
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

        /// <summary>Top-10 bid/ask price levels after applying this event. May have fewer than 10 entries.</summary>
        [JsonPropertyName("levels")]
        public Mbp1LevelJson[] Levels { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>Convenience accessor for the best bid/ask level. <see langword="null"/> if <see cref="Levels"/> is empty.</summary>
        [JsonIgnore]
        public Mbp1LevelJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;
    }
}
