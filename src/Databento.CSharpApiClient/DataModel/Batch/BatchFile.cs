using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    public sealed class BatchFile
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>Hash of the file content, e.g. "sha256:abc123...".</summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("https_url")]
        public string HttpsUrl { get; set; }

        [JsonPropertyName("ftp_url")]
        public string FtpUrl { get; set; }
    }
}
