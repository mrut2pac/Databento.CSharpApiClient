using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    public sealed class SymbologyRequest
    {
        [JsonProperty("dataset")]
        public string Dataset { get; set; }

        [JsonProperty("symbols")]
        public string[] Symbols { get; set; }

        /// <summary>Input symbol type. Default: "raw_symbol".</summary>
        [JsonProperty("stype_in")]
        public string StypeIn { get; set; } = SymbolTypes.RawSymbol;

        /// <summary>Output symbol type. Default: "instrument_id".</summary>
        [JsonProperty("stype_out")]
        public string StypeOut { get; set; } = SymbolTypes.InstrumentId;

        /// <summary>Start date "YYYY-MM-DD" for the symbol mapping range.</summary>
        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        /// <summary>End date "YYYY-MM-DD" for the symbol mapping range.</summary>
        [JsonProperty("end_date")]
        public string EndDate { get; set; }
    }
}
