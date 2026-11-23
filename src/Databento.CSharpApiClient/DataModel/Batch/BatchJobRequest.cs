using System;
using System.Collections.Generic;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// A batch job to submit with <see cref="DatabentoJsonClient.SubmitBatchJobAsync(BatchJobRequest, System.Threading.CancellationToken)"/>:
    /// the data, its encoding, and how the output is split into files. A split option left unset is left to the API's default.
    /// </summary>
    public sealed class BatchJobRequest
    {
        /// <summary>Dataset identifier (e.g. <c>"OPRA.PILLAR"</c>).</summary>
        public string Dataset { get; set; }

        /// <summary>One or more symbols; <c>"ALL_SYMBOLS"</c> as the sole entry requests the whole dataset.</summary>
        public IReadOnlyList<string> Symbols { get; set; }

        /// <summary>Schema name (e.g. <c>"trades"</c>).</summary>
        public string Schema { get; set; }

        /// <summary>Inclusive start of the requested time range (UTC).</summary>
        public DateTimeOffset StartUtc { get; set; }

        /// <summary>Exclusive end of the requested time range (UTC).</summary>
        public DateTimeOffset EndUtc { get; set; }

        /// <summary>Output encoding: <c>"dbn"</c> (the default), <c>"csv"</c> or <c>"json"</c>.</summary>
        public string Encoding { get; set; } = "dbn";

        /// <summary>Output compression: <c>"zstd"</c> (the default) or <c>"none"</c>.</summary>
        public string Compression { get; set; } = "zstd";

        /// <summary>Whether CSV or JSON output writes prices as display decimals rather than integers of 1e-9.</summary>
        public bool PrettyPx { get; set; }

        /// <summary>Whether CSV or JSON output writes timestamps as ISO-8601 strings rather than UNIX nanoseconds.</summary>
        public bool PrettyTs { get; set; }

        /// <summary>Whether CSV or JSON output names each record's symbol.</summary>
        public bool MapSymbols { get; set; }

        /// <summary>
        /// The longest period one output file covers, e.g. <c>"day"</c>, <c>"week"</c>, <c>"month"</c> or <c>"none"</c>;
        /// <see langword="null"/> leaves it to the API, which splits by day.
        /// </summary>
        public string SplitDuration { get; set; }

        /// <summary>Whether the output is split into one file per symbol.</summary>
        public bool SplitSymbols { get; set; }

        /// <summary>The largest size of one output file, in bytes; <see langword="null"/> for no size limit.</summary>
        public long? SplitSize { get; set; }
    }
}
