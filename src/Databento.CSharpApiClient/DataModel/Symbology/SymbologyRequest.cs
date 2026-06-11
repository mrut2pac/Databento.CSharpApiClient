using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    public sealed class SymbologyRequest
    {
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        [JsonPropertyName("symbols")]
        public string[] Symbols { get; set; }

        /// <summary>Input symbol type. Default: "raw_symbol".</summary>
        [JsonPropertyName("stype_in")]
        public string StypeIn { get; set; } = SymbolTypes.RawSymbol;

        /// <summary>Output symbol type. Default: "instrument_id".</summary>
        [JsonPropertyName("stype_out")]
        public string StypeOut { get; set; } = SymbolTypes.InstrumentId;

        /// <summary>Start date "YYYY-MM-DD" for the symbol mapping range.</summary>
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        /// <summary>End date "YYYY-MM-DD" for the symbol mapping range.</summary>
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }
    }
}
