using System;
using System.Diagnostics;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// An OHLCV bar record from any of the OHLCV schemas (<c>ohlcv-1s</c>, <c>ohlcv-1m</c>,
    /// <c>ohlcv-1h</c>, <c>ohlcv-1d</c>, <c>ohlcv-eod</c>) in JSON encoding.
    /// </summary>
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}")]
    public sealed class OhlcvRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Open price of the bar interval (display-scaled).</summary>
        [JsonPropertyName("open")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Open { get; set; }

        /// <summary>Highest traded price within the bar interval (display-scaled).</summary>
        [JsonPropertyName("high")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double High { get; set; }

        /// <summary>Lowest traded price within the bar interval (display-scaled).</summary>
        [JsonPropertyName("low")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Low { get; set; }

        /// <summary>Close price of the bar interval (display-scaled).</summary>
        [JsonPropertyName("close")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Close { get; set; }

        /// <summary>Total traded volume within the bar interval (in lots).</summary>
        [JsonPropertyName("volume")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long Volume { get; set; }

        /// <summary>Timestamp at which the bar interval ended (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
