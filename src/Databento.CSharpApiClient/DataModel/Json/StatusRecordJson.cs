using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A trading-status / halt message record from the <c>status</c> schema (JSON encoding).
    /// Indicates changes to the trading or quoting state of an instrument.
    /// Corresponds to DBN rtype <c>Status</c>.
    /// </summary>
    public sealed class StatusRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>
        /// The status action code that generated this update (e.g. <c>"H"</c> = halt,
        /// <c>"Q"</c> = quote-only, <c>"T"</c> = trading).
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// Trading halt reason code (e.g. <c>"0"</c> = not halted / normal, <c>"T12"</c> = regulatory halt).
        /// </summary>
        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        /// <summary>Trading event code describing the specific event (e.g. <c>"0"</c> = no event).</summary>
        [JsonPropertyName("trading_event")]
        public string TradingEvent { get; set; }

        /// <summary>Indicates whether the instrument is currently in a tradeable state (<c>"Y"</c>, <c>"N"</c>, or <c>"U"</c> = unknown).</summary>
        [JsonPropertyName("is_trading")]
        public string IsTrading { get; set; }

        /// <summary>Indicates whether the instrument is currently in a quotable state (<c>"Y"</c>, <c>"N"</c>, or <c>"U"</c> = unknown).</summary>
        [JsonPropertyName("is_quoting")]
        public string IsQuoting { get; set; }

        /// <summary>Indicates whether short-selling is restricted for this instrument (<c>"Y"</c>, <c>"N"</c>, or <c>"U"</c> = unknown).</summary>
        [JsonPropertyName("is_short_sell_restricted")]
        public string IsShortSellRestricted { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
