using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// Represents a Databento batch-download job as returned by <c>batch.submit_job</c>,
    /// <c>batch.list_jobs</c>, and <c>batch.get_job_details</c>.
    /// </summary>
    public sealed class BatchJob
    {
        /// <summary>Unique batch job identifier assigned by Databento.</summary>
        [JsonPropertyName("job_id")]
        public string JobId { get; set; }

        /// <summary>Dataset the job was submitted against (e.g. <c>"OPRA.PILLAR"</c>).</summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>Symbols requested (as submitted, before resolution).</summary>
        [JsonPropertyName("symbols")]
        public string[] Symbols { get; set; }

        /// <summary>Schema string (e.g. <c>"cbbo-1s"</c>).</summary>
        [JsonPropertyName("schema")]
        public string Schema { get; set; }

        /// <summary>Inclusive start of the requested time range.</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset? Start { get; set; }

        /// <summary>Exclusive end of the requested time range.</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset? End { get; set; }

        /// <summary>Output encoding, e.g. <c>"dbn"</c>, <c>"csv"</c>, <c>"json"</c>.</summary>
        [JsonPropertyName("encoding")]
        public string Encoding { get; set; }

        /// <summary>Output compression, e.g. <c>"zstd"</c> or <c>"none"</c>.</summary>
        [JsonPropertyName("compression")]
        public string Compression { get; set; }

        /// <summary>Whether prices are serialised in display format (true) or nano-integers (false).</summary>
        [JsonPropertyName("pretty_px")]
        public bool PrettyPx { get; set; }

        /// <summary>Whether timestamps are serialised as ISO 8601 strings (true) or nanoseconds (false).</summary>
        [JsonPropertyName("pretty_ts")]
        public bool PrettyTs { get; set; }

        /// <summary>Whether output files include a symbol-mapping column.</summary>
        [JsonPropertyName("map_symbols")]
        public bool MapSymbols { get; set; }

        /// <summary>Whether output is split into one file per symbol.</summary>
        [JsonPropertyName("split_symbols")]
        public bool SplitSymbols { get; set; }

        /// <summary>Duration by which output is split (e.g. <c>"day"</c>).</summary>
        [JsonPropertyName("split_duration")]
        public string SplitDuration { get; set; }

        /// <summary>
        /// Job lifecycle state: <c>"received"</c>, <c>"queued"</c>, <c>"processing"</c>,
        /// <c>"done"</c>, or <c>"expired"</c>.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>When Databento received the job submission.</summary>
        [JsonPropertyName("ts_received")]
        public DateTimeOffset? TsReceived { get; set; }

        /// <summary>When the job entered the processing queue.</summary>
        [JsonPropertyName("ts_queued")]
        public DateTimeOffset? TsQueued { get; set; }

        /// <summary>When processing started.</summary>
        [JsonPropertyName("ts_process_start")]
        public DateTimeOffset? TsProcessStart { get; set; }

        /// <summary>When processing completed.</summary>
        [JsonPropertyName("ts_process_done")]
        public DateTimeOffset? TsProcessDone { get; set; }

        /// <summary>When the output files will be automatically deleted.</summary>
        [JsonPropertyName("ts_expiration")]
        public DateTimeOffset? TsExpiration { get; set; }

        /// <summary>Total number of records in the output.</summary>
        [JsonPropertyName("record_count")]
        public long? RecordCount { get; set; }

        /// <summary>Bytes billed for this job.</summary>
        [JsonPropertyName("billed_size")]
        public long? BilledSize { get; set; }

        /// <summary>Actual uncompressed output size in bytes.</summary>
        [JsonPropertyName("actual_size")]
        public long? ActualSize { get; set; }

        /// <summary>Cost charged for this job in US dollars.</summary>
        [JsonPropertyName("cost_usd")]
        public decimal? CostUsd { get; set; }

        /// <summary>Output files available for download once the job is in state <c>"done"</c>.</summary>
        [JsonPropertyName("files")]
        public BatchFile[] Files { get; set; }
    }
}
