using System.Collections.Generic;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    public sealed class SymbologyResolution
    {
        /// <summary>
        /// Maps each input symbol to its resolved mappings over the requested date range.
        /// Key = input symbol string; Value = time-sliced mapped symbols.
        /// </summary>
        [JsonProperty("result")]
        public Dictionary<string, MappedSymbol[]> Result { get; set; }

        [JsonProperty("symbols")]
        public string[] Symbols { get; set; }

        [JsonProperty("stype_in")]
        public string StypeIn { get; set; }

        [JsonProperty("stype_out")]
        public string StypeOut { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("end_date")]
        public string EndDate { get; set; }

        /// <summary>Symbols that were only partially resolved over the date range.</summary>
        [JsonProperty("partial")]
        public string[] Partial { get; set; }

        /// <summary>Symbols for which no mapping was found.</summary>
        [JsonProperty("not_found")]
        public string[] NotFound { get; set; }
    }
}
