using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// Represents a Databento batch-download job as returned by <c>batch.submit_job</c>,
    /// <c>batch.list_jobs</c>, and <c>batch.get_job_details</c>.
    /// </summary>
    /// <remarks>
    /// <c>batch.list_jobs</c> returns a summary of each job: only <see cref="JobId"/>, <see cref="State"/> and
    /// <see cref="TsReceived"/> are filled. <see cref="DatabentoJsonClient.GetBatchJobDetailsAsync"/> returns the rest.
    /// </remarks>
    public sealed class BatchJob
    {
        /// <summary>Unique batch job identifier assigned by Databento.</summary>
        [JsonPropertyName("id")]
        public string JobId { get; set; }

        /// <summary>The account that submitted the job.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        /// <summary>The bill the job is charged to; <see langword="null"/> until it is billed.</summary>
        [JsonPropertyName("bill_id")]
        public string BillId { get; set; }

        /// <summary>Dataset the job was submitted against (e.g. <c>"OPRA.PILLAR"</c>).</summary>
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; }

        /// <summary>Symbols requested (as submitted, before resolution). The API sends them comma-joined in one string.</summary>
        [JsonPropertyName("symbols")]
        [JsonConverter(typeof(CommaSeparatedListConverter))]
        public string[] Symbols { get; set; }

        /// <summary>Symbology type of <see cref="Symbols"/> (e.g. <c>"raw_symbol"</c>).</summary>
        [JsonPropertyName("stype_in")]
        public string StypeIn { get; set; }

        /// <summary>Symbology type the output maps symbols to (e.g. <c>"instrument_id"</c>).</summary>
        [JsonPropertyName("stype_out")]
        public string StypeOut { get; set; }

        /// <summary>Maximum number of records requested; <see langword="null"/> for no limit.</summary>
        [JsonPropertyName("limit")]
        public long? Limit { get; set; }

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

        /// <summary>Maximum size of each output file in bytes; <see langword="null"/> when files aren't split by size.</summary>
        [JsonPropertyName("split_size")]
        public long? SplitSize { get; set; }

        /// <summary>Archive the output is packaged in (e.g. <c>"zip"</c>); <see langword="null"/> for none.</summary>
        [JsonPropertyName("packaging")]
        public string Packaging { get; set; }

        /// <summary>How the output is delivered (e.g. <c>"download"</c>).</summary>
        [JsonPropertyName("delivery")]
        public string Delivery { get; set; }

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

        /// <summary>Size of the delivered package in bytes, including metadata files.</summary>
        [JsonPropertyName("package_size")]
        public long? PackageSize { get; set; }

        /// <summary>Processing progress in percent (0-100).</summary>
        [JsonPropertyName("progress")]
        public int? Progress { get; set; }

        /// <summary>Cost charged for this job in US dollars.</summary>
        [JsonPropertyName("cost_usd")]
        public decimal? CostUsd { get; set; }

        /// <summary>Always <c>null</c>: the job responses list no files. Use <see cref="DatabentoJsonClient.ListBatchFilesAsync"/>.</summary>
        [Obsolete("The batch job responses list no files, so this is always null. Use ListBatchFiles.")]
        [JsonPropertyName("files")]
        public BatchFile[] Files { get; set; }
    }
}
