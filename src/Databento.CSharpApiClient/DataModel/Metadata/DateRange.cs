using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Available date range returned by <c>metadata.get_dataset_range</c>.
    /// </summary>
    public sealed class DateRange
    {
        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("end")]
        public DateTimeOffset End { get; set; }
    }
}
