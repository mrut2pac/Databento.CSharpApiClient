using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Trade + Consolidated BBO record from the <c>tcbbo</c> schema (JSON encoding).
    /// Each message records a trade event alongside the resulting consolidated (national) BBO.
    /// Corresponds to DBN rtype <c>Tcbbo</c>.
    /// </summary>
    public sealed class TcbboRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Trade price (display-scaled).</summary>
        [JsonPropertyName("price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Price { get; set; }

        /// <summary>Trade quantity in lots.</summary>
        [JsonPropertyName("size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Size { get; set; }

        /// <summary>Order-book action; always "T" for trade events in this schema.</summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side: Buyer, Seller, or None.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>Level at which the trade occurred (0-indexed).</summary>
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

        /// <summary>Consolidated BBO after the trade. Always length 1; use <see cref="Level1"/> for convenient access.</summary>
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
