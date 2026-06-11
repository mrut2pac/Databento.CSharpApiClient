using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class UnitPriceInfo
    {
        /// <summary>
        /// Access mode, e.g. "historical-streaming", "live-intraday".
        /// </summary>
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }
    }
}
