using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Each record type, written by <see cref="DbnWriter"/> in each DBN version and read back by <see cref="DbnReader"/>,
    /// comes back with every field equal, and so does the metadata. The records are invented and set every field the layout
    /// carries to a value other than its default, so a field the writer drops or misplaces cannot pass unnoticed.
    /// </summary>
    [TestClass]
    public class DbnWriterRoundTripTests
    {
        private const uint InstrumentId = 15144;

        private static readonly DateTime TsEvent = new DateTime(2024, 1, 2, 14, 30, 0, 123, DateTimeKind.Utc).AddTicks(4567);

        private static readonly DateTime TsReceived = TsEvent.AddTicks(8910);

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Trades_RoundTrip_EveryFieldEqual(byte version)
        {
            TradeRecordDbn record = new TradeRecordDbn
            {
                RecordType = RType.Mbp0,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 472.65,
                Size = 300,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last | MessageInfoBits.TOB,
                Depth = 1,
                TsReceivedUtc = TsReceived,
                TsInDelta = 1734,
                Sequence = 52981,
                Symbol = "SPY",
            };
            RecordAssert.NoPropertyAtDefault(record, nameof(TradeRecordDbn.RecordType));

            AssertRoundTrip(version, DbnSchema.Trades, record, DbnWriter.WriteTrades, DbnReader.ReadTrades);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Mbp1_RoundTrip_EveryFieldEqual(byte version)
        {
            Mbp1RecordDbn record = new Mbp1RecordDbn
            {
                RecordType = RType.Mbp1,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 472.64,
                Size = 200,
                Action = OrderBookAction.Add,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                Depth = 2,
                TsReceivedUtc = TsReceived,
                TsInDelta = 811,
                Sequence = 7731,
                BidPrice = 472.63,
                AskPrice = 472.66,
                BidSize = 500,
                AskSize = 700,
                BidCount = 3,
                AskCount = 4,
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Mbp1, record, DbnWriter.WriteMbp1, DbnReader.ReadMbp1);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Tbbo_RoundTrip_EveryFieldEqual(byte version)
        {
            TbboRecordDbn record = new TbboRecordDbn
            {
                RecordType = RType.Mbp1,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 472.65,
                Size = 100,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                Depth = 3,
                TsReceivedUtc = TsReceived,
                TsInDelta = 905,
                Sequence = 8812,
                Level = Mbp1Level(0),
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Tbbo, record, DbnWriter.WriteTbbo, DbnReader.ReadTbbo);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Bbo_RoundTrip_EveryFieldEqual(byte version)
        {
            BboRecordDbn record = new BboRecordDbn
            {
                RecordType = RType.Bbo1S,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 472.61,
                Size = 50,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                Sequence = 9911,
                Level = Mbp1Level(0),
                Symbol = "SPY",
            };

            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Bbo1S, record, DbnWriter.WriteBbo, DbnReader.ReadBbo);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Cbbo_RoundTrip_EveryFieldEqual(byte version)
        {
            CbboRecordDbn record = new CbboRecordDbn
            {
                RecordType = RType.Cbbo1M,
                PublisherId = 30,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                Side = TradeAggressor.Seller,
                Price = 12.35,
                Size = 4,
                Flags = MessageInfoBits.Last,
                BidPrice = 12.3,
                AskPrice = 12.4,
                BidSize = 11,
                AskSize = 13,
                BidPublisherId = 39,
                AskPublisherId = 40,
                Symbol = "SPY",
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Cbbo1M, record, DbnWriter.WriteCbbo, DbnReader.ReadCbbo);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Tcbbo_RoundTrip_EveryFieldEqual(byte version)
        {
            TcbboRecordDbn record = new TcbboRecordDbn
            {
                RecordType = RType.Tcbbo,
                PublisherId = 30,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 12.35,
                Size = 6,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                TsInDelta = 377,
                Level = CbboLevel(),
                Symbol = "SPY",
            };

            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Tcbbo, record, DbnWriter.WriteTcbbo, DbnReader.ReadTcbbo);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Cmbp1_RoundTrip_EveryFieldEqual(byte version)
        {
            Cmbp1RecordDbn record = new Cmbp1RecordDbn
            {
                RecordType = RType.Cmbp1,
                PublisherId = 30,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 12.3,
                Size = 9,
                Action = OrderBookAction.Modify,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                TsInDelta = 412,
                Level = CbboLevel(),
            };

            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Cmbp1, record, DbnWriter.WriteCmbp1, DbnReader.ReadCmbp1);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Mbo_RoundTrip_EveryFieldEqual(byte version)
        {
            MboRecordDbn record = new MboRecordDbn
            {
                RecordType = RType.Mbo,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                OrderId = 6_421_300_117_993UL,
                Price = 472.62,
                Size = 25,
                Flags = MessageInfoBits.Last | MessageInfoBits.MBP,
                ChannelId = 7,
                Action = OrderBookAction.Cancel,
                Side = TradeAggressor.Buyer,
                TsReceivedUtc = TsReceived,
                TsInDelta = 655,
                Sequence = 120344,
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Mbo, record, DbnWriter.WriteMbo, DbnReader.ReadMbo);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Mbp10_RoundTrip_EveryFieldEqual(byte version)
        {
            Mbp10RecordDbn record = new Mbp10RecordDbn
            {
                RecordType = RType.Mbp10,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Price = 472.6,
                Size = 75,
                Action = OrderBookAction.Add,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                Depth = 9,
                TsReceivedUtc = TsReceived,
                TsInDelta = 230,
                Sequence = 44019,
                Levels = Enumerable.Range(0, 10).Select(Mbp1Level).ToArray(),
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Mbp10, record, DbnWriter.WriteMbp10, DbnReader.ReadMbp10);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Ohlcv_RoundTrip_EveryFieldEqual(byte version)
        {
            OhlcvRecordDbn record = new OhlcvRecordDbn
            {
                RecordType = RType.Ohlcv1M,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                Open = 472.5,
                High = 473.25,
                Low = 471.75,
                Close = 472.0,
                Volume = 18_245_773UL,
                Symbol = "SPY",
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Ohlcv1M, record, DbnWriter.WriteOhlcv, DbnReader.ReadOhlcv);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Status_RoundTrip_EveryFieldEqual(byte version)
        {
            StatusRecordDbn record = new StatusRecordDbn
            {
                RecordType = RType.Status,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                Action = 7,
                Reason = 12,
                TradingEvent = 3,
                IsTrading = true,
                IsQuoting = true,
                IsShortSellRestricted = true,
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Status, record, DbnWriter.WriteStatus, DbnReader.ReadStatus);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Imbalance_RoundTrip_EveryFieldEqual(byte version)
        {
            ImbalanceRecordDbn record = new ImbalanceRecordDbn
            {
                RecordType = RType.Imbalance,
                PublisherId = 2,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                RefPrice = 472.5,
                AuctionTime = new DateTime(2024, 1, 2, 21, 0, 0, DateTimeKind.Utc),
                ContBookClrPrice = 472.55,
                AuctInterestClrPrice = 472.45,
                SsrFillingPrice = 472.35,
                IndMatchPrice = 472.48,
                UpperCollar = 496.13,
                LowerCollar = 448.88,
                PairedQty = 1_200_000,
                TotalImbalanceQty = 350_000,
                MarketImbalanceQty = 120_000,
                UnpairedQty = 80_000,
                AuctionType = 'C',
                Side = 'B',
                AuctionStatus = 1,
                FreezeStatus = 2,
                NumExtensions = 3,
                UnpairedSide = 'A',
                SignificantImbalance = 'L',
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Imbalance, record, DbnWriter.WriteImbalance, DbnReader.ReadImbalance);
        }

        [DataTestMethod]
        [DataRow((byte)1, 1_234_567L)]
        [DataRow((byte)2, 1_234_567L)]
        [DataRow((byte)3, 5_000_000_000L)]
        public void Statistics_RoundTrip_EveryFieldEqual(byte version, long quantity)
        {
            StatisticsRecordDbn record = new StatisticsRecordDbn
            {
                RecordType = RType.Statistics,
                PublisherId = 20,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                TsRefUtc = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Price = 4718.25,
                Quantity = quantity,
                Sequence = 31337,
                TsInDelta = 190,
                StatType = StatTypes.OpenInterest,
                ChannelId = 4,
                UpdateAction = 1,
                StatFlags = MessageInfoBits.BadTimestampReceived,
                Symbol = "SPY",
            };
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(version, DbnSchema.Statistics, record, DbnWriter.WriteStatistics, DbnReader.ReadStatistics);
        }

        [TestMethod]
        public void DefinitionsV1_RoundTrip_EveryFieldEqual()
        {
            DefinitionRecordDbn record = Definition();
            record.RawSymbol = "SPXW  240102C04700000";
            record.Asset = "SPXW";
            record.RawInstrumentId = 4_000_000_000UL;
            record.TradingReferencePrice = 4712.5;
            record.TradingReferenceDate = 19723;
            record.MdSecurityTradingStatus = 17;
            record.SettlementPriceType = 3;

            // DBN v1 has no legs: the reader gives a v1 record no leg price or delta (NaN) and leaves the other leg fields at their defaults
            record.LegPrice = double.NaN;
            record.LegDelta = double.NaN;
            RecordAssert.NoPropertyAtDefault(record,
                nameof(DefinitionRecordDbn.LegCount), nameof(DefinitionRecordDbn.LegIndex), nameof(DefinitionRecordDbn.LegInstrumentId),
                nameof(DefinitionRecordDbn.LegRawSymbol), nameof(DefinitionRecordDbn.LegInstrumentClass), nameof(DefinitionRecordDbn.LegSide),
                nameof(DefinitionRecordDbn.LegRatioPriceNumerator), nameof(DefinitionRecordDbn.LegRatioPriceDenominator),
                nameof(DefinitionRecordDbn.LegRatioQtyNumerator), nameof(DefinitionRecordDbn.LegRatioQtyDenominator),
                nameof(DefinitionRecordDbn.LegUnderlyingId));

            AssertRoundTrip(1, DbnSchema.Definition, record, DbnWriter.WriteDefinitions, DbnReader.ReadDefinitions);
        }

        [TestMethod]
        public void DefinitionsV3_RoundTrip_EveryFieldEqual()
        {
            DefinitionRecordDbn record = Definition();
            record.RawSymbol = "ESH4-ESM4 calendar spread with a raw symbol longer than v1 holds";
            record.Asset = "ESMINIAB";
            record.RawInstrumentId = 5_000_000_000UL;

            // DBN v3 has no trading reference fields, security trading status or settlement price type: the reader gives them their undefined values
            record.TradingReferencePrice = double.NaN;
            record.TradingReferenceDate = ushort.MaxValue;
            record.MdSecurityTradingStatus = byte.MaxValue;
            record.SettlementPriceType = byte.MaxValue;
            record.LegCount = 2;
            record.LegIndex = 1;
            record.LegInstrumentId = 42_005_347;
            record.LegRawSymbol = "ESM4";
            record.LegInstrumentClass = 'F';
            record.LegSide = 'B';
            record.LegPrice = 4801.5;
            record.LegDelta = 0.5;
            record.LegRatioPriceNumerator = 1;
            record.LegRatioPriceDenominator = 2;
            record.LegRatioQtyNumerator = 3;
            record.LegRatioQtyDenominator = 4;
            record.LegUnderlyingId = 42_005_000;
            RecordAssert.NoPropertyAtDefault(record);

            AssertRoundTrip(3, DbnSchema.Definition, record, DbnWriter.WriteDefinitions, DbnReader.ReadDefinitions);
        }

        [DataTestMethod]
        [DataRow((byte)1)]
        [DataRow((byte)2)]
        [DataRow((byte)3)]
        public void Metadata_OpenEndedWithoutLimit_RoundTripsAsNull(byte version)
        {
            DbnMetadata metadata = Metadata(version, DbnSchema.Trades);
            metadata.End = null;
            metadata.Limit = null;

            DbnMetadata read = ReadMetadata(Write(metadata, Array.Empty<TradeRecordDbn>(), DbnWriter.WriteTrades));

            Assert.IsNull(read.End);
            Assert.IsNull(read.Limit);
        }

        /// <summary>A stream's metadata, the same in every version but the width of its symbols, which DBN v1 fixes at 22.</summary>
        internal static DbnMetadata Metadata(byte version, DbnSchema schema)
        {
            return new DbnMetadata
            {
                Version = version,
                Dataset = "XNAS.ITCH",
                Schema = schema,
                Start = new DateTimeOffset(2024, 1, 2, 14, 30, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2024, 1, 2, 21, 0, 0, TimeSpan.Zero),
                Limit = 1000,
                STypeIn = DbnSType.RawSymbol,
                STypeOut = DbnSType.InstrumentId,
                TsOut = false,
                SymbolCStrLen = version == 1 ? (ushort)22 : (ushort)71,
                SchemaDefinition = Array.Empty<byte>(),
                Symbols = new List<string> { "SPY", "QQQ" },
                Partial = new List<string> { "QQQ" },
                NotFound = new List<string> { "XYZ" },
                Mappings = new List<DbnSymbolMapping>
                {
                    new DbnSymbolMapping
                    {
                        RawSymbol = "SPY",
                        Intervals = new List<DbnMappingInterval>
                        {
                            new DbnMappingInterval { StartDate = 20240102, EndDate = 20240103, Symbol = InstrumentId.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                        },
                    },
                },
            };
        }

        internal static byte[] Write<T>(DbnMetadata metadata, IEnumerable<T> records, Action<Stream, DbnMetadata, IEnumerable<T>> write)
        {
            using MemoryStream stream = new MemoryStream();
            write(stream, metadata, records);
            return stream.ToArray();
        }

        internal static DbnMetadata ReadMetadata(byte[] dbn)
        {
            using MemoryStream stream = new MemoryStream(dbn);
            using BinaryReader reader = new BinaryReader(stream);
            return DbnMetadata.FromBinaryReader(reader);
        }

        private static void AssertRoundTrip<T>(
            byte version,
            DbnSchema schema,
            T record,
            Action<Stream, DbnMetadata, IEnumerable<T>> write,
            Func<Stream, IEnumerable<T>> read)
        {
            DbnMetadata metadata = Metadata(version, schema);
            byte[] dbn = Write(metadata, new[] { record, record }, write);

            Assert.AreEqual(version, dbn[3], "version byte");
            RecordAssert.AllPropertiesEqual(metadata, ReadMetadata(dbn), "metadata");
            using MemoryStream stream = new MemoryStream(dbn);
            List<T> records = read(stream).ToList();
            Assert.AreEqual(2, records.Count);
            RecordAssert.AllPropertiesEqual(record, records[0]);
            RecordAssert.AllPropertiesEqual(record, records[1]);
        }

        private static Mbp1LevelDbn Mbp1Level(int index)
        {
            return new Mbp1LevelDbn
            {
                // from whole nanos, as the reader makes its prices, so the expected price is the one a fixed-point price reads as
                BidPrice = (472_500_000_000L - index * 10_000_000L) / 1e9,
                AskPrice = (472_700_000_000L + index * 10_000_000L) / 1e9,
                BidSize = (uint)(100 + index),
                AskSize = (uint)(200 + index),
                BidCount = (uint)(10 + index),
                AskCount = (uint)(20 + index),
            };
        }

        private static CbboLevelDbn CbboLevel()
        {
            return new CbboLevelDbn
            {
                BidPrice = 12.3,
                AskPrice = 12.4,
                BidSize = 15,
                AskSize = 17,
                BidPublisherId = 41,
                AskPublisherId = 43,
            };
        }

        // every field the two layouts share, set; the caller sets the ones that differ
        private static DefinitionRecordDbn Definition()
        {
            return new DefinitionRecordDbn
            {
                RecordType = RType.InstrumentDef,
                PublisherId = 20,
                InstrumentId = InstrumentId,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                MinPriceIncrement = 0.05,
                DisplayFactor = 1.0,
                Expiration = new DateTime(2024, 1, 2, 21, 15, 0, DateTimeKind.Utc),
                Activation = new DateTime(2023, 12, 1, 14, 30, 0, DateTimeKind.Utc),
                HighLimitPrice = 5000.25,
                LowLimitPrice = 4000.75,
                MaxPriceVariation = 50.5,
                UnitOfMeasureQty = 100.0,
                StrikePrice = 4700.0,
                Exchange = "XCBO",
                Cfi = "OCEICS",
                SecurityType = "OPT",
                InstrumentClass = 'C',
                Action = 'A',
                MinPriceIncrementAmount = 5.0,
                PriceRatio = 1.5,
                InstrumentAttributeValue = 270339,
                UnderlyingId = 477,
                MarketDepthImplied = 2,
                MarketDepth = 10,
                MarketSegmentId = 54,
                MaxTradeVolume = 3000,
                MinLotSize = 1,
                MinLotSizeBlock = 50,
                MinLotSizeRoundLot = 100,
                MinTradeVolume = 2,
                ContractMultiplier = 100,
                DecayQuantity = 6,
                OriginalContractSize = 7,
                ApplId = 311,
                MaturityYear = 2024,
                DecayStartDate = 19720,
                ChannelId = 9,
                Currency = "USD",
                SettlementCurrency = "EUR",
                SecuritySubType = "CAL",
                Group = "SX",
                UnitOfMeasure = "IPNT",
                Underlying = "SPX",
                StrikePriceCurrency = "USD",
                MatchAlgorithm = 'F',
                MainFraction = 4,
                PriceDisplayFormat = 2,
                SubFraction = 8,
                UnderlyingProduct = 5,
                MaturityMonth = 1,
                MaturityDay = 2,
                MaturityWeek = 1,
                UserDefinedInstrument = 'N',
                ContractMultiplierUnit = 1,
                FlowScheduleType = 2,
                TickRule = 15,
            };
        }
    }
}
