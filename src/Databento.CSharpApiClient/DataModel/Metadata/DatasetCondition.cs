using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class DatasetCondition
    {
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>
        /// One of: "available", "degraded", "pending", "missing".
        /// </summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Date string "YYYY-MM-DD" of the last time this dataset's condition was updated.
        /// </summary>
        [JsonPropertyName("last_modified_date")]
        public string LastModifiedDate { get; set; }

        [JsonPropertyName("date_generated")]
        public DateTimeOffset? DateGenerated { get; set; }
    }
}
