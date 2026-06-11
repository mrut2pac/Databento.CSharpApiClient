using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class PublisherInfo
    {
        [JsonProperty("publisher_id")]
        public ushort PublisherId { get; set; }

        [JsonProperty("dataset")]
        public string Dataset { get; set; }

        [JsonProperty("venue")]
        public string Venue { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
