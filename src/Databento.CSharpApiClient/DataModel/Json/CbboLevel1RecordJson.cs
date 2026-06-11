using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Json
{
    public sealed class CbboLevel1RecordJson
    {
        [JsonProperty("bid_px")]
        public double BidPrice { get; set; }

        [JsonProperty("ask_px")]
        public double AskPrice { get; set; }

        [JsonProperty("bid_sz")]
        public int BidSize { get; set; }

        [JsonProperty("ask_sz")]
        public int AskSize { get; set; }

        [JsonProperty("bid_pb")]
        public int BidPublisherId { get; set; }

        [JsonProperty("ask_pb")]
        public int AskPublisherId { get; set; }
    }
}
