using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    /// <summary>
    /// Response from the <c>symbology.resolve</c> endpoint.
    /// Contains the resolved symbol mappings and any unresolved symbols.
    /// </summary>
    public sealed class SymbologyResolution
    {
        /// <summary>
        /// Maps each input symbol to its time-sliced output mappings.
        /// Key = input symbol string; Value = ordered, non-overlapping mapping intervals.
        /// </summary>
        [JsonPropertyName("result")]
        public Dictionary<string, MappedSymbol[]> Result { get; set; }

        /// <summary>Input symbols that were submitted in the request.</summary>
        [JsonPropertyName("symbols")]
        public string[] Symbols { get; set; }

        /// <summary>Input symbol type used for this resolution.</summary>
        [JsonPropertyName("stype_in")]
        public string StypeIn { get; set; }

        /// <summary>Output symbol type produced by this resolution.</summary>
        [JsonPropertyName("stype_out")]
        public string StypeOut { get; set; }

        /// <summary>Effective start date of the resolution range (<c>"YYYY-MM-DD"</c>).</summary>
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        /// <summary>Effective end date of the resolution range (<c>"YYYY-MM-DD"</c>).</summary>
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        /// <summary>Symbols that were only partially resolved over the date range.</summary>
        [JsonPropertyName("partial")]
        public string[] Partial { get; set; }

        /// <summary>Symbols for which no mapping was found in the date range.</summary>
        [JsonPropertyName("not_found")]
        public string[] NotFound { get; set; }
    }
}
