using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single price level of a Market-by-Price (MBP-1) record.
    /// </summary>
    public sealed class Mbp1LevelJson
    {
        /// <summary>The bid price (display-scaled).</summary>
        [JsonPropertyName("bid_px")]
        public double BidPrice { get; set; }

        /// <summary>The ask price (display-scaled).</summary>
        [JsonPropertyName("ask_px")]
        public double AskPrice { get; set; }

        /// <summary>The bid size in lots.</summary>
        [JsonPropertyName("bid_sz")]
        public uint BidSize { get; set; }

        /// <summary>The ask size in lots.</summary>
        [JsonPropertyName("ask_sz")]
        public uint AskSize { get; set; }

        /// <summary>The bid order count at this level.</summary>
        [JsonPropertyName("bid_ct")]
        public uint BidCount { get; set; }

        /// <summary>The ask order count at this level.</summary>
        [JsonPropertyName("ask_ct")]
        public uint AskCount { get; set; }
    }
}
