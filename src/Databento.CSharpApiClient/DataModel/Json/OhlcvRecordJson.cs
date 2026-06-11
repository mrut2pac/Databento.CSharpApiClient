using System;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Json
{
    [DebuggerDisplay("{Header.RecordType} | {Header.TsEventUtc} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}")]
    public sealed class OhlcvRecordJson
    {
        [JsonProperty("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("ts_out")]
        public DateTime TsOutUtc { get; set; }
    }
}
