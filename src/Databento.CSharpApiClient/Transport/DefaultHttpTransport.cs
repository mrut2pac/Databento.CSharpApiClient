using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Transport
{
    public sealed class DefaultHttpTransport : IHttpTransport
    {
        private readonly HttpClient httpClient;

        public DefaultHttpTransport(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}
