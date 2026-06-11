using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Data-quality condition for a dataset on a given date, as returned by
    /// <c>metadata.get_dataset_condition</c>.
    /// </summary>
    public sealed class DatasetCondition
    {
        /// <summary>Dataset identifier (e.g. <c>"XNAS.ITCH"</c>).</summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>
        /// Quality condition: <c>"available"</c>, <c>"degraded"</c>, <c>"pending"</c>, or <c>"missing"</c>.
        /// </summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Date string (<c>"YYYY-MM-DD"</c>) when this dataset's condition was last updated.
        /// </summary>
        [JsonPropertyName("last_modified_date")]
        public string LastModifiedDate { get; set; }

        /// <summary>Timestamp when the condition report was generated.</summary>
        [JsonPropertyName("date_generated")]
        public DateTimeOffset? DateGenerated { get; set; }
    }
}
