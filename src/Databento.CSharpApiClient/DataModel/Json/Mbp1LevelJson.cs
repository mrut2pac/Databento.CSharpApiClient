using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A single price level of a Market-by-Price (MBP-1) record.
    /// </summary>
    public sealed class Mbp1LevelJson
    {
        /// <summary>The bid price (display-scaled).</summary>
        [JsonPropertyName("bid_px")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double BidPrice { get; set; }

        /// <summary>The ask price (display-scaled).</summary>
        [JsonPropertyName("ask_px")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double AskPrice { get; set; }

        /// <summary>The bid size in lots.</summary>
        [JsonPropertyName("bid_sz")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint BidSize { get; set; }

        /// <summary>The ask size in lots.</summary>
        [JsonPropertyName("ask_sz")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint AskSize { get; set; }

        /// <summary>The bid order count at this level.</summary>
        [JsonPropertyName("bid_ct")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint BidCount { get; set; }

        /// <summary>The ask order count at this level.</summary>
        [JsonPropertyName("ask_ct")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint AskCount { get; set; }
    }
}
