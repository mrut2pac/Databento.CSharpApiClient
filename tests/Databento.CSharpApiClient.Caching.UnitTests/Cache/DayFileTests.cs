// Ignore Spelling: Bbo Cbbo Tcbbo Ohlcv

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.Caching.Cache;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Cache
{
    /// <summary>
    /// A day file must give back every value the archive stores of the records written into it, for every schema the archive holds
    /// </summary>
    [TestClass]
    public class DayFileTests
    {
        private const string Option = "SPXW  261016C06600000";

        private static readonly DateTime TsEvent = new DateTime(2026, 10, 16, 14, 30, 0, DateTimeKind.Utc);

        private string directory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "DayFileTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.directory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(Directory.Exists(this.directory))
            {
                Directory.Delete(this.directory, recursive: true);
            }
        }

        [TestMethod]
        public async Task WriteThenRead_CbboRecords_KeepEveryArchivedValue()
        {
            CbboRecordJson[] records =
            {
                new CbboRecordJson
                {
                    Header = Header(RType.Cbbo1M, 0),
                    TsReceivedUtc = TsEvent.AddTicks(1234567),
                    Side = TradeAggressor.Seller,
                    Price = 4.05,
                    Size = 7,
                    Flags = MessageInfoBits.Last,
                    Levels = new[] { new CbboLevel1RecordJson { BidPrice = 4.0, AskPrice = 4.1, BidSize = 120, AskSize = 95, BidPublisherId = 31, AskPublisherId = 32 } },
                    Symbol = Option,
                },
                new CbboRecordJson
                {
                    // a minute without a trade, quoted on one side only
                    Header = Header(RType.Cbbo1M, 1),
                    TsReceivedUtc = TsEvent.AddMinutes(1).AddTicks(7),
                    Side = TradeAggressor.None,
                    Price = null,
                    Levels = new[] { new CbboLevel1RecordJson { BidPrice = double.NaN, AskPrice = 0.05, AskSize = 3, AskPublisherId = 40 } },
                    Symbol = Option,
                },
            };

            await this.AssertRoundTripAsync(new CbboArchiveSchema(RType.Cbbo1M), JsonRecordReader.ReadCbboAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_BboRecords_KeepEveryArchivedValue()
        {
            BboRecordJson[] records =
            {
                new BboRecordJson
                {
                    Header = Header(RType.Bbo1M, 0),
                    Price = 21000.25,
                    Size = 3,
                    Side = TradeAggressor.Buyer,
                    Flags = MessageInfoBits.Last,
                    TsReceivedUtc = TsEvent.AddTicks(1234567),
                    Sequence = 77,
                    Levels = new[] { new Mbp1LevelJson { BidPrice = 21000.0, AskPrice = 21000.5, BidSize = 10, AskSize = 12, BidCount = 4, AskCount = 5 } },
                    Symbol = "MNQZ6",
                },
            };

            await this.AssertRoundTripAsync(new BboArchiveSchema(RType.Bbo1M), JsonRecordReader.ReadBboAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_TcbboRecords_KeepEveryArchivedValue()
        {
            TcbboRecordJson[] records =
            {
                new TcbboRecordJson
                {
                    Header = Header(RType.Tcbbo, 0),
                    Price = 4.05,
                    Size = 2,
                    Action = OrderBookAction.Trade,
                    Side = TradeAggressor.Seller,
                    Flags = MessageInfoBits.Last,
                    TsReceivedUtc = TsEvent.AddTicks(1234567),
                    TsInDelta = 5150,
                    Levels = new[] { new CbboLevel1RecordJson { BidPrice = 4.0, AskPrice = double.NaN, BidSize = 20, AskSize = 30, BidPublisherId = 21, AskPublisherId = 22 } },
                    Symbol = Option,
                },
            };

            await this.AssertRoundTripAsync(new TcbboArchiveSchema(RType.Tcbbo), JsonRecordReader.ReadTcbboAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_TradeRecords_KeepEveryArchivedValue()
        {
            TradeRecordJson[] records =
            {
                new TradeRecordJson
                {
                    Header = Header(RType.Mbp0, 0),
                    Price = 6600.25,
                    Size = 1,
                    Action = OrderBookAction.Trade,
                    Side = TradeAggressor.None,
                    Flags = MessageInfoBits.Last,
                    Depth = 3,
                    TsReceivedUtc = TsEvent.AddTicks(1234567),
                    TsInDelta = 950,
                    Sequence = 123456,
                    Symbol = "SPX",
                },
            };

            await this.AssertRoundTripAsync(new TradeArchiveSchema(RType.Mbp0), JsonRecordReader.ReadTradesAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_OhlcvRecords_KeepEveryArchivedValue()
        {
            OhlcvRecordJson[] records =
            {
                new OhlcvRecordJson
                {
                    Header = Header(RType.Ohlcv1D, 0),
                    Open = 600.1,
                    High = 601.2,
                    Low = 599.3,
                    Close = 600.4,
                    Volume = 125000,
                    Symbol = "QQQ",
                },
            };

            await this.AssertRoundTripAsync(new OhlcvArchiveSchema(RType.Ohlcv1D), JsonRecordReader.ReadOhlcvAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_StatisticsRecords_KeepEveryArchivedValue()
        {
            StatisticsRecordJson[] records =
            {
                new StatisticsRecordJson
                {
                    Header = Header(RType.Statistics, 0),
                    TsReceivedUtc = TsEvent.AddTicks(1234567),
                    TsRefUtc = TsEvent.AddDays(-1),
                    Price = double.NaN,
                    Quantity = 4200,
                    Sequence = 77,
                    TsInDelta = 5150,
                    StatType = 9,
                    ChannelId = 3,
                    UpdateAction = 1,
                    StatFlags = MessageInfoBits.Last,
                    Symbol = Option,
                },
                new StatisticsRecordJson
                {
                    // a price statistic, with no quantity and no reference time
                    Header = Header(RType.Statistics, 1),
                    TsReceivedUtc = TsEvent.AddMinutes(1),
                    TsRefUtc = null,
                    Price = 4.25,
                    Quantity = long.MaxValue,
                    StatType = 3,
                    Symbol = Option,
                },
            };

            await this.AssertRoundTripAsync(new StatisticsArchiveSchema(RType.Statistics), JsonRecordReader.ReadStatisticsAsync, records);
        }

        [TestMethod]
        public async Task WriteThenRead_EmptyDay_IsAnEmptyJsonArray()
        {
            // a day cached as empty is final, and stays readable by hand
            string path = Path.Combine(this.directory, "QQQ_2026-10-16.UTC.json.gz");

            Assert.IsTrue(await DayFile.WriteAsync(path, Array.Empty<OhlcvRecordJson>(), overwrite: true, CancellationToken.None));

            Assert.AreEqual(0, (await DayFile.ReadAsync(path, JsonRecordReader.ReadOhlcvAsync, CancellationToken.None)).Length);
            Assert.AreEqual("[]", ReadText(path));
        }

        [TestMethod]
        public async Task Write_FolderMissing_IsCreatedAndNoTemporaryFileIsLeft()
        {
            string path = Path.Combine(this.directory, "OPRA.PILLAR", "cbbo-1m", "2026", "10", "day.json.gz");

            Assert.IsTrue(await DayFile.WriteAsync(path, new[] { Cbbo(4.05) }, overwrite: true, CancellationToken.None));

            CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(Path.GetDirectoryName(path)));
        }

        [TestMethod]
        public async Task Write_FileThereWithoutOverwrite_KeepsTheFileThere()
        {
            string path = Path.Combine(this.directory, "day.json.gz");
            await DayFile.WriteAsync(path, new[] { Cbbo(4.05) }, overwrite: true, CancellationToken.None);

            bool written = await DayFile.WriteAsync(path, new[] { Cbbo(5.05), Cbbo(5.10) }, overwrite: false, CancellationToken.None);

            Assert.IsFalse(written);
            CbboRecordJson[] read = await DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None);
            Assert.AreEqual(1, read.Length);
            Assert.AreEqual(4.05, read[0].Price);
            CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(this.directory), "the dropped write leaves no temporary file");
        }

        [TestMethod]
        public async Task Write_FileThereWithOverwrite_ReplacesIt()
        {
            string path = Path.Combine(this.directory, "day.json.gz");
            await DayFile.WriteAsync(path, new[] { Cbbo(4.05) }, overwrite: true, CancellationToken.None);

            bool written = await DayFile.WriteAsync(path, new[] { Cbbo(5.05), Cbbo(5.10) }, overwrite: true, CancellationToken.None);

            Assert.IsTrue(written);
            CbboRecordJson[] read = await DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None);
            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(5.05, read[0].Price);
            Assert.AreEqual(5.10, read[1].Price);
        }

        [TestMethod]
        public async Task Read_FileThatIsNotGzip_Throws()
        {
            string path = Path.Combine(this.directory, "day.json.gz");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None));
        }

        [TestMethod]
        public async Task Read_GzipOfSomethingElseThanJsonRecords_Throws()
        {
            string path = Path.Combine(this.directory, "day.json.gz");
            WriteGzip(path, "not json");

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None));
        }

        [TestMethod]
        [DataRow("an empty file")]
        [DataRow("an empty gzip payload")]
        [DataRow("whitespace only")]
        [DataRow("text before an array")]
        [DataRow("JSON lines")]
        public async Task Read_FileHoldingNoJsonArray_Throws(string holding)
        {
            // an empty array is a day cached as empty and final, so a file holding no array must not read as one, nor as records
            string path = Path.Combine(this.directory, "day.json.gz");
            File.WriteAllBytes(path, CacheFixture.NoArrayDayFile(holding));

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None));
        }

        [TestMethod]
        public async Task Read_ArrayAfterWhitespace_IsRead()
        {
            string path = Path.Combine(this.directory, "day.json.gz");
            WriteGzip(path, " \n[]");

            Assert.AreEqual(0, (await DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None)).Length);
        }

        [TestMethod]
        public async Task Read_RecordsOfAnotherSchema_Throws()
        {
            // a TCBBO day file read as CBBO: the record type tells them apart
            string path = Path.Combine(this.directory, "day.json.gz");
            await DayFile.WriteAsync(path, new[] { new TcbboRecordJson { Header = Header(RType.Tcbbo, 0), Price = 4.05, Side = TradeAggressor.Buyer } }, overwrite: true, CancellationToken.None);

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => DayFile.ReadAsync(path, JsonRecordReader.ReadCbboAsync, CancellationToken.None));
        }

        [TestMethod]
        public async Task Read_MissingFile_Throws()
        {
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => DayFile.ReadAsync(Path.Combine(this.directory, "missing.json.gz"), JsonRecordReader.ReadCbboAsync, CancellationToken.None));
        }

        private static RecordHeaderJson Header(RType recordType, int minute)
        {
            return new RecordHeaderJson
            {
                RecordType = recordType,
                PublisherId = 30,
                InstrumentId = 1234567,
                TsEventUtc = TsEvent.AddMinutes(minute),
            };
        }

        private static CbboRecordJson Cbbo(double price)
        {
            return new CbboRecordJson
            {
                Header = Header(RType.Cbbo1M, 0),
                Price = price,
                Side = TradeAggressor.None,
                TsReceivedUtc = TsEvent,
                Levels = new[] { new CbboLevel1RecordJson { BidPrice = price, AskPrice = price + 0.05 } },
            };
        }

        private static string ReadText(string path)
        {
            using(FileStream file = File.OpenRead(path))
            using(GZipStream json = new GZipStream(file, CompressionMode.Decompress))
            using(StreamReader reader = new StreamReader(json, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static void WriteGzip(string path, string text)
        {
            using(FileStream file = File.Create(path))
            using(GZipStream json = new GZipStream(file, CompressionLevel.Optimal))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                json.Write(bytes, 0, bytes.Length);
            }
        }

        private async Task AssertRoundTripAsync<TSource, TRecord, TJson>(ArchiveRecordSchema<TSource, TRecord> schema, Func<Stream, CancellationToken, IAsyncEnumerable<TJson>> readRecords, TJson[] records)
            where TSource : class, IRecord
            where TRecord : class, TSource
            where TJson : class, TSource
        {
            string path = Path.Combine(this.directory, "day.json.gz");

            Assert.IsTrue(await DayFile.WriteAsync(path, records, overwrite: true, CancellationToken.None));
            TJson[] read = await DayFile.ReadAsync(path, readRecords, CancellationToken.None);

            Assert.AreEqual(records.Length, read.Length);
            for(int i = 0; i < records.Length; ++i)
            {
                Assert.IsNull(schema.FindDifferentColumn(records[i], read[i]), Invariant($"records[{i}]"));
                Assert.AreEqual(((IRecord)records[i]).Symbol, ((IRecord)read[i]).Symbol, Invariant($"records[{i}].Symbol"));
            }

            StringAssert.StartsWith(ReadText(path), "[{", "a JSON array of the records, readable by hand");
        }
    }
}
