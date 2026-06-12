using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A symbol mapping record from the <c>symbol_mapping</c> schema (JSON encoding).
    /// Associates an instrument's <c>stype_in</c> (e.g. raw symbol) to its
    /// <c>stype_out</c> (e.g. instrument ID) over a specified date range.
    /// Corresponds to DBN rtype <c>SymbolMapping</c>.
    /// </summary>
    public sealed class SymbolMappingRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Symbol in the input symbology type (e.g. the raw exchange symbol).</summary>
        [JsonPropertyName("stype_in_symbol")]
        public string StypeInSymbol { get; set; }

        /// <summary>Symbol in the output symbology type (e.g. the instrument ID as a string).</summary>
        [JsonPropertyName("stype_out_symbol")]
        public string StypeOutSymbol { get; set; }

        /// <summary>Inclusive start date of the mapping interval (UTC).</summary>
        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>Exclusive end date of the mapping interval (UTC).</summary>
        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
