using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// The DBN client's streaming methods yield records as they are decoded from the response body, retry only before the
    /// body is read, and dispose the response however the enumeration ends.
    /// </summary>
    [TestClass]
    public class DbnStreamingTests
    {
        private const string AnyDataset = "OPRA.PILLAR";
        private const string AnySymbol = "SPXW  261016C06600000";

        // DBN prelude (8 bytes) + the builder's 120-byte metadata body, then 80-byte TCBBO records
        private const int MetadataBytes = 128;
        private const int TcbboBytes = 80;

        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset End = Start.AddHours(1);
        private static readonly DateOnly Day = DateOnly.FromDateTime(Start.UtcDateTime);

        [TestMethod]
        public async Task StreamTcbboAsync_YieldsEveryRecordInOrderWithItsSymbol()
        {
            byte[] bytes = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(801, 4.10), Tcbbo(800, 4.15)),
                Mapping("SPXW  261016C06600000", "800"),
                Mapping("SPXW  261016P06600000", "801"));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes)));

            List<TcbboRecordDbn> records = await ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End));

            CollectionAssert.AreEqual(new[] { 4.05, 4.10, 4.15 }, records.Select(record => Math.Round(record.Price, 2)).ToArray());
            CollectionAssert.AreEqual(
                new[] { "SPXW  261016C06600000", "SPXW  261016P06600000", "SPXW  261016C06600000" },
                records.Select(record => record.Symbol).ToArray());
        }

        [TestMethod]
        public void StreamTcbbo_Sync_YieldsTheSameRecords()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(800, 4.10));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes)));

            List<TcbboRecordDbn> records = BuildClient(transport).StreamTcbbo(AnyDataset, new[] { AnySymbol }, Start, End).ToList();

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_ConnectionDropsAfterFirstRecord_YieldsItThenFailsWithoutRetrying()
        {
            // a retry after a yielded record would hand the caller that record twice
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(800, 4.10));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes, failAfterBytes: MetadataBytes + TcbboBytes)));
            List<TcbboRecordDbn> received = new List<TcbboRecordDbn>();

            await Assert.ThrowsExceptionAsync<IOException>(async () =>
            {
                await foreach(TcbboRecordDbn record in BuildClient(transport, maxRetries: 3).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End))
                {
                    received.Add(record);
                }
            });

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual(1, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_TransientErrorBeforeTheBody_IsRetried()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05));
            FakeTransport transport = new FakeTransport(call => call == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : Ok(new FakeContentStream(bytes)));

            List<TcbboRecordDbn> records = await ToListAsync(BuildClient(transport, maxRetries: 3).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual(2, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_CallerStopsEarly_DisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(800, 4.10)));
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            await foreach(TcbboRecordDbn record in BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End))
            {
                break;
            }

            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_Completes_DisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05)));
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            await ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End));

            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_CanceledAfterARecord_ThrowsAndDisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(800, 4.10)));
            FakeTransport transport = new FakeTransport(_ => Ok(body));
            using CancellationTokenSource cancellation = new CancellationTokenSource();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await foreach(TcbboRecordDbn record in BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End, cancellation.Token))
                {
                    cancellation.Cancel();
                }
            });

            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_HttpError_Throws()
        {
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"detail\":\"bad request\"}"),
            });

            await Assert.ThrowsExceptionAsync<DatabentoHttpException>(() => ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End)));
        }

        [TestMethod]
        public async Task StreamTcbboAsync_RecordCutShort_ThrowsInvalidData()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes.AsSpan(0, MetadataBytes + (TcbboBytes / 2)).ToArray())));

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End)));
        }

        [TestMethod]
        public async Task StreamTcbboAsync_NoRecords_YieldsNothing()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream())));

            List<TcbboRecordDbn> records = await ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(0, records.Count);
        }

        [TestMethod]
        public void StreamTcbboAsync_NoSymbols_ThrowsWhenCalled()
        {
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream())));

            Assert.ThrowsException<ArgumentException>(() => BuildClient(transport).StreamTcbboAsync(AnyDataset, Array.Empty<string>(), Start, End));
            Assert.AreEqual(0, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_HttpError_DisposesTheResponse()
        {
            FakeContentStream body = new FakeContentStream(Encoding.UTF8.GetBytes("{\"detail\":\"bad request\"}"));
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StreamContent(body),
            });

            await Assert.ThrowsExceptionAsync<DatabentoHttpException>(() => ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End)));
            Assert.IsTrue(body.IsDisposed);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_DataEndAfterAvailableEnd_Throws()
        {
            // a refusal, not an empty range: the JSON client throws the same exception for it
            string noData = "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"end is after available range\"}}";
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(noData),
            });

            DatabentoHttpException ex = await Assert.ThrowsExceptionAsync<DatabentoHttpException>(
                () => ToListAsync(BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End)));

            Assert.AreEqual(422, ex.StatusCode);
            Assert.AreEqual("data_end_after_available_end", ex.ErrorCase);
            Assert.AreEqual(DatabentoErrorCase.DataEndAfterAvailableEnd, ex.Code);
            StringAssert.Contains(ex.Message, "end is after available range");
        }

        [TestMethod]
        public async Task StreamTcbboAsync_ConnectionDropsInTheMetadata_IsNotRetried()
        {
            // only the HTTP status is retried; a failure while reading the body is thrown, even before the first record
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes, failAfterBytes: MetadataBytes / 2)));

            await Assert.ThrowsExceptionAsync<IOException>(() => ToListAsync(BuildClient(transport, maxRetries: 3).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End)));
            Assert.AreEqual(1, transport.CallCount);
        }

        [TestMethod]
        public async Task StreamTcbboAsync_EnumeratedTwice_SendsTwoRequests()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05));
            FakeTransport transport = new FakeTransport(_ => Ok(new FakeContentStream(bytes)));
            IAsyncEnumerable<TcbboRecordDbn> records = BuildClient(transport).StreamTcbboAsync(AnyDataset, AnySymbol, Start, End);

            await ToListAsync(records);
            await ToListAsync(records);

            Assert.AreEqual(2, transport.CallCount);
        }

        [TestMethod]
        public async Task GetTcbboAsync_CollectsTheStream()
        {
            byte[] bytes = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, 4.05), Tcbbo(800, 4.10)),
                Mapping(AnySymbol, "800"));
            FakeContentStream body = new FakeContentStream(bytes);
            FakeTransport transport = new FakeTransport(_ => Ok(body));

            TcbboRecordDbn[] records = await BuildClient(transport).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length);
            Assert.AreEqual(AnySymbol, records[1].Symbol);
            Assert.IsTrue(body.IsDisposed);
        }

        private static TcbboSeed Tcbbo(uint instrumentId, double price)
        {
            return new TcbboSeed
            {
                InstrumentId = instrumentId,
                TsEvent = Start,
                TsReceived = Start,
                Price = price,
                Size = 1,
            };
        }

        private static SymbolMappingSeed Mapping(string rawSymbol, string instrumentId)
        {
            return new SymbolMappingSeed
            {
                RawSymbol = rawSymbol,
                Intervals = new[]
                {
                    new SymbolMappingIntervalSeed
                    {
                        StartDate = Day,
                        EndDate = Day.AddDays(1),
                        Symbol = instrumentId,
                    },
                },
            };
        }

        private static HttpResponseMessage Ok(Stream body)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(body),
            };
        }

        private static DatabentoClient BuildClient(FakeTransport transport, int maxRetries = 0)
        {
            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = maxRetries,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(10),
            };
            return new DatabentoClient(options, transport.Object);
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
