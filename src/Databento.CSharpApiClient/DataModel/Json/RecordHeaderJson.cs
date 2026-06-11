using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// Common record header present in every JSON-encoded DBN message (<c>"hd"</c> field).
    /// </summary>
    public class RecordHeaderJson
    {
        /// <summary>Event timestamp at the venue, in UTC.</summary>
        [JsonPropertyName("ts_event")]
        public DateTime? TsEventUtc { get; set; }

        /// <summary>DBN record type discriminator identifying the message layout.</summary>
        [JsonPropertyName("rtype")]
        public RType RecordType { get; set; }

        /// <summary>
        /// Databento publisher identifier. Maps to a venue/feed via
        /// <c>metadata.list_publishers</c>.
        /// </summary>
        [JsonPropertyName("publisher_id")]
        public ushort PublisherId { get; set; }

        /// <summary>
        /// Databento numeric instrument identifier (uint32).
        /// Stable within a dataset and date range; use symbology to convert to ticker.
        /// </summary>
        [JsonPropertyName("instrument_id")]
        public uint InstrumentId { get; set; }
    }
}
