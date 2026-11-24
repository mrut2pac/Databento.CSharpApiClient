// Ignore Spelling: Databento Cbbo Tcbbo Json Dbn

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// <see cref="IDatabentoTimeseriesClient"/> and its extensions offer every time-series call whose records have a record interface,
    /// and both clients answer each with what their own call answers, in an array of the interface.
    /// </summary>
    [TestClass]
    public class TimeseriesClientInterfaceTests
    {
        private const string Dataset = "OPRA.PILLAR";

        private const string Symbol = "SPXW  261016C06600000";

        private static readonly string[] Symbols = { "SPXW  261016C06600000", "SPXW  261016P06600000" };

        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 16, 13, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset End = new DateTimeOffset(2026, 10, 16, 20, 15, 0, TimeSpan.Zero);

        [TestMethod]
        public void Interface_HoldsOnlyAsyncCallsForSeveralSymbols()
        {
            List<string> other = typeof(IDatabentoTimeseriesClient).GetMethods()
                .Where(member => !member.Name.EndsWith("Async", StringComparison.Ordinal) || Parameters(member)[1] != typeof(IReadOnlyList<string>))
                .Select(Signature)
                .ToList();

            Assert.AreEqual(11, typeof(IDatabentoTimeseriesClient).GetMethods().Length);
            Assert.AreEqual(0, other.Count, "The one-symbol and sync forms belong in the extensions: " + string.Join("; ", other));
        }

        [TestMethod]
        [DataRow(typeof(DatabentoClient))]
        [DataRow(typeof(DatabentoJsonClient))]
        public void InterfaceAndExtensions_OfferEveryCallOfTheClientWhoseRecordsHaveARecordInterface(Type client)
        {
            Assert.IsTrue(typeof(IDatabentoTimeseriesClient).IsAssignableFrom(client));
            Type[] recordInterfaces = typeof(IRecord).Assembly.GetExportedTypes()
                .Where(type => type.IsInterface && type != typeof(IRecord) && typeof(IRecord).IsAssignableFrom(type))
                .ToArray();
            List<MethodInfo> calls = client.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.Name.StartsWith("Get", StringComparison.Ordinal) && method.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Where(method => RecordType(method) is Type record && recordInterfaces.Any(recordInterface => recordInterface.IsAssignableFrom(record)))
                .ToList();
            List<MethodInfo> offered = OfferedCalls().ToList();

            List<string> missing = calls.Where(call => OfferedCallFor(offered, call) == null).Select(Signature).ToList();
            List<string> extra = offered
                .Where(member => !calls.Any(call => call.Name == member.Name && OwnParameters(member).SequenceEqual(Parameters(call))))
                .Select(Signature)
                .ToList();
            List<string> wrongRecords = calls
                .Where(call => OfferedCallFor(offered, call) is MethodInfo member && !RecordType(member).IsAssignableFrom(RecordType(call)))
                .Select(Signature)
                .ToList();

            Assert.AreEqual(44, calls.Count, "the clients' calls with a record interface");
            Assert.AreEqual(0, missing.Count, "Not offered: " + string.Join("; ", missing));
            Assert.AreEqual(0, extra.Count, "Not on the client: " + string.Join("; ", extra));
            Assert.AreEqual(0, wrongRecords.Count, "Records not of the record interface: " + string.Join("; ", wrongRecords));
        }

        [TestMethod]
        [DataRow(typeof(DatabentoClient))]
        [DataRow(typeof(DatabentoJsonClient))]
        public async Task EveryOfferedCall_SendsTheRequestOfTheClientsOwnCall(Type clientType)
        {
            List<string> different = new List<string>();
            foreach(MethodInfo member in OfferedCalls())
            {
                MethodInfo own = clientType.GetMethod(member.Name, OwnParameters(member));
                CapturedRequest throughInterface = await CaptureAsync(clientType, member, CancellationToken.None).ConfigureAwait(false);
                CapturedRequest throughClient = await CaptureAsync(clientType, own, CancellationToken.None).ConfigureAwait(false);
                if(throughInterface.ToString() != throughClient.ToString())
                {
                    different.Add(Signature(member) + ": " + throughInterface + " vs " + throughClient);
                }
            }

            Assert.AreEqual(0, different.Count, string.Join(Environment.NewLine, different));
        }

        [TestMethod]
        [DataRow(typeof(DatabentoClient))]
        [DataRow(typeof(DatabentoJsonClient))]
        public async Task EveryOfferedAsyncCall_PassesItsCancellationTokenOn(Type clientType)
        {
            List<string> notCanceled = new List<string>();
            using(CancellationTokenSource canceled = new CancellationTokenSource())
            {
                canceled.Cancel();
                foreach(MethodInfo member in OfferedCalls().Where(member => member.Name.EndsWith("Async", StringComparison.Ordinal)))
                {
                    try
                    {
                        await CaptureAsync(clientType, member, canceled.Token).ConfigureAwait(false);
                        notCanceled.Add(Signature(member));
                    }
                    catch(OperationCanceledException)
                    {
                        // the token reached the client's request
                    }
                }
            }

            Assert.AreEqual(0, notCanceled.Count, "Ran to the end with a canceled token: " + string.Join("; ", notCanceled));
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_DbnClientThroughTheInterface_ReturnsItsRecordsInAnArrayOfTheInterface()
        {
            byte[] stream = DbnBinaryBuilder.BuildCbboStream(Cbbo(6475.25), Cbbo(6475.50));
            using(DatabentoClient client = new DatabentoClient(Options(), Transport(new List<CapturedRequest>(), () => Bytes(stream))))
            {
                ICbboRecord[] records = await ((IDatabentoTimeseriesClient)client).GetCbbo1mAsync(Dataset, Symbols, Start, End).ConfigureAwait(false);

                Assert.AreEqual(typeof(ICbboRecord[]), records.GetType());
                CollectionAssert.AreEqual(new double?[] { 6475.25, 6475.50 }, records.Select(record => record.BidPrice).ToArray());
                records[0] = new Mock<ICbboRecord>().Object;
            }
        }

        [TestMethod]
        public void GetTcbbo_JsonClientThroughAnExtensionForOneSymbol_ReturnsItsRecordsWithTheSymbol()
        {
            string line = "{\"hd\":{\"rtype\":194,\"publisher_id\":1,\"instrument_id\":42,\"ts_event\":\"2026-10-16T13:30:00.000000000Z\"},"
                + "\"price\":\"12.35\",\"size\":3,\"action\":\"T\",\"side\":\"B\",\"flags\":0,\"ts_recv\":\"2026-10-16T13:30:00.000000100Z\","
                + "\"levels\":[{\"bid_px\":\"12.30\",\"ask_px\":\"12.40\",\"bid_sz\":7,\"ask_sz\":9,\"bid_pb\":1,\"ask_pb\":2}]}";
            using(DatabentoJsonClient client = new DatabentoJsonClient(Options(), Transport(new List<CapturedRequest>(), () => Text(line))))
            {
                ITcbboRecord[] records = ((IDatabentoTimeseriesClient)client).GetTcbbo(Dataset, Symbol, Start, End);

                Assert.AreEqual(typeof(ITcbboRecord[]), records.GetType());
                Assert.AreEqual(1, records.Length);
                Assert.AreEqual(12.35, records[0].Price);
                Assert.AreEqual(Symbol, records[0].Symbol, "a one-symbol call's records carry its symbol");
                records[0] = new Mock<ITcbboRecord>().Object;
            }
        }

        /// <summary>
        /// The calls offered on <see cref="IDatabentoTimeseriesClient"/>: its own and its extensions
        /// </summary>
        private static IEnumerable<MethodInfo> OfferedCalls()
        {
            IEnumerable<MethodInfo> extensions = typeof(DatabentoTimeseriesClientExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false));
            return typeof(IDatabentoTimeseriesClient).GetMethods().Concat(extensions);
        }

        private static MethodInfo OfferedCallFor(IEnumerable<MethodInfo> offered, MethodInfo call)
        {
            return offered.SingleOrDefault(member => member.Name == call.Name && OwnParameters(member).SequenceEqual(Parameters(call)));
        }

        private static async Task<CapturedRequest> CaptureAsync(Type clientType, MethodInfo call, CancellationToken cancellationToken)
        {
            List<CapturedRequest> captured = new List<CapturedRequest>();
            IHttpTransport transport = clientType == typeof(DatabentoClient)
                ? Transport(captured, () => Bytes(DbnBinaryBuilder.BuildCbboStream()))
                : Transport(captured, () => Text(string.Empty));
            using(IDisposable client = (IDisposable)Activator.CreateInstance(clientType, Options(), transport))
            {
                object answer = call.IsStatic
                    ? call.Invoke(null, new object[] { client }.Concat(Arguments(OwnParameters(call), cancellationToken)).ToArray())
                    : call.Invoke(client, Arguments(OwnParameters(call), cancellationToken));
                if(answer is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }

            return captured.Single();
        }

        private static object[] Arguments(Type[] parameters, CancellationToken cancellationToken)
        {
            List<object> arguments = new List<object> { Dataset, parameters[1] == typeof(string) ? Symbol : Symbols, Start, End };
            if(parameters.Length == 5)
            {
                arguments.Add(cancellationToken);
            }

            return arguments.ToArray();
        }

        /// <summary>
        /// A call's parameter types without the client an extension takes first
        /// </summary>
        private static Type[] OwnParameters(MethodInfo call)
        {
            return Parameters(call).Skip(call.IsStatic ? 1 : 0).ToArray();
        }

        private static Type[] Parameters(MethodInfo call)
        {
            return call.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        }

        /// <summary>
        /// The element type of a call's array, awaited or not; null for a call that returns no array
        /// </summary>
        private static Type RecordType(MethodInfo call)
        {
            Type returned = call.ReturnType.IsGenericType && call.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                ? call.ReturnType.GetGenericArguments()[0]
                : call.ReturnType;
            return returned.IsArray ? returned.GetElementType() : null;
        }

        private static string Signature(MethodInfo call)
        {
            return call.Name + "(" + string.Join(", ", OwnParameters(call).Select(type => type.Name)) + ")";
        }

        private static CbboSeed Cbbo(double bidPrice)
        {
            return new CbboSeed
            {
                PublisherId = 1,
                InstrumentId = 42,
                TsEvent = Start,
                TsReceived = Start,
                Price = bidPrice,
                Size = 1,
                BidPrice = bidPrice,
                AskPrice = bidPrice + 0.5,
                BidSize = 2,
                AskSize = 3,
                BidPublisherId = 1,
                AskPublisherId = 2,
            };
        }

        private static DatabentoOptions Options()
        {
            return new DatabentoOptions
            {
                ApiKey = "test-api-key-0000",
                MaxRetries = 0,
            };
        }

        private static HttpResponseMessage Bytes(byte[] body)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        }

        private static HttpResponseMessage Text(string body)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }

        /// <summary>
        /// A transport that records each request as it is sent, since the client disposes it afterwards, answers each with a new response,
        /// and is canceled by the token the client sends with it
        /// </summary>
        private static IHttpTransport Transport(List<CapturedRequest> captured, Func<HttpResponseMessage> respond)
        {
            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns<HttpRequestMessage, CancellationToken>((request, cancellationToken) =>
                {
                    captured.Add(CapturedRequest.Of(request));
                    return cancellationToken.IsCancellationRequested
                        ? Task.FromCanceled<HttpResponseMessage>(cancellationToken)
                        : Task.FromResult(respond());
                });
            transport.Setup(t => t.Dispose());
            return transport.Object;
        }

        /// <summary>
        /// A request as it was sent: its method, URI and body
        /// </summary>
        private sealed class CapturedRequest
        {
            private readonly string text;

            private CapturedRequest(string text)
            {
                this.text = text;
            }

            public static CapturedRequest Of(HttpRequestMessage request)
            {
                string body = request.Content == null ? string.Empty : request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return new CapturedRequest(request.Method + " " + request.RequestUri + " " + body);
            }

            public override string ToString()
            {
                return this.text;
            }
        }
    }
}
