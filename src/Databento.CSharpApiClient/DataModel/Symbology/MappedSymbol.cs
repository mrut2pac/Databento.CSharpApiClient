using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    /// <summary>
    /// A single resolved symbol mapping valid within a specific date interval.
    /// </summary>
    public sealed class MappedSymbol
    {
        /// <summary>The resolved output symbol (e.g. the numeric instrument_id as a string).</summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>"YYYY-MM-DD" — inclusive start of the validity interval.</summary>
        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        /// <summary>"YYYY-MM-DD" — inclusive end of the validity interval.</summary>
        [JsonProperty("end_date")]
        public string EndDate { get; set; }
    }
}
