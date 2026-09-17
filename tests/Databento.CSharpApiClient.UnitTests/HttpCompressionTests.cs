using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Transport;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="HttpCompression"/>. These drive a real loopback server rather than
    /// asserting the handler's own settings back, because what matters is what reaches the caller:
    /// a negotiated response must decode transparently, and a stored artifact must not be touched.
    /// </summary>
    [TestClass]
    public class HttpCompressionTests
    {
        private const string Payload = "the quick brown fox jumps over the lazy dog";

        [TestMethod]
        public async Task DecompressingHandler_GzipResponse_ReachesTheCallerDecoded()
        {
            using LoopbackServer server = new LoopbackServer(compressResponse: true);
            using HttpClient client = new HttpClient(HttpCompression.CreateDecompressingHandler(), disposeHandler: true);

            // the whole claim of the change: the parsing path downstream sees exactly what it saw before
            string body = await client.GetStringAsync(server.Uri);

            Assert.AreEqual(Payload, body);
            StringAssert.Contains(server.LastAcceptEncoding, "gzip");
        }

        [TestMethod]
        public async Task PlainHandler_NegotiatesNothing_SoAStoredFileIsDeliveredAsStored()
        {
            using LoopbackServer server = new LoopbackServer(compressResponse: false);
            using HttpClient client = new HttpClient(HttpCompression.CreatePlainHandler(), disposeHandler: true);

            // a batch artifact is already compressed; if the runtime decoded it in flight the bytes would
            // no longer match the file name it is saved under, nor its published hash
            byte[] body = await client.GetByteArrayAsync(server.Uri);

            Assert.AreEqual(Payload, Encoding.UTF8.GetString(body));
            Assert.AreEqual("<none>", server.LastAcceptEncoding);
        }

        [TestMethod]
        public async Task PlainHandler_GzipBody_IsLeftCompressed()
        {
            using LoopbackServer server = new LoopbackServer(compressResponse: true);
            using HttpClient client = new HttpClient(HttpCompression.CreatePlainHandler(), disposeHandler: true);

            // the server compressed it anyway - the point is that this handler does not silently undo it
            byte[] body = await client.GetByteArrayAsync(server.Uri);

            Assert.AreNotEqual(Payload, Encoding.UTF8.GetString(body));
            Assert.AreEqual(0x1f, body[0]);
            Assert.AreEqual(0x8b, body[1]);
        }

        [TestMethod]
        public async Task DownloadBatchFile_GzipArtifact_IsDeliveredAsStored()
        {
            // the wiring, not just the handler: a batch artifact goes out over the download transport, so a
            // stored .gz must arrive as a .gz even though every other call on this client negotiates gzip
            using LoopbackServer server = new LoopbackServer(compressResponse: true);
            using DatabentoJsonClient client = new DatabentoJsonClient(new DatabentoOptions { ApiKey = "db-test" });

            using Stream artifact = await client.DownloadBatchFileAsync(server.Uri.ToString());
            using MemoryStream received = new MemoryStream();
            await artifact.CopyToAsync(received);
            byte[] body = received.ToArray();

            Assert.AreEqual(0x1f, body[0]);
            Assert.AreEqual(0x8b, body[1]);
            Assert.AreEqual("<none>", server.LastAcceptEncoding);
        }

        /// <summary>
        /// A one-request HTTP server on loopback, recording the <c>Accept-Encoding</c> it was sent.
        /// </summary>
        private sealed class LoopbackServer : IDisposable
        {
            private readonly HttpListener listener;
            private readonly Task serving;

            internal LoopbackServer(bool compressResponse)
            {
                int port = GetFreePort();
                this.Uri = new Uri(FormattableString.Invariant($"http://localhost:{port}/"));

                this.listener = new HttpListener();
                this.listener.Prefixes.Add(this.Uri.ToString());
                this.listener.Start();

                this.serving = Task.Run(async () =>
                {
                    HttpListenerContext context = await this.listener.GetContextAsync().ConfigureAwait(false);
                    this.LastAcceptEncoding = context.Request.Headers["Accept-Encoding"] ?? "<none>";

                    byte[] body = Encoding.UTF8.GetBytes(Payload);
                    if(compressResponse)
                    {
                        body = Gzip(body);
                        context.Response.AddHeader("Content-Encoding", "gzip");
                    }

                    context.Response.ContentLength64 = body.Length;
                    await context.Response.OutputStream.WriteAsync(body, 0, body.Length).ConfigureAwait(false);
                    context.Response.Close();
                });
            }

            internal Uri Uri { get; }

            internal string LastAcceptEncoding { get; private set; }

            public void Dispose()
            {
                try
                {
                    this.serving.Wait(TimeSpan.FromSeconds(10));
                }
                catch(AggregateException)
                {
                    // the test has its assertion either way; a server fault must not mask it
                }

                this.listener.Stop();
                ((IDisposable)this.listener).Dispose();
            }

            private static byte[] Gzip(byte[] body)
            {
                using MemoryStream output = new MemoryStream();
                using(GZipStream gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
                {
                    gzip.Write(body, 0, body.Length);
                }

                return output.ToArray();
            }

            private static int GetFreePort()
            {
                System.Net.Sockets.TcpListener probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
                probe.Start();
                int port = ((IPEndPoint)probe.LocalEndpoint).Port;
                probe.Stop();

                return port;
            }
        }
    }
}
