using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

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
        [JsonConverter(typeof(NanoPriceConverter))]
        public double BidPrice { get; set; }

        /// <summary>The ask price (display-scaled).</summary>
        [JsonPropertyName("ask_px")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double AskPrice { get; set; }

        /// <summary>The bid size in lots.</summary>
        [JsonPropertyName("bid_sz")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int BidSize { get; set; }

        /// <summary>The ask size in lots.</summary>
        [JsonPropertyName("ask_sz")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int AskSize { get; set; }

        /// <summary>The publisher ID indicating the venue containing the best bid.</summary>
        [JsonPropertyName("bid_pb")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int BidPublisherId { get; set; }

        /// <summary>The publisher ID indicating the venue containing the best ask.</summary>
        [JsonPropertyName("ask_pb")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int AskPublisherId { get; set; }
    }
}
