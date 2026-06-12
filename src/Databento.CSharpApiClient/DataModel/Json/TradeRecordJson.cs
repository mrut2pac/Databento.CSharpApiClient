using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single trade tick record from the <c>trades</c> schema (JSON encoding).
    /// Corresponds to DBN rtype <c>Mbp0</c>.
    /// </summary>
    public sealed class TradeRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Trade price (display-scaled, i.e. pretty_px=true).</summary>
        [JsonPropertyName("price")]
        public double Price { get; set; }

        /// <summary>Trade quantity in lots.</summary>
        [JsonPropertyName("size")]
        public uint Size { get; set; }

        /// <summary>
        /// Order-book action for this message; always "T" for trade ticks.
        /// </summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side: Buyer, Seller, or None.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags (e.g. last-in-sequence, fill-or-kill).</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>Level at which the trade occurred (0-indexed).</summary>
        [JsonPropertyName("depth")]
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond delta from venue receipt to gateway receipt.</summary>
        [JsonPropertyName("ts_in_delta")]
        public long TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        [JsonPropertyName("sequence")]
        public uint Sequence { get; set; }

        /// <summary>Optional send timestamp from the gateway. Present when ts_out was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
