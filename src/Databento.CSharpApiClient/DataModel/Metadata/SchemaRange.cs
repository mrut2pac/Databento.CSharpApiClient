using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Metadata
{
    /// <summary>
    /// Available range of one schema of a dataset, as listed in <see cref="DateRange.Schemas"/>.
    /// </summary>
    public sealed class SchemaRange
    {
        /// <summary>Earliest available data timestamp (inclusive).</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        /// <summary>Latest available data timestamp (exclusive).</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }
    }
}
