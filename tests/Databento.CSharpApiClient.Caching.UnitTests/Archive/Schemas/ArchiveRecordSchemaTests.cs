// Ignore Spelling: Bbo Cbbo Tcbbo Ohlcv

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive.Schemas
{
    /// <summary>
    /// Every dataset's records must come back from an archive file with every value of the record interface they went in with,
    /// whether they were written from the JSON or the DBN records
    /// </summary>
    [TestClass]
    public class ArchiveRecordSchemaTests
    {
        private static readonly DateTime TsEvent = new DateTime(2026, 10, 16, 14, 30, 0, DateTimeKind.Utc);

        [TestMethod]
        public async Task Cbbo_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<ICbboRecord, CbboRecordDbn>(new CbboArchiveSchema(RType.Cbbo1M));
        }

        [TestMethod]
        public async Task Bbo_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<IBboRecord, BboRecordDbn>(new BboArchiveSchema(RType.Bbo1S));
        }

        [TestMethod]
        public async Task Tcbbo_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<ITcbboRecord, TcbboRecordDbn>(new TcbboArchiveSchema(RType.Tcbbo));
        }

        [TestMethod]
        public async Task Ohlcv_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<IOhlcvRecord, OhlcvRecordDbn>(new OhlcvArchiveSchema(RType.Ohlcv1M));
        }

        [TestMethod]
        public async Task Trades_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<ITradeRecord, TradeRecordDbn>(new TradeArchiveSchema(RType.Mbp0));
        }

        [TestMethod]
        public async Task Statistics_FullyPopulatedRecords_RoundTripExactly()
        {
            await AssertFullRoundTripAsync<IStatisticsRecord, StatisticsRecordDbn>(new StatisticsArchiveSchema(RType.Statistics));
        }

        [TestMethod]
        public async Task Cbbo_JsonRecordsOfADayFile_RoundTrip()
        {
            // a day file holds JSON records, which name their symbol only when the request covered more than one
            CbboRecordJson record = new CbboRecordJson
            {
                Header = new RecordHeaderJson
                {
                    RecordType = RType.Cbbo1M,
                    PublisherId = 30,
                    InstrumentId = 1234567,
                    TsEventUtc = TsEvent,
                },
                TsReceivedUtc = TsEvent.AddTicks(1234567),
                Side = TradeAggressor.Seller,
                Price = 4.05,
                Size = 7,
                Flags = MessageInfoBits.Last,
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = 4.0,
                        AskPrice = 4.1,
                        BidSize = 120,
                        AskSize = 95,
                        BidPublisherId = 31,
                        AskPublisherId = 32,
                    },
                },
                Symbol = null,
            };

            await AssertJsonRoundTripAsync<ICbboRecord, CbboRecordDbn>(new CbboArchiveSchema(RType.Cbbo1M), record);
        }

        [TestMethod]
        public async Task Statistics_JsonRecordsOfADayFile_RoundTrip()
        {
            StatisticsRecordJson record = new StatisticsRecordJson
            {
                Header = Header(RType.Statistics),
                TsReceivedUtc = TsEvent.AddTicks(1234567),
                TsRefUtc = null,
                Price = double.NaN,
                Quantity = 4200,
                Sequence = 77,
                TsInDelta = 5150,
                StatType = 9,
                ChannelId = 3,
                UpdateAction = 1,
                StatFlags = MessageInfoBits.Last,
            };

            await AssertJsonRoundTripAsync<IStatisticsRecord, StatisticsRecordDbn>(new StatisticsArchiveSchema(RType.Statistics), record);
        }

        [TestMethod]
        public async Task Bbo_JsonRecordsOfADayFile_RoundTrip()
        {
            BboRecordJson record = new BboRecordJson
            {
                Header = Header(RType.Bbo1S),
                Price = 21000.25,
                Size = 3,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsEvent.AddTicks(1234567),
                Sequence = 77,
                Levels = new[]
                {
                    new Mbp1LevelJson
                    {
                        BidPrice = 21000.0,
                        AskPrice = 21000.5,
                        BidSize = 10,
                        AskSize = 12,
                        BidCount = 4,
                        AskCount = 5,
                    },
                },
            };

            await AssertJsonRoundTripAsync<IBboRecord, BboRecordDbn>(new BboArchiveSchema(RType.Bbo1S), record);
        }

        [TestMethod]
        public async Task Tcbbo_JsonRecordsOfADayFile_RoundTrip()
        {
            TcbboRecordJson record = new TcbboRecordJson
            {
                Header = Header(RType.Tcbbo),
                Price = 4.05,
                Size = 2,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsEvent.AddTicks(1234567),
                TsInDelta = 5150,
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = 4.0,
                        AskPrice = double.NaN,
                        BidSize = 20,
                        AskSize = 30,
                        BidPublisherId = 21,
                        AskPublisherId = 22,
                    },
                },
            };

            await AssertJsonRoundTripAsync<ITcbboRecord, TcbboRecordDbn>(new TcbboArchiveSchema(RType.Tcbbo), record);
        }

        [TestMethod]
        public async Task Trades_JsonRecordsOfADayFile_RoundTrip()
        {
            TradeRecordJson record = new TradeRecordJson
            {
                Header = Header(RType.Mbp0),
                Price = 6600.25,
                Size = 0,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.None,
                Flags = MessageInfoBits.Last,
                Depth = 0,
                TsReceivedUtc = TsEvent.AddTicks(1234567),
                TsInDelta = 950,
                Sequence = 123456,
            };

            await AssertJsonRoundTripAsync<ITradeRecord, TradeRecordDbn>(new TradeArchiveSchema(RType.Mbp0), record);
        }

        [TestMethod]
        public async Task Ohlcv_JsonRecordsOfADayFile_RoundTrip()
        {
            OhlcvRecordJson record = new OhlcvRecordJson
            {
                Header = Header(RType.Ohlcv1M),
                Open = 600.1,
                High = 601.2,
                Low = 599.3,
                Close = 600.4,
                Volume = 125000,
            };

            await AssertJsonRoundTripAsync<IOhlcvRecord, OhlcvRecordDbn>(new OhlcvArchiveSchema(RType.Ohlcv1M), record);
        }

        [TestMethod]
        public async Task Cbbo_UndefinedValues_ComeBackUndefined()
        {
            // a minute with no trade has no price and no event time, and an unquoted side has no price
            CbboRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);
            record.TsEventUtc = DateTime.MinValue;
            record.Price = double.NaN;
            record.BidPrice = double.NaN;

            CbboRecordDbn[] read = await ArchiveTestRecords.RoundTripAsync<ICbboRecord, CbboRecordDbn>(new CbboArchiveSchema(RType.Cbbo1M), new[] { record });

            ICbboRecord view = read[0];
            Assert.IsNull(view.TsEventUtc);
            Assert.IsNull(view.Price);
            Assert.IsNull(view.BidPrice);
            Assert.AreEqual(((ICbboRecord)record).AskPrice, view.AskPrice);

            // the DBN record holds them as its own undefined values
            Assert.AreEqual(DateTime.MinValue, read[0].TsEventUtc);
            Assert.IsTrue(double.IsNaN(read[0].Price));
            Assert.IsTrue(double.IsNaN(read[0].BidPrice));
        }

        [TestMethod]
        public async Task Statistics_UndefinedValues_ComeBackUndefined()
        {
            // open interest carries no price, and a price statistic no quantity
            StatisticsRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<StatisticsRecordDbn>(1);
            record.Price = double.NaN;
            record.Quantity = long.MaxValue;
            record.TsRefUtc = null;
            record.TsReceivedUtc = DateTime.MinValue;

            StatisticsRecordDbn[] read = await ArchiveTestRecords.RoundTripAsync<IStatisticsRecord, StatisticsRecordDbn>(new StatisticsArchiveSchema(RType.Statistics), new[] { record });

            IStatisticsRecord view = read[0];
            Assert.IsNull(view.Price);
            Assert.IsNull(view.Quantity);
            Assert.IsNull(view.TsRefUtc);
            Assert.IsNull(view.TsReceivedUtc);
            Assert.AreEqual(long.MaxValue, read[0].Quantity);
        }

        [TestMethod]
        public async Task Bbo_RecordWithoutALevel_StoresItsSidesAsUnquoted()
        {
            BboRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<BboRecordDbn>(1);
            record.Level = null;

            BboRecordDbn[] read = await ArchiveTestRecords.RoundTripAsync<IBboRecord, BboRecordDbn>(new BboArchiveSchema(RType.Bbo1S), new[] { record });

            IBboRecord view = read[0];
            Assert.IsNull(view.BidPrice);
            Assert.IsNull(view.AskPrice);
            Assert.AreEqual(0u, view.BidSize);
            Assert.AreEqual(0u, view.AskSize);
            Assert.AreEqual(0u, view.BidCount);
            Assert.AreEqual(0u, view.AskCount);
        }

        [TestMethod]
        public async Task Statistics_UndefinedV1QuantityAndReferenceTime_ComeBackUndefined()
        {
            // DBN v1 sends a 32-bit undefined quantity, and an undefined ts_ref can reach a record as the default date
            StatisticsRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<StatisticsRecordDbn>(1);
            record.Quantity = int.MaxValue;
            record.TsRefUtc = DateTime.MinValue;

            StatisticsRecordDbn[] read = await ArchiveTestRecords.RoundTripAsync<IStatisticsRecord, StatisticsRecordDbn>(new StatisticsArchiveSchema(RType.Statistics), new[] { record });

            IStatisticsRecord view = read[0];
            Assert.IsNull(view.Quantity);
            Assert.IsNull(view.TsRefUtc);
        }

        [TestMethod]
        public async Task Cbbo_JsonRecordWithoutHeader_IsRefused()
        {
            // a record without a header reads as record type 0, which no quote schema holds
            CbboRecordJson record = new CbboRecordJson
            {
                TsReceivedUtc = TsEvent,
                Levels = new[] { new CbboLevel1RecordJson() },
            };

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveTestRecords.RoundTripAsync(new CbboArchiveSchema(RType.Cbbo1M), new ICbboRecord[] { record }));
        }

        [TestMethod]
        [DataRow(RType.Tcbbo, DisplayName = "a record of another schema")]
        [DataRow(RType.Cbbo1S, DisplayName = "a 1-second quote in the 1-minute schema")]
        public async Task Cbbo_RecordOfAnotherRecordType_IsRefused(RType recordType)
        {
            CbboRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);
            record.RecordType = recordType;

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveTestRecords.RoundTripAsync<ICbboRecord, CbboRecordDbn>(new CbboArchiveSchema(RType.Cbbo1M), new[] { record }));
        }

        [TestMethod]
        public async Task Cbbo_TimestampThatIsNotUtc_IsRefused()
        {
            // only a UTC time comes back as it went in; a local one would come back shifted by the machine's offset
            CbboRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);
            record.TsReceivedUtc = DateTime.SpecifyKind(record.TsReceivedUtc, DateTimeKind.Local);

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveTestRecords.RoundTripAsync<ICbboRecord, CbboRecordDbn>(new CbboArchiveSchema(RType.Cbbo1M), new[] { record }));
        }

        [TestMethod]
        public void ParquetSchema_Cbbo_StartsWithTheRawSymbolAndDateOfEachRow()
        {
            CbboArchiveSchema schema = new CbboArchiveSchema(RType.Cbbo1M);

            CollectionAssert.AreEqual(
                new[] { "raw_symbol", "date", "ts_event", "rtype", "publisher_id", "instrument_id", "ts_recv", "side", "price", "size", "flags", "bid_px", "ask_px", "bid_sz", "ask_sz", "bid_pb", "ask_pb" },
                schema.ParquetSchema.DataFields.Select(field => field.Name).ToArray());
            Assert.AreEqual(RType.Cbbo1M, schema.RecordType);
        }

        [TestMethod]
        public void FindDifferentColumn_RecordsDifferingInOneValue_NamesItsColumn()
        {
            CbboArchiveSchema schema = new CbboArchiveSchema(RType.Cbbo1M);
            CbboRecordDbn expected = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);
            CbboRecordDbn actual = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);

            Assert.IsNull(schema.FindDifferentColumn(expected, actual));

            actual.AskSize += 1;
            Assert.AreEqual("ask_sz", schema.FindDifferentColumn(expected, actual));

            actual.TsReceivedUtc = actual.TsReceivedUtc.AddTicks(1);
            Assert.AreEqual("ts_recv", schema.FindDifferentColumn(expected, actual), "the first column that differs");
        }

        [TestMethod]
        public void FindDifferentColumn_UndefinedPriceAsNullOrNaN_IsTheSameValue()
        {
            // a JSON record holds a missing trade price as null, a DBN record as NaN
            CbboArchiveSchema schema = new CbboArchiveSchema(RType.Cbbo1M);
            CbboRecordJson json = new CbboRecordJson
            {
                Header = Header(RType.Cbbo1M),
                Price = null,
                Side = TradeAggressor.None,
                Levels = new[] { new CbboLevel1RecordJson { BidPrice = double.NaN, AskPrice = 0.05 } },
            };
            CbboRecordDbn dbn = new CbboRecordDbn
            {
                RecordType = RType.Cbbo1M,
                PublisherId = 30,
                InstrumentId = 1234567,
                TsEventUtc = TsEvent,
                Price = double.NaN,
                Side = TradeAggressor.None,
                BidPrice = double.NaN,
                AskPrice = 0.05,
            };

            Assert.IsNull(schema.FindDifferentColumn(json, dbn));
        }

        [TestMethod]
        public void FindUnwritableValue_RecordTheSchemaCannotHold_SaysWhy()
        {
            CbboArchiveSchema schema = new CbboArchiveSchema(RType.Cbbo1M);
            CbboRecordDbn record = ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>(1);

            Assert.IsNull(schema.FindUnwritableValue(record));

            record.TsReceivedUtc = DateTime.SpecifyKind(record.TsReceivedUtc, DateTimeKind.Unspecified);
            StringAssert.Contains(schema.FindUnwritableValue(record), "only UTC times");

            record.RecordType = RType.Cbbo1S;
            StringAssert.Contains(schema.FindUnwritableValue(record), "of type Cbbo1S");
        }

        [TestMethod]
        public void FindUnwritableValue_ValueTheRecordInterfaceCannotGive_SaysWhy()
        {
            // TcbboRecordJson holds ts_in_delta as a long and gives it as the interface's int by a checked cast
            TcbboArchiveSchema schema = new TcbboArchiveSchema(RType.Tcbbo);
            TcbboRecordJson record = new TcbboRecordJson
            {
                Header = Header(RType.Tcbbo),
                Side = TradeAggressor.Buyer,
                TsReceivedUtc = TsEvent,
                TsInDelta = int.MaxValue,
            };

            Assert.IsNull(schema.FindUnwritableValue(record));

            record.TsInDelta = (long)int.MaxValue + 1;
            StringAssert.Contains(schema.FindUnwritableValue(record), "ts_in_delta");
        }

        private static RecordHeaderJson Header(RType recordType)
        {
            return new RecordHeaderJson
            {
                RecordType = recordType,
                PublisherId = 30,
                InstrumentId = 1234567,
                TsEventUtc = TsEvent,
            };
        }

        private static async Task AssertJsonRoundTripAsync<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema, TSource record)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            TSource[] records = new[] { record };
            ArchiveTestRecords.AssertSameValues(records, await ArchiveTestRecords.RoundTripAsync(schema, records), ArchiveTestRecords.RawSymbolOf(schema));
        }

        private static async Task AssertFullRoundTripAsync<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema)
            where TSource : class, IRecord
            where TRecord : class, TSource, new()
        {
            TSource[] records = new TSource[] { ArchiveTestRecords.CreateFullyPopulated<TRecord>(1), ArchiveTestRecords.CreateFullyPopulated<TRecord>(2), ArchiveTestRecords.CreateFullyPopulated<TRecord>(3) };

            ArchiveTestRecords.AssertSameValues(records, await ArchiveTestRecords.RoundTripAsync(schema, records), ArchiveTestRecords.RawSymbolOf(schema));
        }
    }
}
