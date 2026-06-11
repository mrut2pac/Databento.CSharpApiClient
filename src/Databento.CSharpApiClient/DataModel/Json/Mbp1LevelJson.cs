using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single price level of a Market-by-Price (MBP-1) record.
    /// </summary>
    public sealed class Mbp1LevelJson
    {
        [JsonPropertyName("bid_px")]
        public double BidPrice { get; set; }

        [JsonPropertyName("ask_px")]
        public double AskPrice { get; set; }

        [JsonPropertyName("bid_sz")]
        public uint BidSize { get; set; }

        [JsonPropertyName("ask_sz")]
        public uint AskSize { get; set; }

        /// <summary>Number of orders on the bid side at this level.</summary>
        [JsonPropertyName("bid_ct")]
        public uint BidCount { get; set; }

        /// <summary>Number of orders on the ask side at this level.</summary>
        [JsonPropertyName("ask_ct")]
        public uint AskCount { get; set; }
    }
}
