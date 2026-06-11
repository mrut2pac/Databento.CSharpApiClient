using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    public sealed class BatchJob
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; }

        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        [JsonPropertyName("symbols")]
        public string[] Symbols { get; set; }

        [JsonPropertyName("schema")]
        public string Schema { get; set; }

        [JsonPropertyName("start")]
        public DateTimeOffset? Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset? End { get; set; }

        [JsonPropertyName("encoding")]
        public string Encoding { get; set; }

        [JsonPropertyName("compression")]
        public string Compression { get; set; }

        [JsonPropertyName("pretty_px")]
        public bool PrettyPx { get; set; }

        [JsonPropertyName("pretty_ts")]
        public bool PrettyTs { get; set; }

        [JsonPropertyName("map_symbols")]
        public bool MapSymbols { get; set; }

        [JsonPropertyName("split_symbols")]
        public bool SplitSymbols { get; set; }

        [JsonPropertyName("split_duration")]
        public string SplitDuration { get; set; }

        /// <summary>
        /// Job lifecycle state: "received", "queued", "processing", "done", "expired".
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("ts_received")]
        public DateTimeOffset? TsReceived { get; set; }

        [JsonPropertyName("ts_queued")]
        public DateTimeOffset? TsQueued { get; set; }

        [JsonPropertyName("ts_process_start")]
        public DateTimeOffset? TsProcessStart { get; set; }

        [JsonPropertyName("ts_process_done")]
        public DateTimeOffset? TsProcessDone { get; set; }

        [JsonPropertyName("ts_expiration")]
        public DateTimeOffset? TsExpiration { get; set; }

        [JsonPropertyName("record_count")]
        public long? RecordCount { get; set; }

        [JsonPropertyName("billed_size")]
        public long? BilledSize { get; set; }

        [JsonPropertyName("actual_size")]
        public long? ActualSize { get; set; }

        [JsonPropertyName("cost_usd")]
        public decimal? CostUsd { get; set; }

        [JsonPropertyName("files")]
        public BatchFile[] Files { get; set; }
    }
}
