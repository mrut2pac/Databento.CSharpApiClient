using System.Net;
using System.Net.Http;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Builds the message handler the clients send through. Its one job is to negotiate a compressed
    /// response: the API honours <c>Accept-Encoding</c> and answers with <c>Content-Encoding: gzip</c>,
    /// and market data is highly repetitive, so a timeseries response compresses by roughly an order of
    /// magnitude on the wire.
    /// </summary>
    /// <remarks>
    /// This is transport-level and entirely transparent. The handler decompresses before the body reaches
    /// the caller, so a response stream reads exactly as it did before and nothing downstream changes.
    /// It is deliberately not the API's own <c>compression</c> query parameter, which frames the payload
    /// itself and would need a decoder this package does not carry.
    /// </remarks>
    internal static class HttpCompression
    {
        /// <summary>
        /// Creates a handler that asks for, and transparently decodes, a compressed response.
        /// </summary>
        /// <returns>The handler to construct an <see cref="HttpClient"/> with.</returns>
        internal static HttpClientHandler CreateDecompressingHandler()
        {
            return new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
            };
        }
    }
}
