using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Abstraction over <see cref="HttpClient"/> used by the Databento clients.
    /// Implement this interface to substitute a mock transport in unit tests.
    /// </summary>
    public interface IHttpTransport : IDisposable
    {
        /// <summary>
        /// Sends an HTTP request and returns the response.
        /// Callers are responsible for disposing the returned <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="request">The HTTP request to send.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
