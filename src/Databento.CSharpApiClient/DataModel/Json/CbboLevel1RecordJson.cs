using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// The consolidated best-bid-and-offer level embedded in a CBBO record.
    /// Carries the top-of-book bid and ask from the consolidated feed.
    /// </summary>
    public sealed class CbboLevel1RecordJson
    {
        /// <summary>The bid price (display-scaled).</summary>
        [JsonPropertyName("bid_px")]
        public double BidPrice { get; set; }

        /// <summary>The ask price (display-scaled).</summary>
        [JsonPropertyName("ask_px")]
        public double AskPrice { get; set; }

        /// <summary>The bid size in lots.</summary>
        [JsonPropertyName("bid_sz")]
        public int BidSize { get; set; }

        /// <summary>The ask size in lots.</summary>
        [JsonPropertyName("ask_sz")]
        public int AskSize { get; set; }

        /// <summary>The publisher ID indicating the venue containing the best bid.</summary>
        [JsonPropertyName("bid_pb")]
        public int BidPublisherId { get; set; }

        /// <summary>The publisher ID indicating the venue containing the best ask.</summary>
        [JsonPropertyName("ask_pb")]
        public int AskPublisherId { get; set; }
    }
}
