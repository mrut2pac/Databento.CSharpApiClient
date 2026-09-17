using System.Net;
using System.Net.Http;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Builds the message handlers the clients send through. The API honours <c>Accept-Encoding</c> and
    /// answers with <c>Content-Encoding: gzip</c>, and market data is highly repetitive, so negotiating
    /// compression makes a timeseries response roughly an order of magnitude smaller on the wire.
    /// </summary>
    /// <remarks>
    /// Compression here is transport-level and transparent: the runtime decodes the body before it reaches
    /// the caller, so a response stream reads exactly as it did before. It is deliberately not the API's own
    /// <c>compression</c> query parameter, which frames the payload itself and would need a decoder this
    /// package does not carry.
    /// <para>
    /// Batch artifacts are downloaded through <see cref="CreatePlainHandler"/> instead. They are already
    /// compressed, so negotiating compression gains nothing, and if such a file were ever served with a
    /// <c>Content-Encoding</c> header the runtime would decode it in flight and hand back bytes that no
    /// longer match the file name or its published hash.
    /// </para>
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

        /// <summary>
        /// Creates a handler that neither negotiates nor decodes compression, so a response body is
        /// delivered exactly as it was stored.
        /// </summary>
        /// <returns>The handler to construct an <see cref="HttpClient"/> with.</returns>
        internal static HttpClientHandler CreatePlainHandler()
        {
            return new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None,
            };
        }
    }
}
