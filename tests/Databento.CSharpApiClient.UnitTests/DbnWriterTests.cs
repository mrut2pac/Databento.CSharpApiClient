using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// <see cref="DbnWriter"/> writes the DBN its reader reads: the reader's own test streams, read and written again, come
    /// out byte for byte; the metadata is laid out as each DBN version lays it out; and what DBN cannot hold is refused.
    /// </summary>
    [TestClass]
    public class DbnWriterTests
    {
        private static readonly DateTimeOffset TsEvent = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero).AddTicks(1_234_567);

        private static readonly DateTimeOffset TsReceived = TsEvent.AddTicks(8_910);

        private static readonly DateOnly Day = new DateOnly(2022, 5, 16);

        // ---- the reader's test streams, written again byte for byte ----

        [TestMethod]
        public void WriteTrades_ReadersV1Stream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTradesStreamV1(Trades(800, 101.25), Trades(801, 99.5)),
                Mapping("SPY", "800"),
                Mapping("QQQ", "801"));

            AssertWrittenAgainByteForByte(original, DbnReader.ReadTrades, DbnWriter.WriteTrades);
        }

        [TestMethod]
        public void WriteTrades_ReadersV2StreamWithWideSymbols_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.WithWideSymbolMappings(
                DbnBinaryBuilder.BuildTradesStream(Trades(800, 101.25), Trades(801, 99.5)),
                Mapping("SPXW  261016C06600000", "800"));

            AssertWrittenAgainByteForByte(original, DbnReader.ReadTrades, DbnWriter.WriteTrades);
        }

        [TestMethod]
        public void WriteCbbo_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildCbboStream(
                new CbboSeed { PublisherId = 30, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Price = 12.35, Size = 4, BidPrice = 12.3, AskPrice = 12.4, BidSize = 11, AskSize = 13, BidPublisherId = 39, AskPublisherId = 40 },
                new CbboSeed { PublisherId = 30, InstrumentId = 801, TsEvent = TsEvent, TsReceived = TsReceived, Price = 0.05, Size = 1, BidPrice = 0.05, AskPrice = 0.1, BidSize = 1, AskSize = 2, BidPublisherId = 41, AskPublisherId = 42 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadCbbo, DbnWriter.WriteCbbo);
        }

        [TestMethod]
        public void WriteMbp1_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildMbp1Stream(
                new Mbp1Seed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Price = 101.25, Size = 5, TsInDelta = 17, Sequence = 99, BidPrice = 101.2, AskPrice = 101.3, BidSize = 7, AskSize = 9, BidCount = 2, AskCount = 3 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadMbp1, DbnWriter.WriteMbp1);
        }

        [TestMethod]
        public void WriteMbo_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildMboStream(
                new MboSeed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, OrderId = 6_421_300_117_993UL, Price = 101.25, Size = 5, Flags = 0x80, ChannelId = 7, Action = (byte)'A', TsInDelta = 17, Sequence = 99 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadMbo, DbnWriter.WriteMbo);
        }

        [TestMethod]
        public void WriteMbp10_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildMbp10Stream(
                new Mbp10Seed
                {
                    PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Price = 101.25, Size = 5, TsInDelta = 17, Sequence = 99,
                    Levels = Enumerable.Range(0, 10).Select(i => new LevelSeed { BidPrice = 101 - i, AskPrice = 102 + i, BidSize = (uint)(10 + i), AskSize = (uint)(20 + i), BidCount = (uint)i, AskCount = (uint)(i + 1) }).ToArray(),
                });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadMbp10, DbnWriter.WriteMbp10);
        }

        [TestMethod]
        public void WriteBbo_ReadersStream_IsWrittenAgainByteForByte()
        {
            // the builder writes ts_in_delta into the field bbo reserves, so it stays 0 here
            byte[] original = DbnBinaryBuilder.BuildBboStream(
                (byte)RType.Bbo1M,
                new BboSeed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Price = 101.25, Size = 5, Sequence = 99, BidPrice = 101.2, AskPrice = 101.3, BidSize = 7, AskSize = 9, BidCount = 2, AskCount = 3 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadBbo, DbnWriter.WriteBbo);
        }

        [TestMethod]
        public void WriteTbbo_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildTbboStream(
                new TbboSeed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Price = 101.25, Size = 5, TsInDelta = 17, Sequence = 99, BidPrice = 101.2, AskPrice = 101.3, BidSize = 7, AskSize = 9, BidCount = 2, AskCount = 3 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadTbbo, DbnWriter.WriteTbbo);
        }

        [TestMethod]
        public void WriteTcbbo_ReadersStream_IsWrittenAgainByteForByte()
        {
            // the builder writes sequence into the field tcbbo reserves, so it stays 0 here
            byte[] original = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800), Tcbbo(801)),
                Mapping("SPXW  261016C06600000", "800"),
                Mapping("SPXW  261016P06600000", "801"));

            AssertWrittenAgainByteForByte(original, DbnReader.ReadTcbbo, DbnWriter.WriteTcbbo);
        }

        [TestMethod]
        public void WriteCmbp1_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildCmbp1Stream(Tcbbo(800), Tcbbo(801));

            AssertWrittenAgainByteForByte(original, DbnReader.ReadCmbp1, DbnWriter.WriteCmbp1);
        }

        [DataTestMethod]
        [DataRow((byte)RType.Ohlcv1S)]
        [DataRow((byte)RType.Ohlcv1M)]
        [DataRow((byte)RType.Ohlcv1H)]
        [DataRow((byte)RType.Ohlcv1D)]
        [DataRow((byte)RType.OhlcvEod)]
        public void WriteOhlcv_ReadersStreamOfEveryBarInterval_IsWrittenAgainByteForByte(byte rtype)
        {
            byte[] original = DbnBinaryBuilder.BuildOhlcvStream(
                rtype,
                new OhlcvSeed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, Open = 101.25, High = 102.5, Low = 100.75, Close = 101.0, Volume = 18_245_773UL });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadOhlcv, DbnWriter.WriteOhlcv);
        }

        // The builder's DBN v2 statistics stream is not here: its 60-byte record is no DBN layout (v1 and v2 statistics records
        // are 64 bytes), so the writer, which writes DBN, cannot match it.
        [TestMethod]
        public void WriteStatistics_ReadersV3Stream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildStatisticsStreamV3(
                new StatisticsSeed { PublisherId = 20, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, TsRef = TsEvent.AddHours(-13), Price = 4718.25, Quantity = 5_000_000_000L, Sequence = 99, TsInDelta = 17, StatType = 9, ChannelId = 4, UpdateAction = 1, StatFlags = 8 },
                new StatisticsSeed { PublisherId = 20, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, TsRefUndefined = true, Price = 4718.5, Quantity = long.MaxValue, Sequence = 100, TsInDelta = 18, StatType = 3, ChannelId = 4, UpdateAction = 1 });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadStatistics, DbnWriter.WriteStatistics);
        }

        [TestMethod]
        public void WriteStatus_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildStatusStream(
                new StatusSeed { PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, Action = 7, Reason = 12, TradingEvent = 3, IsTrading = true, IsQuoting = false, IsShortSellRestricted = true });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadStatus, DbnWriter.WriteStatus);
        }

        [TestMethod]
        public void WriteImbalance_ReadersStream_IsWrittenAgainByteForByte()
        {
            byte[] original = DbnBinaryBuilder.BuildImbalanceStream(
                new ImbalanceSeed
                {
                    PublisherId = 2, InstrumentId = 800, TsEvent = TsEvent, TsReceived = TsReceived, RefPrice = 101.25, AuctionTime = TsEvent.AddHours(7),
                    ContBookClrPrice = 101.3, AuctInterestClrPrice = 101.2, SsrFillingPrice = 101.1, IndMatchPrice = 101.15, UpperCollar = 106.25, LowerCollar = 96.25,
                    PairedQty = 1_200_000, TotalImbalanceQty = 350_000, MarketImbalanceQty = 120_000, UnpairedQty = 80_000,
                    AuctionType = 'C', Side = 'B', AuctionStatus = 1, FreezeStatus = 2, NumExtensions = 3,
                });

            AssertWrittenAgainByteForByte(original, DbnReader.ReadImbalance, DbnWriter.WriteImbalance);
        }

        [DataTestMethod]
        [DataRow((byte)1, DefinitionRecordDbnTests.SpxwCallV1Hex)]
        [DataRow((byte)3, DefinitionRecordDbnTests.EsPutV3Hex)]
        public void DefinitionWriteToBytes_ReadersRecords_AreWrittenAgainByteForByte(byte version, string recordHex)
        {
            DefinitionRecordDbn record;
            using(MemoryStream input = new MemoryStream(Convert.FromHexString(recordHex)))
            using(BinaryReader reader = new BinaryReader(input))
            {
                record = DefinitionRecordDbn.ReadFromBytes(DbnRecordHeader.ReadFromBytes(reader), reader);
            }

            using MemoryStream output = new MemoryStream();
            using(BinaryWriter writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                record.WriteToBytes(writer, version);
            }

            Assert.AreEqual(recordHex, Convert.ToHexString(output.ToArray()));
        }

        [TestMethod]
        public void WriteDefinitions_V1Stream_IsTheMetadataThenTheRecordsBytes()
        {
            DefinitionRecordDbn record;
            using(MemoryStream input = new MemoryStream(Convert.FromHexString(DefinitionRecordDbnTests.SpxwCallV1Hex)))
            using(BinaryReader reader = new BinaryReader(input))
            {
                record = DefinitionRecordDbn.ReadFromBytes(DbnRecordHeader.ReadFromBytes(reader), reader);
            }

            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(1, DbnSchema.Definition);
            byte[] metadataOnly = DbnWriterRoundTripTests.Write(metadata, Array.Empty<DefinitionRecordDbn>(), DbnWriter.WriteDefinitions);
            byte[] dbn = DbnWriterRoundTripTests.Write(metadata, new[] { record }, DbnWriter.WriteDefinitions);

            Assert.AreEqual(Convert.ToHexString(metadataOnly) + DefinitionRecordDbnTests.SpxwCallV1Hex, Convert.ToHexString(dbn));
        }

        // ---- the metadata block, laid out per version ----

        [TestMethod]
        public void WriteTrades_V1MetadataWithoutRecords_IsLaidOutAsDbnV1()
        {
            // hand-laid DBN v1 metadata: record_count (u64::MAX) after limit, no symbol_cstr_len, 47 reserved bytes, no padding
            string expected =
                "44424E01" + "78000000"                     // 'DBN', version 1, metadata_length 120
                + "584E41532E4954434800000000000000"       // dataset "XNAS.ITCH", 16 bytes
                + "0400"                                   // schema trades (4)
                + "00102955258EA617"                       // start 2024-01-02 14:30:00 UTC, 1704205800000000000 ns
                + "FFFFFFFFFFFFFFFF"                       // end: open-ended, u64::MAX as Databento's encoder writes it
                + "0000000000000000"                       // limit: none
                + "FFFFFFFFFFFFFFFF"                       // record_count: unknown (v1 only)
                + "01" + "00" + "00"                       // stype_in raw_symbol, stype_out instrument_id, ts_out false
                + new string('0', 47 * 2)                  // reserved
                + "00000000" + "00000000" + "00000000" + "00000000" + "00000000"; // schema definition, symbols, partial, not_found, mappings

            DbnMetadata metadata = new DbnMetadata
            {
                Version = 1,
                Dataset = "XNAS.ITCH",
                Schema = DbnSchema.Trades,
                Start = new DateTimeOffset(2024, 1, 2, 14, 30, 0, TimeSpan.Zero),
                STypeIn = DbnSType.RawSymbol,
                STypeOut = DbnSType.InstrumentId,
            };

            byte[] dbn = DbnWriterRoundTripTests.Write(metadata, Array.Empty<TradeRecordDbn>(), DbnWriter.WriteTrades);

            Assert.AreEqual(expected, Convert.ToHexString(dbn));
        }

        [DataTestMethod]
        [DataRow((byte)1, 22, 142)] // 100 fixed + 4 schema definition + (4 + 22) symbols + 4 partial + 4 not_found + 4 mappings
        [DataRow((byte)2, 71, 191)] // as v1 with 71-byte symbols; v2 is not padded
        [DataRow((byte)3, 71, 192)] // 191, padded to a multiple of 8 as DBN v3 is
        public void WriteTrades_OneSymbol_MetadataLengthIsAsTheVersionLaysItOut(byte version, int symbolCStrLen, int metadataLength)
        {
            DbnMetadata metadata = new DbnMetadata
            {
                Version = version,
                Dataset = "XNAS.ITCH",
                Schema = DbnSchema.Trades,
                Start = new DateTimeOffset(2024, 1, 2, 14, 30, 0, TimeSpan.Zero),
                STypeIn = DbnSType.RawSymbol,
                STypeOut = DbnSType.InstrumentId,
                SymbolCStrLen = (ushort)symbolCStrLen,
                Symbols = new List<string> { "SPY" },
            };

            byte[] dbn = DbnWriterRoundTripTests.Write(metadata, Array.Empty<TradeRecordDbn>(), DbnWriter.WriteTrades);

            Assert.AreEqual(metadataLength, BitConverter.ToInt32(dbn, 4));
            Assert.AreEqual(8 + metadataLength, dbn.Length);
            CollectionAssert.AreEqual(new List<string> { "SPY" }, DbnWriterRoundTripTests.ReadMetadata(dbn).Symbols);
        }

        [DataTestMethod]
        [DataRow((byte)1, 22)]
        [DataRow((byte)2, 71)]
        [DataRow((byte)3, 71)]
        public void WriteTrades_SymbolWidthNotSet_TakesTheVersionsWidth(byte version, int expectedWidth)
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(version, DbnSchema.Trades);
            metadata.SymbolCStrLen = 0;

            byte[] dbn = DbnWriterRoundTripTests.Write(metadata, Array.Empty<TradeRecordDbn>(), DbnWriter.WriteTrades);

            Assert.AreEqual(expectedWidth, DbnWriterRoundTripTests.ReadMetadata(dbn).SymbolCStrLen);
        }

        // ---- undefined values ----

        [TestMethod]
        public void WriteTrades_UndefinedPriceAndTimestamp_AreWrittenAsDbnsUndefinedValues()
        {
            TradeRecordDbn record = Trade();
            record.Price = double.NaN;
            record.TsReceivedUtc = DateTime.MinValue;

            byte[] bytes = RecordBytes(record);

            Assert.AreEqual("FFFFFFFFFFFFFF7F", Convert.ToHexString(bytes, 16, 8), "price: i64::MAX");
            Assert.AreEqual("FFFFFFFFFFFFFFFF", Convert.ToHexString(bytes, 32, 8), "ts_recv: u64::MAX");
        }

        [TestMethod]
        public void WriteStatistics_UndefinedQuantityInV1_IsWrittenAsTheThirtyTwoBitUndefinedQuantity()
        {
            StatisticsRecordDbn record = Statistics(long.MaxValue);

            using MemoryStream stream = new MemoryStream(DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(1, DbnSchema.Statistics), new[] { record }, DbnWriter.WriteStatistics));
            StatisticsRecordDbn read = DbnReader.ReadStatistics(stream).Single();

            Assert.AreEqual((long)int.MaxValue, read.Quantity);
            Assert.IsNull(((IStatisticsRecord)read).Quantity);
        }

        [DataTestMethod]
        [DataRow((byte)1, 64)]
        [DataRow((byte)2, 64)]
        [DataRow((byte)3, 80)]
        public void StatisticsWriteToBytes_RecordLengthIsTheVersions(byte version, int recordLength)
        {
            using MemoryStream output = new MemoryStream();
            using(BinaryWriter writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                Statistics(12).WriteToBytes(writer, version);
            }

            Assert.AreEqual(recordLength, output.Length);
            Assert.AreEqual(recordLength / 4, output.ToArray()[0]);
        }

        // ---- streams ----

        [TestMethod]
        public async Task WriteTradesAsync_ReaderPipedIntoWriter_WritesTheSameBytes()
        {
            // more records than the writer holds before passing them to the stream
            TradesSeed[] seeds = Enumerable.Range(0, 5000).Select(i => Trades((uint)(800 + i % 3), 100 + i * 0.25)).ToArray();
            byte[] original = DbnBinaryBuilder.BuildTradesStream(seeds);
            DbnMetadata metadata = DbnWriterRoundTripTests.ReadMetadata(original);

            using MemoryStream input = new MemoryStream(original);
            using MemoryStream output = new MemoryStream();
            await DbnWriter.WriteTradesAsync(output, metadata, DbnReader.ReadTradesAsync(input));

            Assert.AreEqual(Convert.ToHexString(original), Convert.ToHexString(output.ToArray()));
        }

        [TestMethod]
        public async Task WriteTradesAsync_LeavesTheCallersStreamOpen()
        {
            using MemoryStream output = new MemoryStream();
            await DbnWriter.WriteTradesAsync(output, DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades), AsAsync(Trade()));

            Assert.IsTrue(output.CanWrite);
        }

        [TestMethod]
        public void WriteTrades_NullArguments_Throw()
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades);
            TradeRecordDbn[] records = { Trade() };

            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTrades(null, metadata, records));
            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTrades(new MemoryStream(), null, records));
            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTrades(new MemoryStream(), metadata, null));
            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTradesAsync(null, metadata, AsAsync(records)));
            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTradesAsync(new MemoryStream(), null, AsAsync(records)));
            Assert.ThrowsException<ArgumentNullException>(() => DbnWriter.WriteTradesAsync(new MemoryStream(), metadata, null));
        }

        // ---- refusals ----

        [DataTestMethod]
        [DataRow((byte)0)]
        [DataRow((byte)4)]
        public void WriteTrades_VersionOtherThanOneToThree_IsRefusedBeforeAnythingIsWritten(byte version)
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades);
            metadata.Version = version;
            using MemoryStream output = new MemoryStream();

            Assert.ThrowsException<ArgumentException>(() => DbnWriter.WriteTrades(output, metadata, new[] { Trade() }));
            Assert.AreEqual(0, output.Length);
        }

        [TestMethod]
        public void WriteTrades_V1SymbolsOtherThanTwentyTwoWide_AreRefused()
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(1, DbnSchema.Trades);
            metadata.SymbolCStrLen = 71;

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(metadata, new[] { Trade() }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteTrades_TsOut_IsRefused()
        {
            // the record classes keep no ts_out, so there is none to append to each record
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades);
            metadata.TsOut = true;

            Assert.ThrowsException<NotSupportedException>(() => DbnWriterRoundTripTests.Write(metadata, new[] { Trade() }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteDefinitions_V2_IsRefusedAsTheReaderRefusesIt()
        {
            DefinitionRecordDbn record = new DefinitionRecordDbn { RecordType = RType.InstrumentDef };

            Assert.ThrowsException<NotSupportedException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(2, DbnSchema.Definition), new[] { record }, DbnWriter.WriteDefinitions));
        }

        [TestMethod]
        public void WriteTrades_RecordOfAnotherRecordType_IsRefused()
        {
            TradeRecordDbn record = Trade();
            record.RecordType = RType.Mbp1;

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades), new[] { record }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteTrades_NullRecord_IsRefused()
        {
            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades), new[] { Trade(), null }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteTrades_SymbolWiderThanItsField_IsRefused()
        {
            // a v1 symbol field is 22 bytes, the last kept for the terminating NUL
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(1, DbnSchema.Trades);
            metadata.Symbols.Add("SPXW  240102C047000001");

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(metadata, new[] { Trade() }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteTrades_NonAsciiDataset_IsRefused()
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades);
            metadata.Dataset = "XNAS.\u00CFTCH";

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(metadata, new[] { Trade() }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteStatistics_QuantityBeyondThirtyTwoBitsInV1_IsRefused()
        {
            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(1, DbnSchema.Statistics), new[] { Statistics(5_000_000_000L) }, DbnWriter.WriteStatistics));
        }

        [TestMethod]
        public void WriteDefinitions_RawInstrumentIdBeyondThirtyTwoBitsInV1_IsRefused()
        {
            DefinitionRecordDbn record = new DefinitionRecordDbn { RecordType = RType.InstrumentDef, RawInstrumentId = 5_000_000_000UL };

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(1, DbnSchema.Definition), new[] { record }, DbnWriter.WriteDefinitions));
        }

        [TestMethod]
        public void WriteTrades_InfinitePrice_IsRefused()
        {
            TradeRecordDbn record = Trade();
            record.Price = double.PositiveInfinity;

            Assert.ThrowsException<ArgumentException>(() => RecordBytes(record));
        }

        [TestMethod]
        public void WriteTrades_TimestampBeforeTheUnixEpoch_IsRefused()
        {
            TradeRecordDbn record = Trade();
            record.TsEventUtc = new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            Assert.ThrowsException<ArgumentException>(() => RecordBytes(record));
        }

        [TestMethod]
        public void WriteTrades_TimestampBeyondSixtyFourBitNanoseconds_IsRefusedRatherThanWrapped()
        {
            // u64 nanoseconds end in 2554; DateTime.MaxValue, a common "no expiry" sentinel, is past it
            TradeRecordDbn record = Trade();
            record.TsReceivedUtc = DateTime.MaxValue;

            Assert.ThrowsException<ArgumentException>(() => RecordBytes(record));
        }

        [TestMethod]
        public void WriteTrades_MetadataEndBeyondSixtyFourBitNanoseconds_IsRefused()
        {
            DbnMetadata metadata = DbnWriterRoundTripTests.Metadata(3, DbnSchema.Trades);
            metadata.End = DateTimeOffset.MaxValue;

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(metadata, new[] { Trade() }, DbnWriter.WriteTrades));
        }

        [TestMethod]
        public void WriteStatistics_V1UndefinedQuantityWrittenAsV3_IsTheSixtyFourBitUndefinedQuantity()
        {
            // a v1 statistics stream read back gives int.MaxValue for an undefined quantity; written as v3 it stays undefined, as Databento's v1-to-v3 upgrade maps it
            byte[] v1 = DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(1, DbnSchema.Statistics), new[] { Statistics(long.MaxValue) }, DbnWriter.WriteStatistics);
            StatisticsRecordDbn readFromV1;
            using(MemoryStream stream = new MemoryStream(v1))
            {
                readFromV1 = DbnReader.ReadStatistics(stream).Single();
            }

            Assert.AreEqual((long)int.MaxValue, readFromV1.Quantity);

            using MemoryStream v3 = new MemoryStream(DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(3, DbnSchema.Statistics), new[] { readFromV1 }, DbnWriter.WriteStatistics));
            Assert.AreEqual(long.MaxValue, DbnReader.ReadStatistics(v3).Single().Quantity);
        }

        [TestMethod]
        public void WriteMbp10_LevelsOtherThanTen_AreRefused()
        {
            Mbp10RecordDbn record = new Mbp10RecordDbn { RecordType = RType.Mbp10, Side = TradeAggressor.None, Levels = new Mbp1LevelDbn[9] };

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(3, DbnSchema.Mbp10), new[] { record }, DbnWriter.WriteMbp10));
        }

        [TestMethod]
        public void WriteMbo_ChannelIdBeyondOneByte_IsRefused()
        {
            MboRecordDbn record = new MboRecordDbn { RecordType = RType.Mbo, Side = TradeAggressor.None, ChannelId = 256 };

            Assert.ThrowsException<ArgumentException>(() => DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.Metadata(3, DbnSchema.Mbo), new[] { record }, DbnWriter.WriteMbo));
        }

        // ---- helpers ----

        private static void AssertWrittenAgainByteForByte<T>(byte[] original, Func<Stream, IEnumerable<T>> read, Action<Stream, DbnMetadata, IEnumerable<T>> write)
        {
            List<T> records;
            using(MemoryStream input = new MemoryStream(original))
            {
                records = read(input).ToList();
            }

            byte[] written = DbnWriterRoundTripTests.Write(DbnWriterRoundTripTests.ReadMetadata(original), records, write);

            Assert.AreEqual(Convert.ToHexString(original), Convert.ToHexString(written));
        }

        private static byte[] RecordBytes(TradeRecordDbn record)
        {
            using MemoryStream output = new MemoryStream();
            using(BinaryWriter writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                record.WriteToBytes(writer);
            }

            return output.ToArray();
        }

        private static async IAsyncEnumerable<T> AsAsync<T>(params T[] records)
        {
            foreach(T record in records)
            {
                await Task.Yield();
                yield return record;
            }
        }

        private static TradesSeed Trades(uint instrumentId, double price)
        {
            return new TradesSeed { PublisherId = 2, InstrumentId = instrumentId, TsEvent = TsEvent, TsReceived = TsReceived, Price = price, Size = 5, TsInDelta = 17, Sequence = 99 };
        }

        private static TcbboSeed Tcbbo(uint instrumentId)
        {
            return new TcbboSeed { PublisherId = 30, InstrumentId = instrumentId, TsEvent = TsEvent, TsReceived = TsReceived, Price = 12.35, Size = 6, TsInDelta = 377, BidPrice = 12.3, AskPrice = 12.4, BidSize = 15, AskSize = 17, BidPublisherId = 41, AskPublisherId = 43 };
        }

        private static SymbolMappingSeed Mapping(string rawSymbol, string instrumentId)
        {
            return new SymbolMappingSeed
            {
                RawSymbol = rawSymbol,
                Intervals = new[] { new SymbolMappingIntervalSeed { StartDate = Day, EndDate = Day.AddDays(1), Symbol = instrumentId } },
            };
        }

        private static TradeRecordDbn Trade()
        {
            return new TradeRecordDbn
            {
                RecordType = RType.Mbp0,
                PublisherId = 2,
                InstrumentId = 800,
                TsEventUtc = TsEvent.UtcDateTime,
                Price = 101.25,
                Size = 5,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Buyer,
                TsReceivedUtc = TsReceived.UtcDateTime,
            };
        }

        private static StatisticsRecordDbn Statistics(long quantity)
        {
            return new StatisticsRecordDbn
            {
                RecordType = RType.Statistics,
                PublisherId = 20,
                InstrumentId = 800,
                TsEventUtc = TsEvent.UtcDateTime,
                TsReceivedUtc = TsReceived.UtcDateTime,
                Price = 4718.25,
                Quantity = quantity,
                StatType = 9,
            };
        }
    }
}
