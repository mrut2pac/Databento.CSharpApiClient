using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    public class RecordHeaderJson
    {
        [JsonPropertyName("ts_event")]
        public DateTime? TsEventUtc { get; set; }

        [JsonPropertyName("rtype")]
        public RType RecordType { get; set; }

        [JsonPropertyName("publisher_id")]
        public ushort PublisherId { get; set; }

        [JsonPropertyName("instrument_id")]
        public uint InstrumentId { get; set; }
    }
}
