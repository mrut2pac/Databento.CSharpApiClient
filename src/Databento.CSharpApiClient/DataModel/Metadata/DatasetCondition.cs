using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Data-quality condition of a dataset on one day, as returned by
    /// <c>metadata.get_dataset_condition</c>.
    /// </summary>
    public sealed class DatasetCondition
    {
        /// <summary>
        /// Dataset identifier (e.g. <c>"XNAS.ITCH"</c>). The API doesn't send it; the client fills it in from the request.
        /// </summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>The day this condition describes (<c>"YYYY-MM-DD"</c>).</summary>
        [JsonPropertyName("date")]
        public string Date { get; set; }

        /// <summary>
        /// Quality condition: <c>"available"</c>, <c>"degraded"</c>, <c>"pending"</c>, or <c>"missing"</c>.
        /// </summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Date string (<c>"YYYY-MM-DD"</c>) when the data for this day was last modified.
        /// </summary>
        [JsonPropertyName("last_modified_date")]
        public string LastModifiedDate { get; set; }

        /// <summary>Always <c>null</c>: the API doesn't send a generation timestamp.</summary>
        [Obsolete("metadata.get_dataset_condition doesn't send date_generated, so this is always null.")]
        [JsonPropertyName("date_generated")]
        public DateTimeOffset? DateGenerated { get; set; }
    }
}
