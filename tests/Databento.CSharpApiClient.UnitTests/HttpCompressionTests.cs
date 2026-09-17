using System.Net;
using System.Net.Http;

using Databento.CSharpApiClient.Transport;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="HttpCompression"/> — the handler the clients send through must negotiate
    /// a compressed response, because market data compresses by roughly an order of magnitude.
    /// </summary>
    [TestClass]
    public class HttpCompressionTests
    {
        [TestMethod]
        public void CreateDecompressingHandler_NegotiatesGzip()
        {
            using HttpClientHandler handler = HttpCompression.CreateDecompressingHandler();

            // the API answers Accept-Encoding with Content-Encoding: gzip, so asking for it is the whole
            // difference between a timeseries response costing its full size on the wire and a tenth of it
            Assert.IsTrue(handler.AutomaticDecompression.HasFlag(DecompressionMethods.GZip));
        }

        [TestMethod]
        public void CreateDecompressingHandler_NegotiatesEveryEncodingTheRuntimeCanDecode()
        {
            using HttpClientHandler handler = HttpCompression.CreateDecompressingHandler();

            // nothing is gained by naming a subset - whatever the server picks, the runtime decodes it
            // before the body reaches the caller
            Assert.AreEqual(DecompressionMethods.All, handler.AutomaticDecompression);
        }
    }
}
