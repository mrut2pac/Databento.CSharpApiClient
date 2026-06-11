using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    public sealed class BatchFile
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>Hash of the file content, e.g. "sha256:abc123...".</summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("https_url")]
        public string HttpsUrl { get; set; }

        [JsonProperty("ftp_url")]
        public string FtpUrl { get; set; }
    }
}
