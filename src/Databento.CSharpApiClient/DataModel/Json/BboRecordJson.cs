using System;
using System.Diagnostics;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A venue-local Best Bid and Offer snapshot record from the <c>bbo-1s</c> or <c>bbo-1m</c>
    /// schema (JSON encoding). Contains the last-trade context plus the resulting BBO at the
    /// venue level.
    /// </summary>
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | ({Level1.BidSize}) {Level1.BidPrice} - {Level1.AskPrice} ({Level1.AskSize})")]
    public sealed class BboRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Last trade price (display-scaled). <see langword="null"/> if no trade in this interval.</summary>
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        /// <summary>Last trade size in lots.</summary>
        [JsonPropertyName("size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Size { get; set; }

        /// <summary>Aggressor side of the last trade: Buyer, Seller, or None.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

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

        /// <summary>
        /// BBO levels array (always length 1 for BBO schemas); use <see cref="Level1"/>
        /// for convenient access.
        /// </summary>
        [JsonPropertyName("levels")]
        public Mbp1LevelJson[] Levels { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>Convenience accessor for the single BBO level. <see langword="null"/> if <see cref="Levels"/> is empty.</summary>
        [JsonIgnore]
        public Mbp1LevelJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;
    }
}
