using System;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Configuration options passed to <see cref="DatabentoClient"/> and
    /// <see cref="DatabentoJsonClient"/> at construction time.
    /// </summary>
    public sealed class DatabentoOptions
    {
        /// <summary>Historical API base URL. Defaults to <c>https://hist.databento.com/v0/</c>.</summary>
        public Uri BaseUri { get; set; } = new Uri("https://hist.databento.com/v0/");

        /// <summary>Databento API key used for HTTP Basic authentication. Required.</summary>
        public string ApiKey { get; set; }

        /// <summary>Value sent in the <c>User-Agent</c> header. Defaults to <c>DatabentoClient/1.0</c>.</summary>
        public string UserAgent { get; set; } = "DatabentoClient/1.0";

        /// <summary>Per-request HTTP timeout. Default: 5 minutes (suitable for large timeseries fetches).</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>Maximum number of automatic retries on transient HTTP errors (429, 5xx). Default: 3. Values below 0 are treated as 0.</summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>Base delay for exponential backoff between retries. Default: 1 second.</summary>
        public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>Upper bound for a single backoff delay (also caps a server-provided Retry-After). Default: 30 seconds.</summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    }
}
