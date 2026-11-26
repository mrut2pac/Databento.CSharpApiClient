// Ignore Spelling: Databento Tcbbo Cbbo Mbo Mbp Bbo Tbbo Cmbp Ohlcv Json

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// <see cref="JsonRecordReader"/> reads JSON records the caller already has, such as a downloaded batch file, as
    /// <see cref="DatabentoJsonClient"/> reads them from a response, whether the data was written with pretty prices and
    /// timestamps (as the client requests them) or without (a batch job's default).
    /// </summary>
    [TestClass]
    public class JsonRecordReaderTests
    {
        private const string AnyApiKey = "test-api-key-0000";

        private const string Symbol = "SPXW  261016C06600000";

        // a TCBBO record as the API sends it with pretty_px and pretty_ts
        private const string PrettyTcbbo = "{\"ts_recv\":\"2026-10-16T14:30:05.000123400Z\","
            + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:05.000000100Z\",\"rtype\":194,\"publisher_id\":30,\"instrument_id\":7654321},"
            + "\"action\":\"T\",\"side\":\"B\",\"flags\":128,\"price\":\"4.050000000\",\"size\":2,\"ts_in_delta\":1500,"
            + "\"levels\":[{\"bid_px\":\"4.000000000\",\"ask_px\":\"4.100000000\",\"bid_sz\":7,\"ask_sz\":25,\"bid_pb\":31,\"ask_pb\":46}],"
            + "\"symbol\":\"" + Symbol + "\"}";

        // the same record without them: 2026-10-16T14:30:05Z is 1792161005 s after the epoch, and a price is in units of 1e-9
        private const string PlainTcbbo = "{\"ts_recv\":\"1792161005000123400\","
            + "\"hd\":{\"ts_event\":\"1792161005000000100\",\"rtype\":194,\"publisher_id\":30,\"instrument_id\":7654321},"
            + "\"action\":\"T\",\"side\":\"B\",\"flags\":128,\"price\":\"4050000000\",\"size\":2,\"ts_in_delta\":1500,"
            + "\"levels\":[{\"bid_px\":\"4000000000\",\"ask_px\":\"4100000000\",\"bid_sz\":7,\"ask_sz\":25,\"bid_pb\":31,\"ask_pb\":46}],"
            + "\"symbol\":\"" + Symbol + "\"}";

        private const string SecondPrettyTcbbo = "{\"ts_recv\":\"2026-10-16T14:30:06.000000000Z\","
            + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:05.900000000Z\",\"rtype\":194,\"publisher_id\":30,\"instrument_id\":7654321},"
            + "\"action\":\"T\",\"side\":\"A\",\"flags\":128,\"price\":\"4.000000000\",\"size\":1,\"ts_in_delta\":900,"
            + "\"levels\":[{\"bid_px\":\"3.950000000\",\"ask_px\":\"4.050000000\",\"bid_sz\":3,\"ask_sz\":20,\"bid_pb\":31,\"ask_pb\":46}],"
            + "\"symbol\":\"" + Symbol + "\"}";

        // a CBBO and a BBO record with pretty_px and pretty_ts, and without; 2026-10-16T14:31:00Z is 1792161060 s after the epoch.
        // Their trade price is optional, unlike TCBBO's.
        private const string PrettyCbbo = "{\"ts_recv\":\"2026-10-16T14:31:00.123456789Z\","
            + "\"hd\":{\"ts_event\":\"2026-10-16T14:31:00.000000000Z\",\"rtype\":193,\"publisher_id\":30,\"instrument_id\":1234567},"
            + "\"side\":\"A\",\"price\":\"4.010000000\",\"size\":3,\"flags\":130,"
            + "\"levels\":[{\"bid_px\":\"3.900000000\",\"ask_px\":\"4.100000000\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":31,\"ask_pb\":33}]}";

        private const string PlainCbbo = "{\"ts_recv\":\"1792161060123456789\","
            + "\"hd\":{\"ts_event\":\"1792161060000000000\",\"rtype\":193,\"publisher_id\":30,\"instrument_id\":1234567},"
            + "\"side\":\"A\",\"price\":\"4010000000\",\"size\":3,\"flags\":130,"
            + "\"levels\":[{\"bid_px\":\"3900000000\",\"ask_px\":\"4100000000\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":31,\"ask_pb\":33}]}";

        private const string PrettyBbo = "{\"ts_recv\":\"2026-10-16T14:31:00.000000000Z\","
            + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:59.987654300Z\",\"rtype\":196,\"publisher_id\":1,\"instrument_id\":42},"
            + "\"side\":\"B\",\"price\":\"21000.250000000\",\"size\":3,\"flags\":128,\"sequence\":77,"
            + "\"levels\":[{\"bid_px\":\"21000.000000000\",\"ask_px\":\"21000.500000000\",\"bid_sz\":10,\"ask_sz\":12,\"bid_ct\":4,\"ask_ct\":5}]}";

        private const string PlainBbo = "{\"ts_recv\":\"1792161060000000000\","
            + "\"hd\":{\"ts_event\":\"1792161059987654300\",\"rtype\":196,\"publisher_id\":1,\"instrument_id\":42},"
            + "\"side\":\"B\",\"price\":\"21000250000000\",\"size\":3,\"flags\":128,\"sequence\":77,"
            + "\"levels\":[{\"bid_px\":\"21000000000000\",\"ask_px\":\"21000500000000\",\"bid_sz\":10,\"ask_sz\":12,\"bid_ct\":4,\"ask_ct\":5}]}";

        [TestMethod]
        public async Task ReadCbboAsync_WithoutPrettyPricesAndTimestamps_ReadsAsThePrettyLine()
        {
            CbboRecordJson plain = (await ReadAllAsync(JsonRecordReader.ReadCbboAsync(ToStream(PlainCbbo)))).Single();
            CbboRecordJson pretty = (await ReadAllAsync(JsonRecordReader.ReadCbboAsync(ToStream(PrettyCbbo)))).Single();

            Assert.AreEqual(JsonSerializer.Serialize(pretty), JsonSerializer.Serialize(plain));
            Assert.AreEqual(4.01, plain.Price);
        }

        [TestMethod]
        public async Task ReadBboAsync_WithoutPrettyPricesAndTimestamps_ReadsAsThePrettyLine()
        {
            BboRecordJson plain = (await ReadAllAsync(JsonRecordReader.ReadBboAsync(ToStream(PlainBbo)))).Single();
            BboRecordJson pretty = (await ReadAllAsync(JsonRecordReader.ReadBboAsync(ToStream(PrettyBbo)))).Single();

            Assert.AreEqual(JsonSerializer.Serialize(pretty), JsonSerializer.Serialize(plain));
            Assert.AreEqual(21000.25, plain.Price);
        }

        [TestMethod]
        public async Task ReadCbboAsync_UndefinedTradePriceWithoutPrettyPrices_IsNull()
        {
            // i64::MAX is the undefined price, which the pretty encoding sends as null
            string undefined = PlainCbbo.Replace("\"price\":\"4010000000\"", "\"price\":\"9223372036854775807\"", StringComparison.Ordinal);

            CbboRecordJson record = (await ReadAllAsync(JsonRecordReader.ReadCbboAsync(ToStream(undefined)))).Single();

            Assert.IsNull(record.Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_ApiLines_YieldsTheRecordsTheJsonClientReads()
        {
            string data = PrettyTcbbo + "\n\n" + SecondPrettyTcbbo + "\n";

            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(data)));
            TcbboRecordJson[] fromClient = await FetchAsync(data, client => client.GetTcbboAsync(Datasets.OpraPillar, Symbol, AnyStart, AnyEnd));

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(Serialize(fromClient), Serialize(read));
            Assert.AreEqual(4.05, read[0].Price);
            Assert.AreEqual(Symbol, read[1].Symbol);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_WithoutPrettyPricesAndTimestamps_ReadsAsThePrettyLine()
        {
            TcbboRecordJson plain = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(PlainTcbbo)))).Single();
            TcbboRecordJson pretty = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(PrettyTcbbo)))).Single();

            Assert.AreEqual(JsonSerializer.Serialize(pretty), JsonSerializer.Serialize(plain));
            Assert.AreEqual(new DateTime(2026, 10, 16, 14, 30, 5, DateTimeKind.Utc).AddTicks(1234), plain.TsReceivedUtc);
            Assert.AreEqual(DateTimeKind.Utc, plain.TsReceivedUtc.Kind);
            Assert.AreEqual(4.1, plain.Level1.AskPrice);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_TimestampsAsJsonNumbers_ReadAsTheStrings()
        {
            string numbers = PlainTcbbo
                .Replace("\"ts_recv\":\"1792161005000123400\"", "\"ts_recv\":1792161005000123400", StringComparison.Ordinal)
                .Replace("\"ts_event\":\"1792161005000000100\"", "\"ts_event\":1792161005000000100", StringComparison.Ordinal);

            TcbboRecordJson fromNumbers = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(numbers)))).Single();
            TcbboRecordJson fromStrings = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(PlainTcbbo)))).Single();

            Assert.AreEqual(JsonSerializer.Serialize(fromStrings), JsonSerializer.Serialize(fromNumbers));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_UndefinedTimestampsWithoutPrettyTimestamps_ReadAsTheDbnDecoderReadsThem()
        {
            // u64::MAX is the undefined timestamp: an optional time is null, as the pretty encoding sends it, and a required
            // one is DateTime.MinValue, as the DBN decoder reads it
            string undefined = PlainTcbbo
                .Replace("\"ts_recv\":\"1792161005000123400\"", "\"ts_recv\":\"18446744073709551615\"", StringComparison.Ordinal)
                .Replace("\"ts_event\":\"1792161005000000100\"", "\"ts_event\":\"18446744073709551615\"", StringComparison.Ordinal);

            TcbboRecordJson record = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(undefined)))).Single();

            Assert.IsNull(record.Header.TsEventUtc);
            Assert.AreEqual(DateTime.MinValue, record.TsReceivedUtc);
        }

        [TestMethod]
        public async Task ReadTradesAsync_TcbboData_ThrowsInvalidData()
        {
            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadTradesAsync(ToStream(PrettyTcbbo))));

            StringAssert.Contains(ex.Message, "0xC2");
        }

        [TestMethod]
        public async Task ReadTcbboAsync_MalformedLine_ThrowsInvalidDataNamingTheLine()
        {
            string data = PrettyTcbbo + "\n{\"ts_recv\":\"2026-10-16T14:30:06Z\",\"hd\":";

            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(data))));

            StringAssert.Contains(ex.Message, "line 2");
            Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_LineWithoutARecordHeader_ThrowsInvalidDataNamingTheLine()
        {
            // valid JSON that is no record, e.g. a batch's condition.json, is refused rather than read as no records
            string data = PrettyTcbbo + "\n{\"date\":\"2026-10-16\",\"condition\":\"available\"}";

            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(data))));

            StringAssert.Contains(ex.Message, "line 2");
        }

        [TestMethod]
        public async Task ReadTcbboAsync_MalformedJsonArray_ThrowsInvalidData()
        {
            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream("[" + PrettyTcbbo + ",{\"hd\":"))));

            Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonArrayOfRecords_YieldsThem()
        {
            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream("[" + PrettyTcbbo + "," + SecondPrettyTcbbo + "]")));

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(4.0, read[1].Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonArray_YieldsRecordsBeforeTheWholeArrayIsRead()
        {
            // an array is read as it arrives, one record at a time, not whole: here the stream fails after its first 200 records
            string head = "[" + string.Join(",", Enumerable.Repeat(PrettyTcbbo, 200)) + ",";
            List<TcbboRecordJson> read = new List<TcbboRecordJson>();

            await Assert.ThrowsExceptionAsync<IOException>(async () =>
            {
                await foreach(TcbboRecordJson record in JsonRecordReader.ReadTcbboAsync(new FakeFailingAfterStream(Encoding.UTF8.GetBytes(head))))
                {
                    read.Add(record);
                }
            });

            Assert.IsTrue(read.Count > 0, "records came out before the stream failed");
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonArrayAfterAByteOrderMark_YieldsTheRecords()
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] array = Encoding.UTF8.GetBytes("[" + PrettyTcbbo + "," + SecondPrettyTcbbo + "]");

            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(new MemoryStream(bom.Concat(array).ToArray())));

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(4.0, read[1].Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonLinesAfterAByteOrderMark_YieldsTheRecords()
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] lines = Encoding.UTF8.GetBytes(PrettyTcbbo + "\n" + SecondPrettyTcbbo + "\n");

            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(new FakeOneByteStream(bom.Concat(lines).ToArray())));

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(4.0, read[1].Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_StreamThatCannotBeRead_ThrowsArgumentException()
        {
            MemoryStream closed = ToStream(PrettyTcbbo);
            closed.Dispose();

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => ReadAllAsync(JsonRecordReader.ReadTcbboAsync(closed)));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonArrayFollowedByText_ThrowsInvalidData()
        {
            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream("[" + PrettyTcbbo + "] x"))));

            Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_EmptyJsonArray_YieldsNothing()
        {
            Assert.AreEqual(0, (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream("[]")))).Length);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_JsonArrayOneByteAtATime_YieldsTheRecords()
        {
            byte[] array = Encoding.UTF8.GetBytes("[" + PrettyTcbbo + ",\n " + SecondPrettyTcbbo + "]\n");

            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(new FakeOneByteStream(array)));

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(4.0, read[1].Price);
        }

        [TestMethod]
        public async Task ReadCbboAsync_TradePriceThatIsNoPrice_ThrowsInvalidDataFromAJsonError()
        {
            string data = PrettyCbbo.Replace("\"price\":\"4.010000000\"", "\"price\":\"not_a_price\"", StringComparison.Ordinal);

            InvalidDataException ex = await Assert.ThrowsExceptionAsync<InvalidDataException>(
                () => ReadAllAsync(JsonRecordReader.ReadCbboAsync(ToStream(data))));

            Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        }

        [TestMethod]
        public async Task ReadCbboAsync_TradePriceAsAJsonNumber_ReadsAsADisplayPrice()
        {
            string data = PrettyCbbo.Replace("\"price\":\"4.010000000\"", "\"price\":4.01", StringComparison.Ordinal);

            CbboRecordJson record = (await ReadAllAsync(JsonRecordReader.ReadCbboAsync(ToStream(data)))).Single();

            Assert.AreEqual(4.01, record.Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_ZeroTimestampWithoutPrettyTimestamps_IsUndefined()
        {
            // 0, like u64::MAX, is an undefined timestamp, as the DBN decoder reads it
            string zero = PlainTcbbo.Replace("\"ts_event\":\"1792161005000000100\"", "\"ts_event\":\"0\"", StringComparison.Ordinal);

            TcbboRecordJson record = (await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(zero)))).Single();

            Assert.IsNull(record.Header.TsEventUtc);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_Canceled_StopsReading()
        {
            using CancellationTokenSource cancel = new CancellationTokenSource();
            cancel.Cancel();

            // the stream reader may throw OperationCanceledException's TaskCanceledException
            Exception thrown = null;
            try
            {
                await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(ToStream(PrettyTcbbo), cancel.Token));
            }
            catch(Exception ex)
            {
                thrown = ex;
            }

            Assert.IsInstanceOfType(thrown, typeof(OperationCanceledException));
        }

        [TestMethod]
        public async Task ReadTcbboAsync_LeavesTheCallersStreamOpen()
        {
            using MemoryStream stream = ToStream(PrettyTcbbo);

            await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(stream));

            Assert.IsTrue(stream.CanRead);
        }

        [TestMethod]
        public void ReadTcbbo_NullStream_ThrowsWhenCalled()
        {
            Assert.ThrowsException<ArgumentNullException>(() => JsonRecordReader.ReadTcbboAsync(null));
            Assert.ThrowsException<ArgumentNullException>(() => JsonRecordReader.ReadTcbbo(null));
        }

        [TestMethod]
        public void ReadTcbbo_Sync_YieldsTheRecords()
        {
            TcbboRecordJson[] read = JsonRecordReader.ReadTcbbo(ToStream(PrettyTcbbo + "\n" + SecondPrettyTcbbo)).ToArray();

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(4.0, read[1].Price);
        }

        [TestMethod]
        public async Task ReadTcbboAsync_DbnRecordsSavedAsJsonGz_ReadBackAsTheMappedRecords()
        {
            // the cached client's round trip: DBN records saved as Databento's JSON, gzip-compressed, read back later
            TcbboRecordJson[] saved =
            {
                Tcbbo(4.05, 2).ToJsonRecord(),
                Tcbbo(4.0, 1).ToJsonRecord(),
            };

            using MemoryStream file = new MemoryStream();
            using(GZipStream gzip = new GZipStream(file, CompressionLevel.Optimal, leaveOpen: true))
            using(StreamWriter writer = new StreamWriter(gzip))
            {
                foreach(TcbboRecordJson record in saved)
                {
                    await writer.WriteLineAsync(JsonSerializer.Serialize(record));
                }
            }

            file.Position = 0;
            using GZipStream unzip = new GZipStream(file, CompressionMode.Decompress);
            TcbboRecordJson[] read = await ReadAllAsync(JsonRecordReader.ReadTcbboAsync(unzip));

            Assert.AreEqual(Serialize(saved), Serialize(read));
        }

        [TestMethod]
        [DataRow("ReadCbbo", 0xC0)]
        [DataRow("ReadCbbo", 0xC1)]
        [DataRow("ReadTrades", 0x00)]
        [DataRow("ReadMbp1", 0x01)]
        [DataRow("ReadMbo", 0xA0)]
        [DataRow("ReadMbp10", 0x0A)]
        [DataRow("ReadTbbo", 0x01)]
        [DataRow("ReadBbo", 0xC3)]
        [DataRow("ReadBbo", 0xC4)]
        [DataRow("ReadTcbbo", 0xC2)]
        [DataRow("ReadCmbp1", 0xB1)]
        [DataRow("ReadOhlcv", 0x20)]
        [DataRow("ReadOhlcv", 0x24)]
        [DataRow("ReadStatistics", 0x18)]
        [DataRow("ReadDefinitions", 0x13)]
        [DataRow("ReadStatus", 0x12)]
        [DataRow("ReadImbalance", 0x14)]
        public void EveryReader_ReadsItsSchemasRecordTypeAndRefusesAnother(string method, int rtype)
        {
            MethodInfo read = typeof(JsonRecordReader).GetMethod(method, new[] { typeof(Stream) });

            IEnumerable<object> records = ((System.Collections.IEnumerable)read.Invoke(null, new object[] { ToStream(MinimalLine(rtype)) })).Cast<object>();
            object record = records.Single();
            RecordHeaderJson header = (RecordHeaderJson)record.GetType().GetProperty("Header").GetValue(record);
            Assert.AreEqual((RType)rtype, header.RecordType);
            Assert.AreEqual(42u, header.InstrumentId);

            // the system record type belongs to no schema
            IEnumerable<object> other = ((System.Collections.IEnumerable)read.Invoke(null, new object[] { ToStream(MinimalLine(0x17)) })).Cast<object>();
            Assert.ThrowsException<InvalidDataException>(() => other.ToArray());
        }

        [TestMethod]
        public void JsonRecordReader_ReadsEverySchemaDbnReaderReads()
        {
            string[] dbn = typeof(DbnReader).GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => m.Name).OrderBy(n => n).ToArray();
            string[] json = typeof(JsonRecordReader).GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => m.Name).OrderBy(n => n).ToArray();

            CollectionAssert.AreEqual(dbn, json);
        }

        private static readonly DateTimeOffset AnyStart = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset AnyEnd = new DateTimeOffset(2026, 10, 17, 0, 0, 0, TimeSpan.Zero);

        private static string MinimalLine(int rtype)
            => "{\"hd\":{\"ts_event\":\"2026-10-16T14:30:00.000000000Z\",\"rtype\":" + rtype + ",\"publisher_id\":1,\"instrument_id\":42}}";

        private static TcbboRecordDbn Tcbbo(double price, uint size)
        {
            return new TcbboRecordDbn
            {
                RecordType = RType.Tcbbo,
                PublisherId = 30,
                InstrumentId = 7654321,
                TsEventUtc = new DateTime(2026, 10, 16, 14, 30, 5, DateTimeKind.Utc),
                TsReceivedUtc = new DateTime(2026, 10, 16, 14, 30, 5, DateTimeKind.Utc).AddTicks(1234),
                Price = price,
                Size = size,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                Level = new CbboLevelDbn
                {
                    BidPrice = price - 0.05,
                    AskPrice = price + 0.05,
                    BidSize = 7,
                    AskSize = 25,
                    BidPublisherId = 31,
                    AskPublisherId = 46,
                },
                Symbol = Symbol,
            };
        }

        private static MemoryStream ToStream(string data) => new MemoryStream(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// A stream that gives its bytes, at most <c>chunk</c> a read, then fails as a broken connection would or ends
        /// </summary>
        private class FakeChunkedStream : Stream
        {
            private readonly byte[] bytes;

            private readonly int chunk;

            private readonly bool failAtEnd;

            private int position;

            public FakeChunkedStream(byte[] bytes, int chunk, bool failAtEnd)
            {
                this.bytes = bytes;
                this.chunk = chunk;
                this.failAtEnd = failAtEnd;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(Span<byte> buffer)
            {
                int count = Math.Min(Math.Min(this.chunk, buffer.Length), this.bytes.Length - this.position);
                if(count == 0 && buffer.Length > 0 && this.failAtEnd)
                {
                    throw new IOException("The connection broke.");
                }

                this.bytes.AsSpan(this.position, count).CopyTo(buffer);
                this.position += count;
                return count;
            }

            public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => ValueTask.FromResult(this.Read(buffer.Span));

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => Task.FromResult(this.Read(buffer.AsSpan(offset, count)));

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        /// <summary>
        /// A stream that gives its bytes, then fails as a broken connection would
        /// </summary>
        private sealed class FakeFailingAfterStream : FakeChunkedStream
        {
            public FakeFailingAfterStream(byte[] bytes)
                : base(bytes, int.MaxValue, failAtEnd: true)
            {
            }
        }

        /// <summary>
        /// A stream that gives one byte a read, so every token is cut across reads
        /// </summary>
        private sealed class FakeOneByteStream : FakeChunkedStream
        {
            public FakeOneByteStream(byte[] bytes)
                : base(bytes, 1, failAtEnd: false)
            {
            }
        }

        private static string Serialize<T>(T[] records) => string.Join("\n", records.Select(record => JsonSerializer.Serialize(record)));

        private static async Task<T[]> ReadAllAsync<T>(IAsyncEnumerable<T> records)
        {
            List<T> list = new List<T>();
            await foreach(T record in records)
            {
                list.Add(record);
            }

            return list.ToArray();
        }

        private static async Task<T[]> FetchAsync<T>(string body, Func<DatabentoJsonClient, Task<T[]>> fetch)
        {
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });

            using DatabentoJsonClient client = new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
            return await fetch(client);
        }
    }
}
