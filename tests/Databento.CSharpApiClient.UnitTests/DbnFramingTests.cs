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
        // MBO framing
        // =====================================================================================

        [TestMethod]
        public async Task GetMbo_TwoRecords_AllParsedCorrectly()
        {
            MboSeed[] seeds = new[]
            {
                new MboSeed
                {
                    PublisherId = 2, InstrumentId = 400,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(75),
                    OrderId = 100001UL, Price = 415.50, Size = 100,
                    Flags = 0x01, ChannelId = 7, Action = (byte)'A',
                    TsInDelta = 300, Sequence = 5001,
                },
                new MboSeed
                {
                    PublisherId = 2, InstrumentId = 400,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(90),
                    OrderId = 100002UL, Price = 415.25, Size = 50,
                    Flags = 0x00, ChannelId = 7, Action = (byte)'C',
                    TsInDelta = 150, Sequence = 5002,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildMboStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            MboRecordDbn[] records = await client.GetMboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both MBO records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                MboSeed s = seeds[i];
                MboRecordDbn r = records[i];
                Assert.AreEqual(s.OrderId, r.OrderId, $"records[{i}].OrderId");
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.ChannelId, r.ChannelId, $"records[{i}].ChannelId");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.AreEqual(s.TsInDelta, r.TsInDelta, $"records[{i}].TsInDelta");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // MBP-10 framing
        // =====================================================================================

        [TestMethod]
        public async Task GetMbp10_TwoRecords_AllParsedCorrectly()
        {
            LevelSeed[] levels1 = new LevelSeed[2]
            {
                new LevelSeed { BidPrice = 500.49, AskPrice = 500.51, BidSize = 100, AskSize = 200, BidCount = 3, AskCount = 5 },
                new LevelSeed { BidPrice = 500.48, AskPrice = 500.52, BidSize = 50, AskSize = 80, BidCount = 2, AskCount = 4 },
            };

            Mbp10Seed[] seeds = new[]
            {
                new Mbp10Seed
                {
                    PublisherId = 3, InstrumentId = 500,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(120),
                    Price = 500.50, Size = 75,
                    TsInDelta = 400, Sequence = 1000,
                    Levels = levels1,
                },
                new Mbp10Seed
                {
                    PublisherId = 3, InstrumentId = 500,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(110),
                    Price = 500.75, Size = 30,
                    TsInDelta = 350, Sequence = 1001,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildMbp10Stream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            Mbp10RecordDbn[] records = await client.GetMbp10Async(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both MBP-10 records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                Mbp10Seed s = seeds[i];
                Mbp10RecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.AreEqual(s.TsInDelta, r.TsInDelta, $"records[{i}].TsInDelta");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                Assert.IsNotNull(r.Levels, $"records[{i}].Levels not null");
                Assert.AreEqual(10, r.Levels.Length, $"records[{i}].Levels.Length = 10");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }

            // Verify level data for first record
            AssertPriceEqual(levels1[0].BidPrice, records[0].Levels[0].BidPrice, "records[0].Levels[0].BidPrice");
            AssertPriceEqual(levels1[0].AskPrice, records[0].Levels[0].AskPrice, "records[0].Levels[0].AskPrice");
            Assert.AreEqual(levels1[0].BidSize, records[0].Levels[0].BidSize, "records[0].Levels[0].BidSize");
            AssertPriceEqual(levels1[1].BidPrice, records[0].Levels[1].BidPrice, "records[0].Levels[1].BidPrice");
        }

        // =====================================================================================
        // BBO framing (bbo-1s)
        // =====================================================================================

        [TestMethod]
        public async Task GetBbo1s_TwoRecords_AllParsedCorrectly()
        {
            BboSeed[] seeds = new[]
            {
                new BboSeed
                {
                    PublisherId = 4, InstrumentId = 600,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(50),
                    Price = 600.00, Size = 10,
                    TsInDelta = 200, Sequence = 2000,
                    BidPrice = 599.99, AskPrice = 600.01,
                    BidSize = 150, AskSize = 200,
                    BidCount = 4, AskCount = 6,
                },
                new BboSeed
                {
                    PublisherId = 4, InstrumentId = 600,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(60),
                    Price = 601.00, Size = 5,
                    TsInDelta = 180, Sequence = 2001,
                    BidPrice = 600.99, AskPrice = 601.01,
                    BidSize = 100, AskSize = 120,
                    BidCount = 2, AskCount = 3,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildBboStream(0xC3 /*Bbo1S*/, seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            BboRecordDbn[] records = await client.GetBbo1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both BBO records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                BboSeed s = seeds[i];
                BboRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.IsNotNull(r.Level, $"records[{i}].Level not null");
                AssertPriceEqual(s.BidPrice, r.Level.BidPrice, $"records[{i}].Level.BidPrice");
                AssertPriceEqual(s.AskPrice, r.Level.AskPrice, $"records[{i}].Level.AskPrice");
                Assert.AreEqual(s.BidSize, r.Level.BidSize, $"records[{i}].Level.BidSize");
                Assert.AreEqual(s.AskSize, r.Level.AskSize, $"records[{i}].Level.AskSize");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // TBBO framing
        // =====================================================================================

        [TestMethod]
        public async Task GetTbbo_TwoRecords_AllParsedCorrectly()
        {
            TbboSeed[] seeds = new[]
            {
                new TbboSeed
                {
                    PublisherId = 6, InstrumentId = 700,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(80),
                    Price = 700.00, Size = 25,
                    TsInDelta = 500, Sequence = 3000,
                    BidPrice = 699.99, AskPrice = 700.01,
                    BidSize = 75, AskSize = 90,
                    BidCount = 2, AskCount = 3,
                },
                new TbboSeed
                {
                    PublisherId = 6, InstrumentId = 700,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(70),
                    Price = 700.50, Size = 10,
                    TsInDelta = 450, Sequence = 3001,
                    BidPrice = 700.49, AskPrice = 700.51,
                    BidSize = 50, AskSize = 60,
                    BidCount = 1, AskCount = 2,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildTbboStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            TbboRecordDbn[] records = await client.GetTbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both TBBO records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                TbboSeed s = seeds[i];
                TbboRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.IsNotNull(r.Level, $"records[{i}].Level not null");
                AssertPriceEqual(s.BidPrice, r.Level.BidPrice, $"records[{i}].Level.BidPrice");
                AssertPriceEqual(s.AskPrice, r.Level.AskPrice, $"records[{i}].Level.AskPrice");
                Assert.AreEqual(s.BidCount, r.Level.BidCount, $"records[{i}].Level.BidCount");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // TCBBO framing
        // =====================================================================================

        [TestMethod]
        public async Task GetTcbbo_TwoRecords_AllParsedCorrectly()
        {
            TcbboSeed[] seeds = new[]
            {
                new TcbboSeed
                {
                    PublisherId = 7, InstrumentId = 800,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(90),
                    Price = 800.00, Size = 20,
                    TsInDelta = 600, Sequence = 4000,
                    BidPrice = 799.99, AskPrice = 800.01,
                    BidSize = 500, AskSize = 600,
                    BidPublisherId = 11, AskPublisherId = 12,
                },
                new TcbboSeed
                {
                    PublisherId = 7, InstrumentId = 800,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(85),
                    Price = 800.25, Size = 15,
                    TsInDelta = 550, Sequence = 4001,
                    BidPrice = 800.24, AskPrice = 800.26,
                    BidSize = 300, AskSize = 350,
                    BidPublisherId = 13, AskPublisherId = 14,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildTcbboStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            TcbboRecordDbn[] records = await client.GetTcbboAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both TCBBO records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                TcbboSeed s = seeds[i];
                TcbboRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Size, r.Size, $"records[{i}].Size");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.IsNotNull(r.Level, $"records[{i}].Level not null");
                AssertPriceEqual(s.BidPrice, r.Level.BidPrice, $"records[{i}].Level.BidPrice");
                AssertPriceEqual(s.AskPrice, r.Level.AskPrice, $"records[{i}].Level.AskPrice");
                Assert.AreEqual(s.BidPublisherId, r.Level.BidPublisherId, $"records[{i}].Level.BidPublisherId");
                Assert.AreEqual(s.AskPublisherId, r.Level.AskPublisherId, $"records[{i}].Level.AskPublisherId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // CMBP-1 framing
        // =====================================================================================

        [TestMethod]
        public async Task GetCmbp1_TwoRecords_AllParsedCorrectly()
        {
            TcbboSeed[] seeds = new[]
            {
                new TcbboSeed
                {
                    PublisherId = 8, InstrumentId = 900,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(65),
                    Price = 900.00, Size = 5,
                    TsInDelta = 700, Sequence = 5000,
                    BidPrice = 899.99, AskPrice = 900.01,
                    BidSize = 200, AskSize = 250,
                    BidPublisherId = 21, AskPublisherId = 22,
                },
                new TcbboSeed
                {
                    PublisherId = 8, InstrumentId = 900,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(55),
                    Price = 900.50, Size = 8,
                    TsInDelta = 650, Sequence = 5001,
                    BidPrice = 900.49, AskPrice = 900.51,
                    BidSize = 150, AskSize = 175,
                    BidPublisherId = 23, AskPublisherId = 24,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildCmbp1Stream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            Cmbp1RecordDbn[] records = await client.GetCmbp1Async(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both CMBP-1 records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                TcbboSeed s = seeds[i];
                Cmbp1RecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.IsNotNull(r.Level, $"records[{i}].Level not null");
                AssertPriceEqual(s.BidPrice, r.Level.BidPrice, $"records[{i}].Level.BidPrice");
                Assert.AreEqual(s.BidPublisherId, r.Level.BidPublisherId, $"records[{i}].Level.BidPublisherId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
            }
        }

        // =====================================================================================
        // OHLCV framing
        // =====================================================================================

        [TestMethod]
        public async Task GetOhlcv1s_TwoRecords_AllParsedCorrectly()
        {
            OhlcvSeed[] seeds = new[]
            {
                new OhlcvSeed
                {
                    PublisherId = 9, InstrumentId = 1000,
                    TsEvent = Start,
                    Open = 410.00, High = 412.50, Low = 409.75, Close = 411.25,
                    Volume = 5000UL,
                },
                new OhlcvSeed
                {
                    PublisherId = 9, InstrumentId = 1000,
                    TsEvent = Start.AddSeconds(1),
                    Open = 411.25, High = 413.00, Low = 411.00, Close = 412.75,
                    Volume = 3000UL,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildOhlcvStream(0x20 /*Ohlcv1S*/, seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            OhlcvRecordDbn[] records = await client.GetOhlcv1sAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both OHLCV records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                OhlcvSeed s = seeds[i];
                OhlcvRecordDbn r = records[i];
                AssertPriceEqual(s.Open, r.Open, $"records[{i}].Open");
                AssertPriceEqual(s.High, r.High, $"records[{i}].High");
                AssertPriceEqual(s.Low, r.Low, $"records[{i}].Low");
                AssertPriceEqual(s.Close, r.Close, $"records[{i}].Close");
                Assert.AreEqual(s.Volume, r.Volume, $"records[{i}].Volume");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
            }
        }

        // =====================================================================================
        // Statistics framing
        // =====================================================================================

        [TestMethod]
        public async Task GetStatistics_TwoRecords_AllParsedCorrectly()
        {
            StatisticsSeed[] seeds = new[]
            {
                new StatisticsSeed
                {
                    PublisherId = 10, InstrumentId = 2000,
                    TsEvent = Start.AddSeconds(1),
                    TsReceived = Start.AddSeconds(1).AddMicroseconds(100),
                    TsRef = Start,
                    Price = 4500.00, Quantity = 100000,
                    Sequence = 1, TsInDelta = 50,
                    StatType = 3, ChannelId = 5, UpdateAction = 1,
                },
                new StatisticsSeed
                {
                    PublisherId = 10, InstrumentId = 2000,
                    TsEvent = Start.AddSeconds(2),
                    TsReceived = Start.AddSeconds(2).AddMicroseconds(80),
                    TsRef = Start,
                    Price = 4502.50, Quantity = 125000,
                    Sequence = 2, TsInDelta = 45,
                    StatType = 9, ChannelId = 5, UpdateAction = 1,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildStatisticsStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            StatisticsRecordDbn[] records = await client.GetStatisticsAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both Statistics records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                StatisticsSeed s = seeds[i];
                StatisticsRecordDbn r = records[i];
                AssertPriceEqual(s.Price, r.Price, $"records[{i}].Price");
                Assert.AreEqual(s.Quantity, r.Quantity, $"records[{i}].Quantity");
                Assert.AreEqual(s.Sequence, r.Sequence, $"records[{i}].Sequence");
                Assert.AreEqual(s.StatType, r.StatType, $"records[{i}].StatType");
                Assert.AreEqual(s.ChannelId, r.ChannelId, $"records[{i}].ChannelId");
                Assert.AreEqual(s.UpdateAction, r.UpdateAction, $"records[{i}].UpdateAction");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // Status framing
        // =====================================================================================

        [TestMethod]
        public async Task GetStatus_TwoRecords_AllParsedCorrectly()
        {
            StatusSeed[] seeds = new[]
            {
                new StatusSeed
                {
                    PublisherId = 11, InstrumentId = 3000,
                    TsEvent = Start,
                    TsReceived = Start.AddMicroseconds(100),
                    Action = 7, Reason = 0, TradingEvent = 0,
                    IsTrading = true, IsQuoting = true, IsShortSellRestricted = false,
                },
                new StatusSeed
                {
                    PublisherId = 11, InstrumentId = 3000,
                    TsEvent = Start.AddHours(4),
                    TsReceived = Start.AddHours(4).AddMicroseconds(80),
                    Action = 8, Reason = 2, TradingEvent = 1,
                    IsTrading = false, IsQuoting = false, IsShortSellRestricted = false,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildStatusStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            StatusRecordDbn[] records = await client.GetStatusAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both Status records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                StatusSeed s = seeds[i];
                StatusRecordDbn r = records[i];
                Assert.AreEqual(s.Action, r.Action, $"records[{i}].Action");
                Assert.AreEqual(s.Reason, r.Reason, $"records[{i}].Reason");
                Assert.AreEqual(s.TradingEvent, r.TradingEvent, $"records[{i}].TradingEvent");
                Assert.AreEqual(s.IsTrading, r.IsTrading, $"records[{i}].IsTrading");
                Assert.AreEqual(s.IsQuoting, r.IsQuoting, $"records[{i}].IsQuoting");
                Assert.AreEqual(s.IsShortSellRestricted, r.IsShortSellRestricted, $"records[{i}].IsShortSellRestricted");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
                AssertUtcClose(s.TsEvent, r.TsEventUtc, $"records[{i}].TsEventUtc");
                AssertUtcClose(s.TsReceived, r.TsReceivedUtc, $"records[{i}].TsReceivedUtc");
            }
        }

        // =====================================================================================
        // Imbalance framing
        // =====================================================================================

        [TestMethod]
        public async Task GetImbalance_TwoRecords_AllParsedCorrectly()
        {
            ImbalanceSeed[] seeds = new[]
            {
                new ImbalanceSeed
                {
                    PublisherId = 12, InstrumentId = 4000,
                    TsEvent = Start.AddHours(6),
                    TsReceived = Start.AddHours(6).AddMicroseconds(50),
                    AuctionTime = Start.AddHours(6).AddMinutes(30),
                    RefPrice = 420.00,
                    ContBookClrPrice = 419.50, AuctInterestClrPrice = 420.50,
                    SsrFillingPrice = 421.00, IndMatchPrice = 420.25,
                    UpperCollar = 425.00, LowerCollar = 415.00,
                    PairedQty = 10000, TotalImbalanceQty = 2000,
                    MarketImbalanceQty = 500, UnpairedQty = 1500,
                    AuctionType = 'O', Side = 'B',
                    AuctionStatus = 0, FreezeStatus = 0, NumExtensions = 0,
                },
                new ImbalanceSeed
                {
                    PublisherId = 12, InstrumentId = 4000,
                    TsEvent = Start.AddHours(6).AddSeconds(5),
                    TsReceived = Start.AddHours(6).AddSeconds(5).AddMicroseconds(60),
                    AuctionTime = Start.AddHours(6).AddMinutes(30),
                    RefPrice = 421.00,
                    ContBookClrPrice = 420.50, AuctInterestClrPrice = 421.50,
                    SsrFillingPrice = 422.00, IndMatchPrice = 421.25,
                    UpperCollar = 426.00, LowerCollar = 416.00,
                    PairedQty = 12000, TotalImbalanceQty = 1800,
                    MarketImbalanceQty = 400, UnpairedQty = 1400,
                    AuctionType = 'O', Side = 'S',
                    AuctionStatus = 0, FreezeStatus = 0, NumExtensions = 0,
                },
            };

            byte[] streamBytes = DbnBinaryBuilder.BuildImbalanceStream(seeds);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            ImbalanceRecordDbn[] records = await client.GetImbalanceAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(2, records.Length, "Expected both Imbalance records.");

            for(int i = 0; i < seeds.Length; i++)
            {
                ImbalanceSeed s = seeds[i];
                ImbalanceRecordDbn r = records[i];
                AssertPriceEqual(s.RefPrice, r.RefPrice, $"records[{i}].RefPrice");
                AssertPriceEqual(s.IndMatchPrice, r.IndMatchPrice, $"records[{i}].IndMatchPrice");
                Assert.AreEqual(s.PairedQty, r.PairedQty, $"records[{i}].PairedQty");
                Assert.AreEqual(s.TotalImbalanceQty, r.TotalImbalanceQty, $"records[{i}].TotalImbalanceQty");
                Assert.AreEqual(s.Side, r.Side, $"records[{i}].Side");
                Assert.AreEqual(s.AuctionType, r.AuctionType, $"records[{i}].AuctionType");
                Assert.AreEqual(s.InstrumentId, r.InstrumentId, $"records[{i}].InstrumentId");
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
        public async Task GetOhlcv1s_EmptyStream_ReturnsEmptyArray()
        {
            byte[] streamBytes = DbnBinaryBuilder.BuildOhlcvStream(0x20 /*Ohlcv1S*/);
            DatabentoClient client = BuildClientWithBytes(streamBytes);

            OhlcvRecordDbn[] records = await client.GetOhlcv1sAsync(AnyDataset, AnySymbol, Start, End);

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
