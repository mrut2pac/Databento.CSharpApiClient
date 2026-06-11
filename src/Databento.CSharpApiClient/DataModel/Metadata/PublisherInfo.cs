using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Describes a single Databento publisher (venue/feed combination) as returned
    /// by <c>metadata.list_publishers</c>.
    /// </summary>
    public sealed class PublisherInfo
    {
        /// <summary>Numeric publisher identifier used in record headers.</summary>
        [JsonPropertyName("publisher_id")]
        public ushort PublisherId { get; set; }

        /// <summary>Dataset this publisher belongs to (e.g. <c>"XNAS.ITCH"</c>).</summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>Venue/exchange MIC code (e.g. <c>"XNAS"</c>).</summary>
        [JsonPropertyName("venue")]
        public string Venue { get; set; }

        /// <summary>Human-readable description of the publisher.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>Short publisher name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
