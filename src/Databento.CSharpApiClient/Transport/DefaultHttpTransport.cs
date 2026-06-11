using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Default <see cref="IHttpTransport"/> implementation backed by a real <see cref="HttpClient"/>.
    /// Uses <see cref="HttpCompletionOption.ResponseHeadersRead"/> so large response bodies can be
    /// streamed without buffering the entire payload in memory.
    /// </summary>
    public sealed class DefaultHttpTransport : IHttpTransport
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initialises a new instance wrapping the supplied <paramref name="httpClient"/>.
        /// The transport takes ownership of the client and disposes it on <see cref="Dispose"/>.
        /// </summary>
        /// <param name="httpClient">Configured <see cref="HttpClient"/> with base address and auth headers set.</param>
        public DefaultHttpTransport(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}
