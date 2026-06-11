using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Per-unit pricing information for a dataset access mode, as returned by
    /// <c>metadata.list_unit_prices</c>.
    /// </summary>
    public sealed class UnitPriceInfo
    {
        /// <summary>
        /// Access mode, e.g. <c>"historical-streaming"</c> or <c>"live-intraday"</c>.
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        /// <summary>Price per unit (per record or per GB, depending on the dataset) in US dollars.</summary>
        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
    }
}
