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
        /// Status action code that generated this update, as Databento's <c>StatusAction</c> numbering
        /// (e.g. <c>7</c> = trading, <c>8</c> = halt, <c>14</c> = short-sell restriction change).
        /// </summary>
        [JsonPropertyName("action")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort Action { get; set; }

        /// <summary>Reason code for the status change, as Databento's <c>StatusReason</c> numbering (<c>0</c> = none).</summary>
        [JsonPropertyName("reason")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort Reason { get; set; }

        /// <summary>Further detail on the trading event, as Databento's <c>TradingEvent</c> numbering (<c>0</c> = none).</summary>
        [JsonPropertyName("trading_event")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort TradingEvent { get; set; }

        /// <summary>Whether the instrument is currently in a tradeable state (<c>"Y"</c>, <c>"N"</c>, or <c>"~"</c> = not available).</summary>
        [JsonPropertyName("is_trading")]
        public string IsTrading { get; set; }

        /// <summary>Whether the instrument is currently in a quotable state (<c>"Y"</c>, <c>"N"</c>, or <c>"~"</c> = not available).</summary>
        [JsonPropertyName("is_quoting")]
        public string IsQuoting { get; set; }

        /// <summary>Whether short-selling is restricted for this instrument (<c>"Y"</c>, <c>"N"</c>, or <c>"~"</c> = not available).</summary>
        [JsonPropertyName("is_short_sell_restricted")]
        public string IsShortSellRestricted { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>The raw symbol this record belongs to. Populated only when the request covered more than one symbol; <see langword="null"/> otherwise.</summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }
    }
}
