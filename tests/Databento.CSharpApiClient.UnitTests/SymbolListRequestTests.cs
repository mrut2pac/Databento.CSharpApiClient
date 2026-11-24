using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// The requests that carry a symbol list (timeseries.get_range in both encodings, and the cost, record count and billable size
    /// estimates) send their parameters as a POST form, as Databento's own client does, so a request can carry the 2,000 symbols the
    /// API takes. In a GET query string fewer than 300 OPRA symbols already pass the 8 KB a server accepts.
    /// </summary>
    [TestClass]
    public class SymbolListRequestTests
    {
        private const string ApiKey = "test-api-key-0000";

        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 16, 13, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset End = new DateTimeOffset(2026, 10, 16, 20, 15, 0, TimeSpan.Zero);

        [TestMethod]
        public async Task GetCbbo1mAsync_SeveralSymbols_PostsItsParametersAsAForm()
        {
            List<CapturedRequest> captured = new List<CapturedRequest>();
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(captured, _ => Ok(string.Empty))))
            {
                await client.GetCbbo1mAsync("OPRA.PILLAR", new[] { "SPXW  261016C06600000", "SPXW  261016P06600000" }, Start, End);
            }

            CapturedRequest request = captured.Single();
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("timeseries.get_range", request.Uri, "nothing goes in the query string");
            CollectionAssert.AreEquivalent(
                new Dictionary<string, string>
                {
                    ["dataset"] = "OPRA.PILLAR",
                    ["schema"] = "cbbo-1m",
                    ["symbols"] = "SPXW  261016C06600000,SPXW  261016P06600000",
                    ["start"] = "2026-10-16T13:30:00Z",
                    ["end"] = "2026-10-16T20:15:00Z",
                    ["stype_in"] = "raw_symbol",
                    ["pretty_px"] = "true",
                    ["pretty_ts"] = "true",
                    ["encoding"] = "json",
                    ["compression"] = "none",
                    ["map_symbols"] = "true",
                }.ToList(),
                request.Form.ToList());
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_TwoThousandOptionSymbols_AllGoInTheBody()
        {
            // Databento takes 2,000 symbols a request; an OPRA symbol escapes to about 30 bytes of a query string
            string[] symbols = Enumerable.Range(0, 2000).Select(strike => FormattableString.Invariant($"SPXW  261016C{strike * 1000:00000000}")).ToArray();
            List<CapturedRequest> captured = new List<CapturedRequest>();
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(captured, _ => Ok(string.Empty))))
            {
                await client.GetCbbo1mAsync("OPRA.PILLAR", symbols, Start, End);
            }

            CapturedRequest request = captured.Single();
            Assert.AreEqual("timeseries.get_range", request.Uri);
            CollectionAssert.AreEqual(symbols, request.Form["symbols"].Split(','));
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_ServerErrorThenAnswer_SendsTheSameFormAgain()
        {
            // a request's content is read when it is sent, so each attempt needs its own
            List<CapturedRequest> captured = new List<CapturedRequest>();
            IHttpTransport transport = Transport(captured, call => call == 1 ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("busy") } : Ok(string.Empty));
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(maxRetries: 1), transport))
            {
                await client.GetCbbo1mAsync("OPRA.PILLAR", new[] { "SPXW  261016C06600000" }, Start, End);
            }

            Assert.AreEqual(2, captured.Count);
            Assert.AreEqual(HttpMethod.Post, captured[1].Method);
            CollectionAssert.AreEquivalent(captured[0].Form.ToList(), captured[1].Form.ToList());
        }

        [TestMethod]
        public async Task StreamCbbo1mAsync_SingleSymbol_PostsItsParametersAsAFormWithoutMapSymbols()
        {
            List<CapturedRequest> captured = new List<CapturedRequest>();
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(captured, _ => Ok(string.Empty))))
            {
                await foreach(object _ in client.StreamCbbo1mAsync("OPRA.PILLAR", "SPXW  261016C06600000", Start, End))
                {
                }
            }

            CapturedRequest request = captured.Single();
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("timeseries.get_range", request.Uri, "nothing goes in the query string");
            Assert.AreEqual("SPXW  261016C06600000", request.Form["symbols"]);
            Assert.IsFalse(request.Form.ContainsKey("map_symbols"), "the caller knows the symbol of a single-symbol request");
        }

        [TestMethod]
        [DataRow("metadata.get_cost", "0.25")]
        [DataRow("metadata.get_record_count", "42")]
        [DataRow("metadata.get_billable_size", "4096")]
        public async Task Estimate_SeveralSymbols_PostsItsParametersAsAForm(string endpoint, string answer)
        {
            List<CapturedRequest> captured = new List<CapturedRequest>();
            string[] symbols = { "SPXW  261016C06600000", "SPXW  261016P06600000" };
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(captured, _ => Ok(answer))))
            {
                switch(endpoint)
                {
                    case "metadata.get_cost":
                        Assert.AreEqual(0.25, await client.GetCostAsync("OPRA.PILLAR", symbols, "cbbo-1m", Start, End));
                        break;
                    case "metadata.get_record_count":
                        Assert.AreEqual(42L, await client.GetRecordCountAsync("OPRA.PILLAR", symbols, "cbbo-1m", Start, End));
                        break;
                    default:
                        Assert.AreEqual(4096L, await client.GetBillableSizeAsync("OPRA.PILLAR", symbols, "cbbo-1m", Start, End));
                        break;
                }
            }

            CapturedRequest request = captured.Single();
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(endpoint, request.Uri);
            CollectionAssert.AreEquivalent(
                new Dictionary<string, string>
                {
                    ["dataset"] = "OPRA.PILLAR",
                    ["schema"] = "cbbo-1m",
                    ["symbols"] = "SPXW  261016C06600000,SPXW  261016P06600000",
                    ["start"] = "2026-10-16T13:30:00Z",
                    ["end"] = "2026-10-16T20:15:00Z",
                    ["stype_in"] = "raw_symbol",
                }.ToList(),
                request.Form.ToList());
        }

        [TestMethod]
        public async Task DbnStreamTcbboAsync_SeveralSymbols_PostsItsParametersAsAForm()
        {
            List<CapturedRequest> captured = new List<CapturedRequest>();
            IHttpTransport transport = Transport(captured, _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(DbnBinaryBuilder.BuildTcbboStream()) });
            using(DatabentoClient client = new DatabentoClient(Options(), transport))
            {
                await foreach(TcbboRecordDbn _ in client.StreamTcbboAsync("OPRA.PILLAR", new[] { "SPXW  261016C06600000", "SPXW  261016P06600000" }, Start, End))
                {
                }
            }

            CapturedRequest request = captured.Single();
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("timeseries.get_range", request.Uri);
            CollectionAssert.AreEquivalent(
                new Dictionary<string, string>
                {
                    ["dataset"] = "OPRA.PILLAR",
                    ["schema"] = "tcbbo",
                    ["symbols"] = "SPXW  261016C06600000,SPXW  261016P06600000",
                    ["start"] = "2026-10-16T13:30:00Z",
                    ["end"] = "2026-10-16T20:15:00Z",
                    ["stype_in"] = "raw_symbol",
                    ["encoding"] = "dbn",
                    ["compression"] = "none",
                }.ToList(),
                request.Form.ToList());
        }

        [TestMethod]
        public async Task DbnStreamTcbboAsync_ServerErrorThenAnswer_SendsTheSameFormAgain()
        {
            // a request's content is disposed with it, so each attempt needs its own
            List<CapturedRequest> captured = new List<CapturedRequest>();
            IHttpTransport transport = Transport(captured, call => call == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("busy") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(DbnBinaryBuilder.BuildTcbboStream()) });
            using(DatabentoClient client = new DatabentoClient(Options(maxRetries: 1), transport))
            {
                await foreach(TcbboRecordDbn _ in client.StreamTcbboAsync("OPRA.PILLAR", new[] { "SPXW  261016C06600000" }, Start, End))
                {
                }
            }

            Assert.AreEqual(2, captured.Count);
            CollectionAssert.AreEquivalent(captured[0].Form.ToList(), captured[1].Form.ToList());
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_Form_IsSentAsFormUrlEncoded()
        {
            // a form parser reads no other content type
            List<CapturedRequest> captured = new List<CapturedRequest>();
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(captured, _ => Ok(string.Empty))))
            {
                await client.GetCbbo1mAsync("OPRA.PILLAR", new[] { "SPXW  261016C06600000" }, Start, End);
            }

            Assert.AreEqual("application/x-www-form-urlencoded", captured.Single().ContentType);
        }

        private static DatabentoOptions Options(int maxRetries = 0)
        {
            return new DatabentoOptions
            {
                ApiKey = ApiKey,
                MaxRetries = maxRetries,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(10),
            };
        }

        private static HttpResponseMessage Ok(string body)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }

        /// <summary>
        /// A transport that records each request as it is sent, its form read then, since the client disposes the request afterwards
        /// </summary>
        private static IHttpTransport Transport(List<CapturedRequest> captured, Func<int, HttpResponseMessage> respond)
        {
            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured.Add(CapturedRequest.Of(request)))
                .ReturnsAsync(() => respond(captured.Count));
            transport.Setup(t => t.Dispose());
            return transport.Object;
        }

        /// <summary>
        /// A request as it was sent: its method, its whole URI and its decoded form fields, none for a request without a body
        /// </summary>
        private sealed class CapturedRequest
        {
            private CapturedRequest(HttpMethod method, string uri, Dictionary<string, string> form, string contentType)
            {
                this.Method = method;
                this.Uri = uri;
                this.Form = form;
                this.ContentType = contentType;
            }

            public HttpMethod Method { get; }

            public string ContentType { get; }

            public string Uri { get; }

            public Dictionary<string, string> Form { get; }

            public static CapturedRequest Of(HttpRequestMessage request)
            {
                Dictionary<string, string> form = new Dictionary<string, string>(StringComparer.Ordinal);
                if(request.Content != null)
                {
                    string body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    foreach(string field in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] pair = field.Split('=', 2);
                        form.Add(WebUtility.UrlDecode(pair[0]), WebUtility.UrlDecode(pair[1]));
                    }
                }

                return new CapturedRequest(request.Method, request.RequestUri.ToString(), form, request.Content?.Headers.ContentType?.MediaType);
            }
        }
    }
}
