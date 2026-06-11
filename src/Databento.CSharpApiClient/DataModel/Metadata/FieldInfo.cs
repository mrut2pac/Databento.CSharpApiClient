using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class FieldInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
