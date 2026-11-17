// Ignore Spelling: Api

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Databento historical data service DBN-binary client.
    /// Covers <c>timeseries.get_range</c> for all supported DBN schemas.
    /// </summary>
    public sealed partial class DatabentoClient : IDisposable
    {
        private readonly DatabentoOptions options;
        private readonly IHttpTransport transport;

        /// <summary>
        /// Initialises a new <see cref="DatabentoClient"/> using the supplied options.
        /// A default <see cref="HttpClient"/> with Basic authentication is created unless a custom
        /// <paramref name="transport"/> is provided (useful for unit testing).
        /// </summary>
        /// <param name="options">Connection and authentication settings.</param>
        /// <param name="transport">Optional custom HTTP transport; leave <see langword="null"/> to use the built-in client.</param>
        public DatabentoClient(DatabentoOptions options, IHttpTransport transport = null)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if(string.IsNullOrWhiteSpace(this.options.ApiKey))
            {
                throw new ArgumentException("ApiKey needs to be specified, otherwise no data will be provided!", nameof(options));
            }

            if(transport != null)
            {
                this.transport = transport;
            }
            else
            {
                HttpClientHandler handler = this.options.RequestCompressedResponses
                    ? HttpCompression.CreateDecompressingHandler()
                    : HttpCompression.CreatePlainHandler();

                HttpClient httpClient = new HttpClient(handler, disposeHandler: true);
                httpClient.BaseAddress = this.options.BaseUri;
                httpClient.Timeout = this.options.Timeout;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(this.options.UserAgent ?? "DatabentoClient/1.0");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                string apiKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.options.ApiKey + ":"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKeyBase64);

                this.transport = new DefaultHttpTransport(httpClient);
            }
        }

        /// <summary>Releases the underlying HTTP transport (and its <see cref="HttpClient"/>).</summary>
        public void Dispose()
        {
            this.transport.Dispose();
        }
    }
}
