using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Symbology
{
    /// <summary>
    /// A single resolved symbol mapping valid within a specific date interval.
    /// </summary>
    public sealed class MappedSymbol
    {
        /// <summary>The resolved output symbol (e.g. the numeric instrument_id as a string). Sent by the API as <c>s</c>.</summary>
        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        /// <summary>"YYYY-MM-DD" — inclusive start of the validity interval. Sent by the API as <c>d0</c>.</summary>
        [JsonPropertyName("d0")]
        public string StartDate { get; set; }

        /// <summary>"YYYY-MM-DD" — exclusive end of the validity interval. Sent by the API as <c>d1</c>.</summary>
        [JsonPropertyName("d1")]
        public string EndDate { get; set; }
    }
}
