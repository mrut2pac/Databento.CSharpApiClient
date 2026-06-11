using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    public sealed class DatasetCondition
    {
        [JsonProperty("dataset")]
        public string Dataset { get; set; }

        /// <summary>
        /// One of: "available", "degraded", "pending", "missing".
        /// </summary>
        [JsonProperty("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Date string "YYYY-MM-DD" of the last time this dataset's condition was updated.
        /// </summary>
        [JsonProperty("last_modified_date")]
        public string LastModifiedDate { get; set; }

        [JsonProperty("date_generated")]
        public DateTimeOffset? DateGenerated { get; set; }
    }
}
