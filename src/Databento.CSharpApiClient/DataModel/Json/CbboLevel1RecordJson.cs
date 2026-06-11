using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    public sealed class CbboLevel1RecordJson
    {
        [JsonPropertyName("bid_px")]
        public double BidPrice { get; set; }

        [JsonPropertyName("ask_px")]
        public double AskPrice { get; set; }

        [JsonPropertyName("bid_sz")]
        public int BidSize { get; set; }

        [JsonPropertyName("ask_sz")]
        public int AskSize { get; set; }

        [JsonPropertyName("bid_pb")]
        public int BidPublisherId { get; set; }

        [JsonPropertyName("ask_pb")]
        public int AskPublisherId { get; set; }
    }
}
