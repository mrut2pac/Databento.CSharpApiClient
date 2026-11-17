// Ignore Spelling: Api Databento Json

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Databento Historical HTTP API client — JSON encoding.
    /// Covers timeseries (all supported schemas), metadata, batch, and symbology endpoints.
    /// </summary>
    public sealed partial class DatabentoJsonClient : IDisposable
    {
        /// <summary>
        /// The symbol a caller passes as the sole entry to request the whole dataset.
        /// </summary>
        private const string AllSymbols = "ALL_SYMBOLS";

        private readonly DatabentoOptions options;
        private readonly IHttpTransport transport;

        /// <summary>
        /// Transport used for batch artifact downloads, which must deliver a body exactly as stored.
        /// The same instance as <see cref="transport"/> when the caller supplied its own.
        /// </summary>
        private readonly IHttpTransport downloadTransport;

        // STJ options: property names are set via [JsonPropertyName] attributes on each model class,
        // so no special naming policy is needed here. Unknown JSON properties are silently ignored
        // by default. DateTime values from Databento carry Z-suffix when pretty_ts=true, so STJ
        // correctly sets Kind=Utc without additional configuration.
        private static readonly JsonSerializerOptions RecordDeserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            // The Historical API serialises numeric fields as JSON strings when pretty_px=true
            // (e.g. "price":"4.000000000", "bid_px":"3.700000000"); read those into the numeric DTO
            // properties instead of throwing on the string token.
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        /// <summary>
        /// Initialises a new <see cref="DatabentoJsonClient"/> using the supplied options.
        /// A default <see cref="HttpClient"/> with Basic authentication is created unless a custom
        /// <paramref name="transport"/> is provided (useful for unit testing).
        /// </summary>
        /// <param name="options">Connection and authentication settings.</param>
        /// <param name="transport">Optional custom HTTP transport; leave <see langword="null"/> to use the built-in client.</param>
        public DatabentoJsonClient(DatabentoOptions options, IHttpTransport transport = null)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if(string.IsNullOrWhiteSpace(this.options.ApiKey))
            {
                throw new ArgumentException("ApiKey must be set.", nameof(options));
            }

            if(transport != null)
            {
                // a caller bringing its own transport owns this decision for every request it carries
                this.transport = transport;
                this.downloadTransport = transport;
            }
            else
            {
                HttpClientHandler handler = this.options.RequestCompressedResponses
                    ? HttpCompression.CreateDecompressingHandler()
                    : HttpCompression.CreatePlainHandler();

                this.transport = new DefaultHttpTransport(this.CreateHttpClient(handler));

                // an artifact is already compressed, so negotiating it back gains nothing and risks the
                // runtime decoding a stored file in flight
                this.downloadTransport = this.options.RequestCompressedResponses
                    ? new DefaultHttpTransport(this.CreateHttpClient(HttpCompression.CreatePlainHandler()))
                    : this.transport;
            }
        }

        /// <summary>
        /// Builds the authenticated <see cref="HttpClient"/> the transports send through.
        /// </summary>
        /// <param name="handler">Handler deciding whether the response is negotiated compressed.</param>
        /// <returns>The configured client, which takes ownership of the handler.</returns>
        private HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            HttpClient httpClient = new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = this.options.BaseUri,
                Timeout = this.options.Timeout,
            };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(this.options.UserAgent ?? "DatabentoJsonClient/1.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string apiKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.options.ApiKey + ":"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKeyBase64);

            return httpClient;
        }

        /// <summary>Releases the underlying HTTP transports (and their <see cref="HttpClient"/>s).</summary>
        public void Dispose()
        {
            if(!ReferenceEquals(this.downloadTransport, this.transport))
            {
                this.downloadTransport.Dispose();
            }

            this.transport.Dispose();
        }
    }
}
