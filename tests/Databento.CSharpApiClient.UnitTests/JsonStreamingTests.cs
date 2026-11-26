using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.Exceptions;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// The JSON client's streaming methods yield records line by line as the response arrives, retry only before the body
    /// is read, throw an error answer as <see cref="DatabentoHttpException"/> and dispose the response however the enumeration ends.
    /// </summary>
    [TestClass]
    public class JsonStreamingTests
    {
        private const string AnyDataset = "XNAS.ITCH";
        private const string AnySymbol = "SPY";

        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset End = Start.AddHours(1);

        [TestMethod]
        public async Task StreamTradesAsync_YieldsEveryRecordInOrderWithTheRequestedSymbol()
        {
            string body = Trade(1, "4.00") + "\n" + Trade(2, "4.10") + "\n";
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            List<TradeRecordJson> records = await ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End));

            CollectionAssert.AreEqual(new uint[] { 1, 2 }, records.Select(record => record.Sequence).ToArray());
            Assert.IsTrue(records.All(record => record.Symbol == AnySymbol));
        }

        [TestMethod]
        public void StreamTrades_Sync_YieldsTheSameRecords()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(Trade(1, "4.00") + "\n" + Trade(2, "4.10")));

            List<TradeRecordJson> records = BuildClient(transport).StreamTrades(AnyDataset, new[] { AnySymbol }, Start, End).ToList();

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        public async Task StreamTradesAsync_ArrayResponse_YieldsItsRecords()
        {
            FakeTransport transport = new FakeTransport(_ => Ok("[" + Trade(1, "4.00") + ",\n" + Trade(2, "4.10") + "]"));

            List<TradeRecordJson> records = await ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        public async Task StreamTradesAsync_ConnectionDropsAfterFirstLine_YieldsItThenFailsWithoutRetrying()
        {
            string first = Trade(1, "4.00") + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(first + Trade(2, "4.10") + "\n");
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes, failAfterBytes: Encoding.UTF8.GetByteCount(first))));
            List<TradeRecordJson> received = new List<TradeRecordJson>();

            await Assert.ThrowsExceptionAsync<IOException>(async () =>
            {
                await foreach(TradeRecordJson record in BuildClient(transport, maxRetries: 3).StreamTradesAsync(AnyDataset, AnySymbol, Start, End))
                {
                    received.Add(record);
                }
            });

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual(1, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTradesAsync_TransientErrorBeforeTheBody_IsRetried()
        {
            FakeTransport transport = new FakeTransport(call => call == 1
                ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                : Ok(Trade(1, "4.00")));

            List<TradeRecordJson> records = await ToListAsync(BuildClient(transport, maxRetries: 3).StreamTradesAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual(2, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTradesAsync_EmptyBody_YieldsNothing()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(string.Empty));

            List<TradeRecordJson> records = await ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(0, records.Count);
        }

        [TestMethod]
        public async Task StreamTradesAsync_DataEndAfterAvailableEnd_ThrowsWithTheStatusAndCase()
        {
            // a refusal, not an empty range: the DBN client throws the same exception for it
            string refusal = "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"end is after available range\"}}";
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(refusal, Encoding.UTF8, "application/json"),
            });

            DatabentoHttpException ex = await Assert.ThrowsExceptionAsync<DatabentoHttpException>(
                () => ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End)));

            Assert.AreEqual(422, ex.StatusCode);
            Assert.AreEqual("data_end_after_available_end", ex.ErrorCase);
            Assert.AreEqual(DatabentoErrorCase.DataEndAfterAvailableEnd, ex.Code);
            StringAssert.Contains(ex.Message, "end is after available range");
        }

        [TestMethod]
        public async Task StreamTradesAsync_HttpError_ThrowsAndDisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(Encoding.UTF8.GetBytes("{\"detail\":\"bad request\"}"));
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StreamContent(body),
            });

            await Assert.ThrowsExceptionAsync<DatabentoHttpException>(() => ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End)));
            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTradesAsync_DataEndAfterAvailableEnd_DisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(Encoding.UTF8.GetBytes(
                "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"end is after available range\"}}"));
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StreamContent(body),
            });

            await Assert.ThrowsExceptionAsync<DatabentoHttpException>(() => ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End)));

            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public void StreamTradesAsync_NoSymbols_ThrowsWhenCalled()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(Trade(1, "4.00")));

            Assert.ThrowsException<ArgumentException>(() => BuildClient(transport).StreamTradesAsync(AnyDataset, Array.Empty<string>(), Start, End));
            Assert.AreEqual(0, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTradesAsync_CallerStopsEarly_DisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(Encoding.UTF8.GetBytes(Trade(1, "4.00") + "\n" + Trade(2, "4.10")));
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            await foreach(TradeRecordJson record in BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End))
            {
                break;
            }

            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTradesAsync_UnreadableLine_ThrowsDatabentoException()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(Trade(1, "4.00") + "\n{not json}\n"));

            await Assert.ThrowsExceptionAsync<DatabentoException>(() => ToListAsync(BuildClient(transport).StreamTradesAsync(AnyDataset, AnySymbol, Start, End)));
        }

        [TestMethod]
        public async Task GetTradesAsync_CollectsTheStream()
        {
            FakeContentStream body = new FakeContentStream(Encoding.UTF8.GetBytes(Trade(1, "4.00") + "\n" + Trade(2, "4.10")));
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            TradeRecordJson[] records = await BuildClient(transport).GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length);
            Assert.AreEqual(AnySymbol, records[1].Symbol);
            Assert.IsTrue(body.IsDisposed);
        }

        private static string Trade(uint sequence, string price)
        {
            return "{\"hd\":{\"rtype\":0,\"publisher_id\":1,\"instrument_id\":42,\"ts_event\":\"2022-05-16T13:30:00.000000000Z\"},"
                + "\"price\":\"" + price + "\",\"size\":1,\"action\":\"T\",\"side\":\"N\",\"flags\":0,\"depth\":0,"
                + "\"ts_recv\":\"2022-05-16T13:30:00.000000000Z\",\"ts_in_delta\":0,\"sequence\":" + sequence + "}";
        }

        private static HttpResponseMessage Ok(string body)
        {
            return Ok(new FakeContentStream(Encoding.UTF8.GetBytes(body)));
        }

        private static HttpResponseMessage Ok(Stream body)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(body),
            };
        }

        private static DatabentoJsonClient BuildClient(FakeTransport transport, int maxRetries = 0)
        {
            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = maxRetries,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(10),
            };
            return new DatabentoJsonClient(options, transport.Object);
        }

        private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> records)
        {
            List<T> list = new List<T>();
            await foreach(T record in records)
            {
                list.Add(record);
            }

            return list;
        }
    }
}
