using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Describes a single field within a Databento schema as returned by
    /// <c>metadata.list_fields</c>.
    /// </summary>
    public sealed class FieldInfo
    {
        /// <summary>Field name as it appears in the serialized output.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Data type of the field (e.g. <c>"int64_t"</c>, <c>"double"</c>, <c>"char[22]"</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>Human-readable description of the field's meaning.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
