using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// Metadata for a single output file produced by a Databento batch job.
    /// Returned inside <see cref="BatchJob.Files"/>.
    /// </summary>
    public sealed class BatchFile
    {
        /// <summary>Output file name (e.g. <c>"data.dbn.zst"</c>).</summary>
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        /// <summary>File size in bytes (uncompressed).</summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>Integrity hash, e.g. <c>"sha256:abc123..."</c>.</summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>HTTPS download URL. Use with <see cref="DatabentoJsonClient.DownloadBatchFileAsync"/>.</summary>
        [JsonPropertyName("https_url")]
        public string HttpsUrl { get; set; }

        /// <summary>FTP download URL (alternative delivery path).</summary>
        [JsonPropertyName("ftp_url")]
        public string FtpUrl { get; set; }
    }
}
