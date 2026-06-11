using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Json
{
    public class RecordHeaderJson
    {
        [JsonProperty("ts_event")]
        public DateTime? TsEventUtc { get; set; }

        [JsonProperty("rtype")]
        public RType RecordType { get; set; }

        [JsonProperty("publisher_id")]
        public ushort PublisherId { get; set; }

        [JsonProperty("instrument_id")]
        public uint InstrumentId { get; set; }
    }
}
