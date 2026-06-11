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
    /// Deterministic framing tests: verifies that all N records in a synthetic DBN stream are
    /// correctly parsed without off-by-one positioning errors.  The key regression being guarded
    /// is reading <c>RecordLength</c> bytes (total) instead of <c>RecordLength - 16</c> bytes
    /// (body only), which caused every record after the first to be silently corrupted.
    /// </summary>
    [TestClass]
    public class DbnFramingTests
    {
        private const string AnyDataset = "XNAS.ITCH";
        private const string AnySymbol = "SPY";

        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset End = Start.AddHours(1);

        // =====================================================================================
        // CBBO framing
        // =====================================================================================

        [TestMethod]
        public async Task GetCbbo1s_ThreeRecords_AllParsedCorrectly()
        {
            CbboSeed[] seeds = new[]
            {
                new CbboSeed
                {
                    PublisherId = 1, InstrumentId = 101,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(50),
                    Price = 4.50, Size = 10,
                    BidPrice = 4.49, AskPrice = 4.51,
                    BidSize = 5, AskSize = 8,
                    BidPublisherId = 1, AskPublisherId = 2,
                },
                new CbboSeed
                {
                    PublisherId = 1, InstrumentId = 102,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(30),
                    Price = 5.25, Size = 20,
                    BidPrice = 5.24, AskPrice = 5.26,
                    BidSize = 15, AskSize = 12,
                    BidPublisherId = 3, AskPublisherId = 4,
                },
                new CbboSeed
                {
                    PublisherId = 2, InstrumentId = 103,
                    TsEvent = Start.AddSeconds(3),
                    TsReceived = Start.AddSeconds(3).AddMicroseconds(10),
                    Price = 100.00, Size = 1,
                    BidPrice = 99.99, AskPrice = 100.01,
                    BidSize = 100, AskSize = 200,
                    BidPublisherId = 5, AskPublisherId = 6,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildCbboStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            CbboRecordDbn[] records = await client.GetCbbo1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(3, records.Length, "Expected all 3 records in the stream.");

            for(int i = 0; i < seeds.Length; i++)
            {
                CbboSeed s = seeds[i];
                CbboRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                AssertPriceEqual(s.BidPrice, r.BidPrice, $"records[{i}].BidPrice");
                AssertPriceEqual(s.AskPrice, r.AskPrice, $"records[{i}].AskPrice");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.BidSize, r.BidSize, $"records[{i}].BidSize");
                Assert.AreEqual(s.AskSize, r.AskSize, $"records[{i}].AskSize");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                Assert.AreEqual(s.PublisherId, r.PublisherId, $"records[{i}].PublisherId");
                Assert.AreEqual(s.BidPublisherId, r.BidPublisherId, $"records[{i}].BidPublisherId");
                Assert.AreEqual(s.AskPublisherId, r.AskPublisherId, $"records[{i}].AskPublisherId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // Trades framing
        // =====================================================================================

        [TestMethod]
        public async Task GetTrades_ThreeRecords_AllParsedCorrectly()
        {
            TradesSeed[] seeds = new[]
            {
                new TradesSeed
                {
                    PublisherId = 10, InstrumentId = 200,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(100),
                    Price = 410.50, Size = 300,
                    TsInDelta = 512, Sequence = 1,
                },
                new TradesSeed
                {
                    PublisherId = 10, InstrumentId = 200,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(80),
                    Price = 410.75, Size = 100,
                    TsInDelta = 256, Sequence = 2,
                },
                new TradesSeed
                {
                    PublisherId = 10, InstrumentId = 201,
                    TsEvent = Start.AddSeconds(3),
                    TsReceived = Start.AddSeconds(3).AddMicroseconds(60),
                    Price = 0.0099, Size = 1,
                    TsInDelta = 1024, Sequence = 3,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildTradesStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            TradeRecordDbn[] records = await client.GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(3, records.Length, "Expected all 3 trade records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                TradesSeed s = seeds[i];
                TradeRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.AreEqual(s.TsInDelta, r.TsInDelta, $"records[{i}].TsInDelta");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // MBP-1 framing
        // =====================================================================================

        [TestMethod]
        public async Task GetMbp1_TwoRecords_AllParsedCorrectly()
        {
            Mbp1Seed[] seeds = new[]
            {
                new Mbp1Seed
                {
                    PublisherId = 5, InstrumentId = 300,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(200),
                    Price = 400.50, Size = 50,
                    TsInDelta = 100, Sequence = 99,
                    BidPrice = 400.49, AskPrice = 400.51,
                    BidSize = 200, AskSize = 150,
                    BidCount = 5, AskCount = 7,
                },
                new Mbp1Seed
                {
                    PublisherId = 5, InstrumentId = 300,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(150),
                    Price = 401.00, Size = 25,
                    TsInDelta = 200, Sequence = 100,
                    BidPrice = 400.99, AskPrice = 401.01,
                    BidSize = 100, AskSize = 120,
                    BidCount = 3, AskCount = 4,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildMbp1Stream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            Mbp1RecordDbn[] records = await client.GetMbp1Async(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both MBP-1 records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                Mbp1Seed s = seeds[i];
                Mbp1RecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                AssertPriceEqual(s.BidPrice, r.BidPrice, $"records[{i}].BidPrice");
                AssertPriceEqual(s.AskPrice, r.AskPrice, $"records[{i}].AskPrice");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.BidSize, r.BidSize, $"records[{i}].BidSize");
                Assert.AreEqual(s.AskSize, r.AskSize, $"records[{i}].AskSize");
                Assert.AreEqual(s.BidCount, r.BidCount, $"records[{i}].BidCount");
                Assert.AreEqual(s.AskCount, r.AskCount, $"records[{i}].AskCount");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // Edge cases
        // =====================================================================================

        [TestMethod]
        public async Task GetTrades_EmptyStream_ReturnsEmptyArray()
        {
            byte[] streamBytes = DbnBinaryBuilder.BuildTradesStream();
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            TradeRecordDbn[] records = await client.GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.IsNotNull(records);
            Assert.AreEqual(0, records.Length);
        }

        [TestMethod]
        public async Task GetCbbo1s_OneRecord_RoundTripPricePreserved()
        {
            // Verify the nano-price round-trip: double -> nano (write) -> double (read) is lossless
            // for prices with up to 2 decimal places (common for options).
            double expectedBid = 6475.50;
            double expectedAsk = 6476.00;

            byte[] streamBytes = DbnBinaryBuilder.BuildCbboStream(new CbboSeed
            {
                PublisherId = 1,
                InstrumentId = 1,
                TsEvent = Start,
                TsReceived = Start,
                Price = 6475.75,
                Size = 5,
                BidPrice = expectedBid,
                AskPrice = expectedAsk,
                BidSize = 2,
                AskSize = 3,
            });
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            CbboRecordDbn[] records = await client.GetCbbo1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(1, records.Length);
            AssertPriceEqual(expectedBid, records[0].BidPrice, "BidPrice round-trip");
            AssertPriceEqual(expectedAsk, records[0].AskPrice, "AskPrice round-trip");
        }

        // =====================================================================================
        // Helpers
        // =====================================================================================

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

        private static void AssertPriceEqual(double expected, double actual, string label)
        {
            // Prices are stored as int64 nano-dollars: 1 nano = 1e-9, so tolerance = 1e-7 covers
            // any sub-cent rounding while being far stricter than floating-point noise.
            Assert.AreEqual(expected, actual, 1e-7, label);
        }

        private static void AssertUtcClose(DateTimeOffset expected, DateTime actual, string label)
        {
            // DBN stores ns precision; DateTime has 100 ns tick resolution.
            // Allow 1 microsecond (10 ticks) tolerance for sub-tick ns truncation.
            DateTime expectedUtc = expected.UtcDateTime;
            double deltaUs = Math.Abs((expectedUtc - actual).TotalMilliseconds * 1000.0);
            Assert.IsTrue(deltaUs < 1.0, $"{label}: expected {expectedUtc:O}, got {actual:O}, delta {deltaUs:F3} µs");
        }
    }
}
