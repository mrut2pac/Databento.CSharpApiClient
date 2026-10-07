using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Sends requests like the client's own transport and keeps every successful JSON response body,
    /// so <see cref="ResponseModelGuard"/> can compare what the API sent against the response models.
    /// </summary>
    internal sealed class CapturingHttpTransport : IHttpTransport
    {
        private readonly HttpClient httpClient;

        private readonly List<CapturedResponse> responses = new List<CapturedResponse>();

        public CapturingHttpTransport(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>The successful JSON responses sent through this transport, in order.</summary>
        public IReadOnlyList<CapturedResponse> Responses
        {
            get
            {
                lock(this.responses)
                {
                    return this.responses.ToArray();
                }
            }
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // headers first, as the client's own transport reads them; the body is buffered only to keep a copy
            HttpResponseMessage response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if(response.IsSuccessStatusCode)
            {
                await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                string requestBody = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                lock(this.responses)
                {
                    this.responses.Add(new CapturedResponse(
                        new Uri(this.httpClient.BaseAddress, request.RequestUri),
                        requestBody,
                        response.Content.Headers.ContentType?.MediaType,
                        body));
                }
            }

            return response;
        }

        public void Dispose() => this.httpClient.Dispose();
    }

    /// <summary>One captured response: the absolute request URI, the form body sent with it, the media type and the body returned.</summary>
    internal sealed record CapturedResponse(Uri RequestUri, string RequestBody, string ContentType, string Body);
}
