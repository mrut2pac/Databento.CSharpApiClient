using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    public sealed class BatchJob
    {
        [JsonProperty("job_id")]
        public string JobId { get; set; }

        [JsonProperty("dataset")]
        public string Dataset { get; set; }

        [JsonProperty("symbols")]
        public string[] Symbols { get; set; }

        [JsonProperty("schema")]
        public string Schema { get; set; }

        [JsonProperty("start")]
        public DateTimeOffset? Start { get; set; }

        [JsonProperty("end")]
        public DateTimeOffset? End { get; set; }

        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        [JsonProperty("compression")]
        public string Compression { get; set; }

        [JsonProperty("pretty_px")]
        public bool PrettyPx { get; set; }

        [JsonProperty("pretty_ts")]
        public bool PrettyTs { get; set; }

        [JsonProperty("map_symbols")]
        public bool MapSymbols { get; set; }

        [JsonProperty("split_symbols")]
        public bool SplitSymbols { get; set; }

        [JsonProperty("split_duration")]
        public string SplitDuration { get; set; }

        /// <summary>
        /// Job lifecycle state: "received", "queued", "processing", "done", "expired".
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("ts_received")]
        public DateTimeOffset? TsReceived { get; set; }

        [JsonProperty("ts_queued")]
        public DateTimeOffset? TsQueued { get; set; }

        [JsonProperty("ts_process_start")]
        public DateTimeOffset? TsProcessStart { get; set; }

        [JsonProperty("ts_process_done")]
        public DateTimeOffset? TsProcessDone { get; set; }

        [JsonProperty("ts_expiration")]
        public DateTimeOffset? TsExpiration { get; set; }

        [JsonProperty("record_count")]
        public long? RecordCount { get; set; }

        [JsonProperty("billed_size")]
        public long? BilledSize { get; set; }

        [JsonProperty("actual_size")]
        public long? ActualSize { get; set; }

        [JsonProperty("cost_usd")]
        public decimal? CostUsd { get; set; }

        [JsonProperty("files")]
        public BatchFile[] Files { get; set; }
    }
}
