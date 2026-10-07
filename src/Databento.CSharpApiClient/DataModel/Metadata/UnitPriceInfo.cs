using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Per-schema unit prices for one dataset access mode, as returned by
    /// <c>metadata.list_unit_prices</c>.
    /// </summary>
    public sealed class UnitPriceInfo
    {
        /// <summary>
        /// Access mode, e.g. <c>"historical"</c>, <c>"historical-streaming"</c> or <c>"live"</c>.
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Price per GB in US dollars, keyed by schema (e.g. <c>"mbo"</c>, <c>"ohlcv-1d"</c>).
        /// Only the schemas the dataset serves in this mode are present.
        /// </summary>
        [JsonPropertyName("unit_prices")]
        public Dictionary<string, decimal> UnitPrices { get; set; }

        /// <summary>Always 0: the API sends no single price per mode. Use <see cref="UnitPrices"/>.</summary>
        [Obsolete("metadata.list_unit_prices sends no single price per mode, so this is always 0. Use UnitPrices, keyed by schema.")]
        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
    }
}
