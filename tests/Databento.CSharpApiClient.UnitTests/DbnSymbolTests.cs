using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// The DBN decoders give each record the requested symbol its instrument ID maps to, on the date of the record's index
    /// timestamp (ts_recv, or ts_event for a record without one), from the symbol mappings in the stream's metadata.
    /// </summary>
    [TestClass]
    public class DbnSymbolTests
    {
        private const string AnyDataset = "OPRA.PILLAR";
        private const string AnySymbol = "SPXW  261016C06600000";

        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset End = Start.AddDays(2);
        private static readonly DateOnly Day = DateOnly.FromDateTime(Start.UtcDateTime);

        [TestMethod]
        public async Task Tcbbo_EachInstrument_GetsItsMappedSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start), Tcbbo(801, Start), Tcbbo(800, Start.AddMinutes(1))),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")),
                Mapping("SPXW  261016P06600000", Interval(Day, Day.AddDays(1), "801")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            CollectionAssert.AreEqual(
                new[] { "SPXW  261016C06600000", "SPXW  261016P06600000", "SPXW  261016C06600000" },
                Array.ConvertAll(records, record => record.Symbol));
        }

        [TestMethod]
        public async Task Tcbbo_InstrumentIdReusedNextDay_GetsEachDaysSymbol()
        {
            // venues reassign instrument IDs, so the same ID names another contract on another day
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start), Tcbbo(800, Start.AddDays(1))),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")),
                Mapping("SPXW  261017C06600000", Interval(Day.AddDays(1), Day.AddDays(2), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPXW  261016C06600000", records[0].Symbol);
            Assert.AreEqual("SPXW  261017C06600000", records[1].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_ReceivedOnTheNextDay_TakesTheReceiveDate()
        {
            DateTimeOffset beforeMidnight = new DateTimeOffset(2022, 5, 16, 23, 59, 59, TimeSpan.Zero);
            TcbboSeed seed = Tcbbo(800, beforeMidnight);
            seed.TsReceived = beforeMidnight.AddSeconds(2);
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(seed),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")),
                Mapping("SPXW  261017C06600000", Interval(Day.AddDays(1), Day.AddDays(2), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPXW  261017C06600000", records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_IntervalEnd_IsExclusive()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start.AddDays(1))),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.IsNull(records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_UnmappedInstrument_HasNoSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(999, Start)),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.IsNull(records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_ReceiveTimeUndefined_TakesTheEventDate()
        {
            TcbboSeed seed = Tcbbo(800, Start.AddDays(1));
            seed.TsReceived = default;
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(seed),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")),
                Mapping("SPXW  261017C06600000", Interval(Day.AddDays(1), Day.AddDays(2), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPXW  261017C06600000", records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_SymbolWithSeveralIntervals_MatchesTheLaterOne()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(801, Start.AddDays(1))),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800"), Interval(Day.AddDays(1), Day.AddDays(2), "801")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPXW  261016C06600000", records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_WideSymbols_AreRead()
        {
            // the live API sends 71-byte symbols in DBN v2 and v3 metadata
            byte[] stream = DbnBinaryBuilder.WithWideSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start)),
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPXW  261016C06600000", records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_NoMappings_HasNoSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start)));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.IsNull(records[0].Symbol);
        }

        [TestMethod]
        public async Task Tcbbo_MappingsNotToInstrumentIds_GiveNoSymbol()
        {
            // only a mapping to instrument IDs says which record a symbol belongs to
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800, Start)),
                stypeOut: 0x01 /*RawSymbol*/,
                Mapping("SPXW  261016C06600000", Interval(Day, Day.AddDays(1), "800")));

            TcbboRecordDbn[] records = await BuildClientWithBytes(stream).GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.IsNull(records[0].Symbol);
        }

        [TestMethod]
        public async Task Bbo_GetsItsMappedSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildBboStream(0xC3, new BboSeed { InstrumentId = 800, TsEvent = Start, TsReceived = Start }),
                Mapping("MNQZ6", Interval(Day, Day.AddDays(1), "800")));

            BboRecordDbn[] records = await BuildClientWithBytes(stream).GetBbo1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("MNQZ6", records[0].Symbol);
        }

        [TestMethod]
        public async Task Cbbo_GetsItsMappedSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildCbboStream(new CbboSeed { InstrumentId = 800, TsEvent = Start, TsReceived = Start }),
                Mapping(AnySymbol, Interval(Day, Day.AddDays(1), "800")));

            CbboRecordDbn[] records = await BuildClientWithBytes(stream).GetCbbo1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(AnySymbol, records[0].Symbol);
        }

        [TestMethod]
        public async Task Trades_GetsItsMappedSymbol()
        {
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTradesStream(new TradesSeed { InstrumentId = 800, TsEvent = Start, TsReceived = Start }),
                Mapping("SPX", Interval(Day, Day.AddDays(1), "800")));

            TradeRecordDbn[] records = await BuildClientWithBytes(stream).GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPX", records[0].Symbol);
        }

        [TestMethod]
        public async Task Ohlcv_TakesTheEventDate()
        {
            // a bar has no ts_recv; its ts_event is the bar's open
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildOhlcvStream(0x21, new OhlcvSeed { InstrumentId = 800, TsEvent = Start.AddDays(1) }),
                Mapping("QQQ", Interval(Day, Day.AddDays(1), "800")),
                Mapping("SPY", Interval(Day.AddDays(1), Day.AddDays(2), "800")));

            OhlcvRecordDbn[] records = await BuildClientWithBytes(stream).GetOhlcv1mAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual("SPY", records[0].Symbol);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task Statistics_EitherLayout_GetsItsMappedSymbol(bool v3Layout)
        {
            StatisticsSeed seed = new StatisticsSeed { InstrumentId = 800, TsEvent = Start, TsReceived = Start, TsRefUndefined = true };
            byte[] stream = DbnBinaryBuilder.WithSymbolMappings(
                v3Layout ? DbnBinaryBuilder.BuildStatisticsStreamV3(seed) : DbnBinaryBuilder.BuildStatisticsStream(seed),
                Mapping(AnySymbol, Interval(Day, Day.AddDays(1), "800")));

            StatisticsRecordDbn[] records = await BuildClientWithBytes(stream).GetStatisticsAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(AnySymbol, records[0].Symbol);
        }

        private static TcbboSeed Tcbbo(uint instrumentId, DateTimeOffset time)
        {
            return new TcbboSeed
            {
                InstrumentId = instrumentId,
                TsEvent = time,
                TsReceived = time,
                Price = 4.05,
                Size = 1,
            };
        }

        private static SymbolMappingSeed Mapping(string rawSymbol, params SymbolMappingIntervalSeed[] intervals)
        {
            return new SymbolMappingSeed
            {
                RawSymbol = rawSymbol,
                Intervals = intervals,
            };
        }

        private static SymbolMappingIntervalSeed Interval(DateOnly startDate, DateOnly endDate, string symbol)
        {
            return new SymbolMappingIntervalSeed
            {
                StartDate = startDate,
                EndDate = endDate,
                Symbol = symbol,
            };
        }

        private static DatabentoClient BuildClientWithBytes(byte[] bytes)
        {
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(bytes);
                    return response;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = 0,
            };
            return new DatabentoClient(options, mockTransport.Object);
        }
    }
}
