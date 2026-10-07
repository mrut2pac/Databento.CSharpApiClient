using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// The download URLs of one batch output file, as listed in <see cref="BatchFile.Urls"/>.
    /// </summary>
    public sealed class BatchFileUrls
    {
        /// <summary>HTTPS download URL. Use with <see cref="DatabentoJsonClient.DownloadBatchFileAsync"/>.</summary>
        [JsonPropertyName("https")]
        public string Https { get; set; }

        /// <summary>FTP download URL.</summary>
        [JsonPropertyName("ftp")]
        public string Ftp { get; set; }
    }
}
