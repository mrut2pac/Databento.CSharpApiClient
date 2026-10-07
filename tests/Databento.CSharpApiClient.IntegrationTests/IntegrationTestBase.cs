using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private readonly List<CapturingHttpTransport> capturingTransports = new List<CapturingHttpTransport>();

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
        /// Skips the test when the exception indicates a missing data subscription,
        /// an endpoint that is not available on the current account, or a schema the
        /// dataset doesn't serve.
        /// Call in a catch block around calls that may fail with 403/404 or when the
        /// schema is not available on the current subscription tier.
        /// </summary>
        protected static void SkipIfNoLicense(DatabentoHttpException ex)
        {
            Skip.If(
                ex.StatusCode == 403
                    || ex.StatusCode == 404
                    || ex.ErrorCase == "license_not_found_unauthorized"
                    || ex.ErrorCase == "dataset_schema_not_supported",
                "Skipped — dataset/schema not available on this subscription: " + ex.Message);
        }

        /// <summary>
        /// Creates a JSON client whose responses are captured, so <see cref="Dispose"/> can check each one against
        /// its response model with <see cref="ResponseModelGuard"/>. The capturing transport mirrors the client's
        /// default one (decompression, headers); <see cref="CreateDefaultJsonClient"/> keeps that one under test.
        /// </summary>
        protected DatabentoJsonClient CreateJsonClient()
        {
            DatabentoOptions options = new DatabentoOptions { ApiKey = this.ApiKey };
            HttpClient http = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }, disposeHandler: true)
            {
                BaseAddress = options.BaseUri,
                Timeout = options.Timeout,
            };
            http.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? "DatabentoJsonClient/1.0");
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(this.ApiKey + ":")));

            CapturingHttpTransport transport = new CapturingHttpTransport(http);
            this.capturingTransports.Add(transport);
            return new DatabentoJsonClient(options, transport);
        }

        /// <summary>Creates a JSON client on its own default transport, which the response guard doesn't see.</summary>
        protected DatabentoJsonClient CreateDefaultJsonClient()
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

        /// <summary>
        /// Fails the test when a response it received doesn't match its model: a key the API sent that no property maps,
        /// or a mapped property that never arrived.
        /// </summary>
        public void Dispose()
        {
            List<string> problems = new List<string>();
            foreach(CapturingHttpTransport transport in this.capturingTransports)
            {
                foreach(CapturedResponse response in transport.Responses)
                {
                    problems.AddRange(ResponseModelGuard.Check(response));
                }
            }

            GC.SuppressFinalize(this);
            if(problems.Count > 0)
            {
                throw new InvalidOperationException("The API's responses don't match the response models:" + Environment.NewLine
                    + string.Join(Environment.NewLine, problems.Distinct()));
            }
        }
    }
}
