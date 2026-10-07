using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// Metadata for a single output file produced by a Databento batch job, as returned by <c>batch.list_files</c>.
    /// </summary>
    public sealed class BatchFile
    {
        /// <summary>Output file name (e.g. <c>"data.dbn.zst"</c>).</summary>
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        /// <summary>File size in bytes.</summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>Integrity hash, e.g. <c>"sha256:abc123..."</c>.</summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>The file's download URLs, by protocol.</summary>
        [JsonPropertyName("urls")]
        public BatchFileUrls Urls { get; set; }

        /// <summary>HTTPS download URL, from <see cref="Urls"/>. Use with <see cref="DatabentoJsonClient.DownloadBatchFileAsync"/>.</summary>
        [JsonIgnore]
        public string HttpsUrl
        {
            get => this.httpsUrl ?? this.Urls?.Https;
            set => this.httpsUrl = value;
        }

        /// <summary>FTP download URL (alternative delivery path), from <see cref="Urls"/>.</summary>
        [JsonIgnore]
        public string FtpUrl
        {
            get => this.ftpUrl ?? this.Urls?.Ftp;
            set => this.ftpUrl = value;
        }

        private string httpsUrl;

        private string ftpUrl;
    }
}
