using System;
using System.Diagnostics;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}")]
    public sealed class OhlcvRecordJson
    {
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }

        [JsonPropertyName("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
