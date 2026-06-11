using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class FieldInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
