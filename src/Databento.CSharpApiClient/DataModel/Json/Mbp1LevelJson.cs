using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single price level of a Market-by-Price (MBP-1) record.
    /// </summary>
    public sealed class Mbp1LevelJson
    {
        [JsonProperty("bid_px")]
        public double BidPrice { get; set; }

        [JsonProperty("ask_px")]
        public double AskPrice { get; set; }

        [JsonProperty("bid_sz")]
        public uint BidSize { get; set; }

        [JsonProperty("ask_sz")]
        public uint AskSize { get; set; }

        /// <summary>Number of orders on the bid side at this level.</summary>
        [JsonProperty("bid_ct")]
        public uint BidCount { get; set; }

        /// <summary>Number of orders on the ask side at this level.</summary>
        [JsonProperty("ask_ct")]
        public uint AskCount { get; set; }
    }
}
