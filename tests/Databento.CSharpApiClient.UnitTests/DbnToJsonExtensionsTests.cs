// Ignore Spelling: Databento Bbo Tcbbo Json Cbbo Ohlcv Dbn

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// <see cref="DbnToJsonExtensions"/> gives a DBN record as the record the JSON client reads from the JSON encoding of the same
    /// record, so records downloaded as DBN can be saved as JSON and read back with no trace of the transport. Each test pairs a
    /// DBN record with the JSON line the API sends for it (pretty prices and timestamps, as the client requests them).
    /// </summary>
    [TestClass]
    public class DbnToJsonExtensionsTests
    {
        private const string AnyDataset = Datasets.OpraPillar;
        private const string AnyApiKey = "test-api-key-0000";

        private static readonly DateTimeOffset AnyStart = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AnyEnd = new DateTimeOffset(2026, 10, 17, 0, 0, 0, TimeSpan.Zero);

        // =====================================================================
        // CBBO
        // =====================================================================

        [TestMethod]
        public async Task Cbbo_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "SPXW  261016C06600000";
            CbboRecordDbn dbn = new CbboRecordDbn
            {
                RecordType = RType.Cbbo1M,
                PublisherId = 30,
                InstrumentId = 1234567,
                TsEventUtc = Utc(2026, 10, 16, 14, 31, 0),
                // the decoder keeps whole 100 ns ticks of 123456789 ns
                TsReceivedUtc = Utc(2026, 10, 16, 14, 31, 0).AddTicks(1234567),
                Side = TradeAggressor.Seller,
                Price = 4.01,
                Size = 3,
                Flags = MessageInfoBits.Last | MessageInfoBits.PublisherSpecific,
                BidPrice = 3.9,
                AskPrice = 4.1,
                BidSize = 185,
                AskSize = 147,
                BidPublisherId = 31,
                AskPublisherId = 33,
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:31:00.123456789Z\","
                + "\"hd\":{\"ts_event\":\"2026-10-16T14:31:00.000000000Z\",\"rtype\":193,\"publisher_id\":30,\"instrument_id\":1234567},"
                + "\"side\":\"A\",\"price\":\"4.010000000\",\"size\":3,\"flags\":130,"
                + "\"levels\":[{\"bid_px\":\"3.900000000\",\"ask_px\":\"4.100000000\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":31,\"ask_pb\":33}]}";

            CbboRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetCbbo1mAsync(AnyDataset, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<ICbboRecord>(dbn, json);
        }

        [TestMethod]
        public async Task Cbbo_UndefinedValues_MapAsTheJsonClientReadsThem()
        {
            // no trade in the interval and nobody bidding: the API sends null prices, which the DBN decoder reads as NaN
            const string symbol = "SPXW  261016C06600000";
            CbboRecordDbn dbn = new CbboRecordDbn
            {
                RecordType = RType.Cbbo1M,
                PublisherId = 30,
                InstrumentId = 1234567,
                TsEventUtc = DateTime.MinValue,
                TsReceivedUtc = Utc(2026, 10, 16, 14, 32, 0),
                Side = TradeAggressor.None,
                Price = double.NaN,
                Size = 0,
                Flags = MessageInfoBits.None,
                BidPrice = double.NaN,
                AskPrice = 0.05,
                BidSize = 0,
                AskSize = 10,
                BidPublisherId = 0,
                AskPublisherId = 33,
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:32:00.000000000Z\","
                + "\"hd\":{\"ts_event\":null,\"rtype\":193,\"publisher_id\":30,\"instrument_id\":1234567},"
                + "\"side\":\"N\",\"price\":null,\"size\":0,\"flags\":0,"
                + "\"levels\":[{\"bid_px\":null,\"ask_px\":\"0.050000000\",\"bid_sz\":0,\"ask_sz\":10,\"bid_pb\":0,\"ask_pb\":33}]}";

            CbboRecordJson json = dbn.ToJsonRecord();

            Assert.IsNull(json.Header.TsEventUtc);
            Assert.IsNull(json.Price);
            Assert.IsTrue(double.IsNaN(json.Level1.BidPrice));
            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetCbbo1mAsync(AnyDataset, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<ICbboRecord>(dbn, json);
        }

        [TestMethod]
        public void ToJsonRecord_ValueBeyondTheJsonModel_Throws()
        {
            // the JSON models hold CBBO and TCBBO book sizes as int and a volume as long, so the JSON encoding of such a record
            // can't be read either; wrapping silently would give a negative size
            CbboRecordDbn cbboTrade = new CbboRecordDbn { Size = uint.MaxValue };
            CbboRecordDbn cbboBid = new CbboRecordDbn { BidSize = (uint)int.MaxValue + 1 };
            CbboRecordDbn cbboAsk = new CbboRecordDbn { AskSize = uint.MaxValue };
            TcbboRecordDbn tcbboBid = new TcbboRecordDbn { Level = new CbboLevelDbn { BidSize = uint.MaxValue } };
            TcbboRecordDbn tcbboAsk = new TcbboRecordDbn { Level = new CbboLevelDbn { AskSize = uint.MaxValue } };
            OhlcvRecordDbn ohlcv = new OhlcvRecordDbn { Volume = (ulong)long.MaxValue + 1 };

            Assert.ThrowsException<OverflowException>(() => cbboTrade.ToJsonRecord());
            Assert.ThrowsException<OverflowException>(() => cbboBid.ToJsonRecord());
            Assert.ThrowsException<OverflowException>(() => cbboAsk.ToJsonRecord());
            Assert.ThrowsException<OverflowException>(() => tcbboBid.ToJsonRecord());
            Assert.ThrowsException<OverflowException>(() => tcbboAsk.ToJsonRecord());
            Assert.ThrowsException<OverflowException>(() => ohlcv.ToJsonRecord());
        }

        [TestMethod]
        public void ToJsonRecord_LargestValuesTheJsonModelHolds_Map()
        {
            // a TCBBO trade size is uint in both models, so only its book sizes are bounded
            CbboRecordDbn cbbo = new CbboRecordDbn { Size = int.MaxValue, BidSize = int.MaxValue, AskSize = int.MaxValue };
            TcbboRecordDbn tcbbo = new TcbboRecordDbn { Size = uint.MaxValue, Level = new CbboLevelDbn { BidSize = int.MaxValue, AskSize = int.MaxValue } };
            OhlcvRecordDbn ohlcv = new OhlcvRecordDbn { Volume = long.MaxValue };

            Assert.AreEqual(int.MaxValue, cbbo.ToJsonRecord().Level1.AskSize);
            Assert.AreEqual(uint.MaxValue, tcbbo.ToJsonRecord().Size);
            Assert.AreEqual(int.MaxValue, tcbbo.ToJsonRecord().Level1.BidSize);
            Assert.AreEqual(long.MaxValue, ohlcv.ToJsonRecord().Volume);
        }

        [TestMethod]
        public void ToJsonRecord_NullRecord_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((BboRecordDbn)null).ToJsonRecord());
            Assert.ThrowsException<ArgumentNullException>(() => ((CbboRecordDbn)null).ToJsonRecord());
            Assert.ThrowsException<ArgumentNullException>(() => ((TcbboRecordDbn)null).ToJsonRecord());
            Assert.ThrowsException<ArgumentNullException>(() => ((TradeRecordDbn)null).ToJsonRecord());
            Assert.ThrowsException<ArgumentNullException>(() => ((OhlcvRecordDbn)null).ToJsonRecord());
            Assert.ThrowsException<ArgumentNullException>(() => ((StatisticsRecordDbn)null).ToJsonRecord());
        }

        // =====================================================================
        // BBO
        // =====================================================================

        [TestMethod]
        public async Task Bbo_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "MNQZ6";
            BboRecordDbn dbn = new BboRecordDbn
            {
                RecordType = RType.Bbo1M,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 59).AddTicks(9876543),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 31, 0),
                Price = 21000.25,
                Size = 3,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                Sequence = 77,
                Level = new Mbp1LevelDbn
                {
                    BidPrice = 21000.0,
                    AskPrice = 21000.5,
                    BidSize = 10,
                    AskSize = 12,
                    BidCount = 4,
                    AskCount = 5,
                },
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:31:00.000000000Z\","
                + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:59.987654300Z\",\"rtype\":196,\"publisher_id\":1,\"instrument_id\":42},"
                + "\"side\":\"B\",\"price\":\"21000.250000000\",\"size\":3,\"flags\":128,\"sequence\":77,"
                + "\"levels\":[{\"bid_px\":\"21000.000000000\",\"ask_px\":\"21000.500000000\",\"bid_sz\":10,\"ask_sz\":12,\"bid_ct\":4,\"ask_ct\":5}]}";

            BboRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetBbo1mAsync(AnyDataset, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<IBboRecord>(dbn, json);
        }

        [TestMethod]
        public async Task Bbo_UndefinedValues_MapAsTheJsonClientReadsThem()
        {
            const string symbol = "MNQZ6";
            BboRecordDbn dbn = new BboRecordDbn
            {
                RecordType = RType.Bbo1S,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 0),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 30, 1),
                Price = double.NaN,
                Size = 0,
                Side = TradeAggressor.None,
                Flags = MessageInfoBits.None,
                Sequence = 78,
                Level = new Mbp1LevelDbn
                {
                    BidPrice = 21000.0,
                    AskPrice = double.NaN,
                    BidSize = 10,
                    AskSize = 0,
                    BidCount = 4,
                    AskCount = 0,
                },
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:30:01.000000000Z\","
                + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:00.000000000Z\",\"rtype\":195,\"publisher_id\":1,\"instrument_id\":42},"
                + "\"side\":\"N\",\"price\":null,\"size\":0,\"flags\":0,\"sequence\":78,"
                + "\"levels\":[{\"bid_px\":\"21000.000000000\",\"ask_px\":null,\"bid_sz\":10,\"ask_sz\":0,\"bid_ct\":4,\"ask_ct\":0}]}";

            BboRecordJson json = dbn.ToJsonRecord();

            Assert.IsNull(json.Price);
            Assert.IsTrue(double.IsNaN(json.Level1.AskPrice));
            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetBbo1sAsync(AnyDataset, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<IBboRecord>(dbn, json);
        }

        // =====================================================================
        // TCBBO
        // =====================================================================

        [TestMethod]
        public async Task Tcbbo_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "SPXW  261016P06500000";
            TcbboRecordDbn dbn = new TcbboRecordDbn
            {
                RecordType = RType.Tcbbo,
                PublisherId = 30,
                InstrumentId = 7654321,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 5).AddTicks(1),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 30, 5).AddTicks(1234),
                Price = 4.05,
                Size = 2,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsInDelta = 1500,
                Level = new CbboLevelDbn
                {
                    BidPrice = 4.0,
                    AskPrice = 4.1,
                    BidSize = 7,
                    AskSize = 25,
                    BidPublisherId = 31,
                    AskPublisherId = 46,
                },
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:30:05.000123400Z\","
                + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:05.000000100Z\",\"rtype\":194,\"publisher_id\":30,\"instrument_id\":7654321},"
                + "\"action\":\"T\",\"side\":\"B\",\"flags\":128,\"price\":\"4.050000000\",\"size\":2,\"ts_in_delta\":1500,"
                + "\"levels\":[{\"bid_px\":\"4.000000000\",\"ask_px\":\"4.100000000\",\"bid_sz\":7,\"ask_sz\":25,\"bid_pb\":31,\"ask_pb\":46}]}";

            TcbboRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetTcbboAsync(AnyDataset, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<ITcbboRecord>(dbn, json);
        }

        [TestMethod]
        public void BboAndTcbbo_WithoutALevel_MapToNoLevels()
        {
            // the decoder always reads a level, but a record built by hand may have none, which the interfaces read as an empty book
            BboRecordDbn bbo = new BboRecordDbn
            {
                RecordType = RType.Bbo1S,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 0),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 30, 1),
                Price = double.NaN,
                Side = TradeAggressor.None,
            };
            TcbboRecordDbn tcbbo = new TcbboRecordDbn
            {
                RecordType = RType.Tcbbo,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 0),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 30, 1),
                Price = 4.05,
                Side = TradeAggressor.Buyer,
            };

            BboRecordJson bboJson = bbo.ToJsonRecord();
            TcbboRecordJson tcbboJson = tcbbo.ToJsonRecord();

            Assert.IsNull(bboJson.Levels);
            Assert.IsNull(tcbboJson.Levels);
            AssertSameThroughInterface<IBboRecord>(bbo, bboJson);
            AssertSameThroughInterface<ITcbboRecord>(tcbbo, tcbboJson);
        }

        // =====================================================================
        // Trades
        // =====================================================================

        [TestMethod]
        public async Task Trade_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "SPY";
            TradeRecordDbn dbn = new TradeRecordDbn
            {
                RecordType = RType.Mbp0,
                PublisherId = 2,
                InstrumentId = 15144,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 0).AddTicks(1234),
                TsReceivedUtc = Utc(2026, 10, 16, 14, 30, 0).AddTicks(2345),
                Price = 580.12,
                Size = 100,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                Depth = 1,
                TsInDelta = 18456,
                Sequence = 1234567,
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2026-10-16T14:30:00.000234500Z\","
                + "\"hd\":{\"ts_event\":\"2026-10-16T14:30:00.000123400Z\",\"rtype\":0,\"publisher_id\":2,\"instrument_id\":15144},"
                + "\"action\":\"T\",\"side\":\"A\",\"depth\":1,\"price\":\"580.120000000\",\"size\":100,\"flags\":128,"
                + "\"ts_in_delta\":18456,\"sequence\":1234567}";

            TradeRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetTradesAsync(Datasets.XnasItch, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<ITradeRecord>(dbn, json);
        }

        // =====================================================================
        // OHLCV
        // =====================================================================

        [TestMethod]
        public async Task Ohlcv_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "SPY";
            OhlcvRecordDbn dbn = new OhlcvRecordDbn
            {
                RecordType = RType.Ohlcv1M,
                PublisherId = 2,
                InstrumentId = 15144,
                TsEventUtc = Utc(2026, 10, 16, 14, 30, 0),
                Open = 580.1,
                High = 580.5,
                Low = 579.9,
                Close = 580.25,
                Volume = 125300,
                Symbol = symbol,
            };
            string apiLine = "{\"hd\":{\"ts_event\":\"2026-10-16T14:30:00.000000000Z\",\"rtype\":33,\"publisher_id\":2,\"instrument_id\":15144},"
                + "\"open\":\"580.100000000\",\"high\":\"580.500000000\",\"low\":\"579.900000000\",\"close\":\"580.250000000\",\"volume\":\"125300\"}";

            OhlcvRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetOhlcv1mAsync(Datasets.XnasItch, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<IOhlcvRecord>(dbn, json);
        }

        // =====================================================================
        // Statistics
        // =====================================================================

        [TestMethod]
        public async Task Statistics_ToJsonRecord_IsTheRecordTheJsonClientReads()
        {
            const string symbol = "ESH4";
            StatisticsRecordDbn dbn = new StatisticsRecordDbn
            {
                RecordType = RType.Statistics,
                PublisherId = 1,
                InstrumentId = 17077,
                TsEventUtc = Utc(2024, 3, 15, 0, 22, 48).AddTicks(123400),
                TsReceivedUtc = Utc(2024, 3, 15, 0, 22, 48).AddTicks(125600),
                TsRefUtc = Utc(2024, 3, 14, 0, 0, 0),
                Price = 4321.75,
                Quantity = 2048000,
                Sequence = 12345678,
                TsInDelta = 9000,
                StatType = 3,
                ChannelId = 2,
                UpdateAction = 1,
                StatFlags = MessageInfoBits.PublisherSpecific,
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2024-03-15T00:22:48.012560000Z\","
                + "\"hd\":{\"ts_event\":\"2024-03-15T00:22:48.012340000Z\",\"rtype\":24,\"publisher_id\":1,\"instrument_id\":17077},"
                + "\"ts_ref\":\"2024-03-14T00:00:00.000000000Z\",\"price\":\"4321.750000000\",\"quantity\":\"2048000\","
                + "\"sequence\":12345678,\"ts_in_delta\":9000,\"stat_type\":3,\"channel_id\":2,\"update_action\":1,\"stat_flags\":2}";

            StatisticsRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetStatisticsAsync(Datasets.GlbxMdp3, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<IStatisticsRecord>(dbn, json);
        }

        [TestMethod]
        public async Task Statistics_UndefinedValues_MapAsTheJsonClientReadsThem()
        {
            // a settlement price has no quantity and an open-interest count no price; this record has neither, nor a reference time
            const string symbol = "ESH4";
            StatisticsRecordDbn dbn = new StatisticsRecordDbn
            {
                RecordType = RType.Statistics,
                PublisherId = 1,
                InstrumentId = 17077,
                TsEventUtc = Utc(2024, 3, 15, 0, 22, 48),
                TsReceivedUtc = Utc(2024, 3, 15, 0, 22, 49),
                TsRefUtc = null,
                Price = double.NaN,
                Quantity = long.MaxValue,
                Sequence = 12345679,
                TsInDelta = 0,
                StatType = 5,
                ChannelId = 0,
                UpdateAction = 2,
                StatFlags = MessageInfoBits.None,
                Symbol = symbol,
            };
            string apiLine = "{\"ts_recv\":\"2024-03-15T00:22:49.000000000Z\","
                + "\"hd\":{\"ts_event\":\"2024-03-15T00:22:48.000000000Z\",\"rtype\":24,\"publisher_id\":1,\"instrument_id\":17077},"
                + "\"ts_ref\":null,\"price\":null,\"quantity\":\"9223372036854775807\","
                + "\"sequence\":12345679,\"ts_in_delta\":0,\"stat_type\":5,\"channel_id\":0,\"update_action\":2,\"stat_flags\":0}";

            StatisticsRecordJson json = dbn.ToJsonRecord();

            await AssertIsTheJsonClientsRecordAsync(json, apiLine, c => c.GetStatisticsAsync(Datasets.GlbxMdp3, symbol, AnyStart, AnyEnd));
            AssertSameThroughInterface<IStatisticsRecord>(dbn, json);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static DateTime Utc(int year, int month, int day, int hour, int minute, int second)
            => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

        // The mapped record serializes exactly as the record the JSON client reads from the API's line, and what it serializes
        // to reads back as the same record, through the client and through plain System.Text.Json alike, so a saved DBN download
        // is indistinguishable from a JSON one.
        private static async Task AssertIsTheJsonClientsRecordAsync<TJson>(TJson mapped, string apiLine, Func<DatabentoJsonClient, Task<TJson[]>> fetch)
        {
            string fromApi = JsonSerializer.Serialize(await FetchOneAsync(apiLine, fetch));
            string fromDbn = JsonSerializer.Serialize(mapped);
            Assert.AreEqual(fromApi, fromDbn);

            string readBack = JsonSerializer.Serialize(await FetchOneAsync(fromDbn, fetch));
            Assert.AreEqual(fromApi, readBack);

            string readBackPlain = JsonSerializer.Serialize(JsonSerializer.Deserialize<TJson>(fromDbn));
            Assert.AreEqual(fromApi, readBackPlain);
        }

        private static async Task<TJson> FetchOneAsync<TJson>(string responseLine, Func<DatabentoJsonClient, Task<TJson[]>> fetch)
        {
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseLine, Encoding.UTF8, "application/json"),
            });

            using DatabentoJsonClient client = new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
            TJson[] records = await fetch(client);
            Assert.AreEqual(1, records.Length);
            return records[0];
        }

        // every property of the record interface, its base interfaces' included, reads the same from both records
        private static void AssertSameThroughInterface<TInterface>(TInterface dbn, TInterface json)
        {
            IEnumerable<PropertyInfo> properties = new[] { typeof(TInterface) }
                .Concat(typeof(TInterface).GetInterfaces())
                .SelectMany(t => t.GetProperties());
            foreach(PropertyInfo property in properties)
            {
                Assert.AreEqual(property.GetValue(dbn), property.GetValue(json), property.DeclaringType.Name + "." + property.Name);
            }
        }
    }
}
