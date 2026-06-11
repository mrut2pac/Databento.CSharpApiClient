using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class UnitPriceInfo
    {
        /// <summary>
        /// Access mode, e.g. "historical-streaming", "live-intraday".
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
    }
}
