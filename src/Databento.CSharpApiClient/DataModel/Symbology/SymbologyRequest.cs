using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    /// <summary>
    /// Request body for the <c>symbology.resolve</c> endpoint.
    /// Maps input symbols from one type to another over a date range.
    /// </summary>
    public sealed class SymbologyRequest
    {
        /// <summary>Dataset to resolve symbols within (e.g. <c>"XNAS.ITCH"</c>).</summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>One or more symbols to resolve.</summary>
        [JsonPropertyName("symbols")]
        public string[] Symbols { get; set; }

        /// <summary>Input symbol type. Default: <c>"raw_symbol"</c>.</summary>
        [JsonPropertyName("stype_in")]
        public string StypeIn { get; set; } = SymbolTypes.RawSymbol;

        /// <summary>Output symbol type. Default: <c>"instrument_id"</c>.</summary>
        [JsonPropertyName("stype_out")]
        public string StypeOut { get; set; } = SymbolTypes.InstrumentId;

        /// <summary>Start of the resolution date range, formatted as <c>"YYYY-MM-DD"</c>.</summary>
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        /// <summary>End of the resolution date range, formatted as <c>"YYYY-MM-DD"</c>.</summary>
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }
    }
}
