using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Databento.CSharpApiClient.Exceptions;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Base class for all integration tests.
    /// Reads the API key from the <c>DATABENTO_API_KEY</c> environment variable.
    /// Individual tests must call <see cref="SkipIfNoApiKey"/> at their start so
    /// that tests are cleanly skipped in CI environments without credentials.
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly string ApiKey;

        protected IntegrationTestBase()
        {
            this.ApiKey = Environment.GetEnvironmentVariable("DATABENTO_API_KEY") ?? string.Empty;
        }

        /// <summary>
        /// Skips the test when <c>DATABENTO_API_KEY</c> is not set.
        /// Call at the very start of every <c>[SkippableFact]</c> test body.
        /// </summary>
        protected void SkipIfNoApiKey()
        {
            Skip.If(string.IsNullOrWhiteSpace(this.ApiKey), "DATABENTO_API_KEY environment variable not set.");
        }

        /// <summary>
        /// Skips the test when the exception indicates a missing data subscription.
        /// Call in a catch block around calls that may fail with 403.
        /// </summary>
        protected static void SkipIfNoLicense(DatabentoHttpException ex)
        {
            Skip.If(
                ex.StatusCode == 403 || ex.ErrorCase == "license_not_found_unauthorized",
                "Skipped — no subscription for this dataset: " + ex.Message);
        }

        protected DatabentoJsonClient CreateJsonClient()
            => new DatabentoJsonClient(new DatabentoOptions { ApiKey = this.ApiKey });

        protected DatabentoClient CreateBinaryClient()
            => new DatabentoClient(new DatabentoOptions { ApiKey = this.ApiKey });

        /// <summary>
        /// Fetches the raw response body for a <c>timeseries.get_range</c> request.
        /// Used by diagnostic tests to expose the actual API JSON when deserialization fails.
        /// </summary>
        protected string FetchRawTimeseries(string dataset, string schema, string symbol, string start, string end)
        {
            using(HttpClient http = new HttpClient())
            {
                string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.ApiKey + ":"));
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

                string url = "https://hist.databento.com/v0/timeseries.get_range"
                    + "?dataset=" + Uri.EscapeDataString(dataset)
                    + "&schema=" + Uri.EscapeDataString(schema)
                    + "&symbols=" + Uri.EscapeDataString(symbol)
                    + "&start=" + Uri.EscapeDataString(start)
                    + "&end=" + Uri.EscapeDataString(end)
                    + "&stype_in=raw_symbol&pretty_px=true&pretty_ts=true&encoding=json&compression=none";

                return http.GetStringAsync(url).GetAwaiter().GetResult();
            }
        }

        public void Dispose() { }
    }
}
