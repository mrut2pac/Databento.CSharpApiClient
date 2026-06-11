using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Available date range returned by <c>metadata.get_dataset_range</c>.
    /// </summary>
    public sealed class DateRange
    {
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }
    }
}
