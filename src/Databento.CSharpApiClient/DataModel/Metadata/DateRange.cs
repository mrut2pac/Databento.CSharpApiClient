using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Available date range returned by <c>metadata.get_dataset_range</c>.
    /// </summary>
    public sealed class DateRange
    {
        /// <summary>Earliest available data timestamp (inclusive).</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        /// <summary>Latest available data timestamp (exclusive).</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }
    }
}
