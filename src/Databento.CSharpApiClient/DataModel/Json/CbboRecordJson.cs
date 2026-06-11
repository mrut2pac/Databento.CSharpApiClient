using System;
using System.Diagnostics;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Consolidated/National Best Bid and Offer (CBBO) snapshot record from the
    /// <c>cbbo-1s</c> or <c>cbbo-1m</c> schema (JSON encoding).
    /// </summary>
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | ({Level1.BidSize}) {Level1.BidPrice} - {Level1.AskPrice} ({Level1.AskSize})")]
    public sealed class CbboRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Aggressor side of the last trade that updated the CBBO.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Last trade price (display-scaled). <see langword="null"/> if no trade in this interval.</summary>
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        /// <summary>Last trade size in lots.</summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>Message info flags.</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>
        /// BBO levels array. Always length 1 for CBBO schemas; use <see cref="Level1"/>
        /// for convenient access.
        /// </summary>
        [JsonPropertyName("levels")]
        public CbboLevel1RecordJson[] Levels { get; set; }

        /// <summary>Convenience accessor for the single BBO level. <see langword="null"/> if <see cref="Levels"/> is empty.</summary>
        [JsonIgnore]
        public CbboLevel1RecordJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
