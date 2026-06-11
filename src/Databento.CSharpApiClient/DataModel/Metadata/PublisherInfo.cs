using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class PublisherInfo
    {
        [JsonPropertyName("publisher_id")]
        public ushort PublisherId { get; set; }

        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        [JsonPropertyName("venue")]
        public string Venue { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
