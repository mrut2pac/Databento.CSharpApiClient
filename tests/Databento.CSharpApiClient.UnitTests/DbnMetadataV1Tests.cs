using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// DBN v1 metadata, which the live API serves for XNAS.ITCH and OPRA.PILLAR, carries an 8-byte record_count after the
    /// limit that later versions dropped. Reading it as the v2 layout misplaces every field after the limit, so stype_out
    /// reads as Null and no record gets a symbol from the mappings.
    /// </summary>
    [TestClass]
    public class DbnMetadataV1Tests
    {
        // The prelude and metadata of a live XNAS.ITCH trades response for SPY, 2022-05-16 13:30-14:30 UTC: DBN v1, stype_in
        // raw_symbol, stype_out instrument_id, SPY mapped to instrument 15144 from 2022-05-16 to 2022-05-17.
        private const string LiveXnasItchV1Header =
            "44424E01C6000000584E41532E49544348000000000000000400007084D8FC98EF1600103D09439CEF160000000000000000FFFFFFFFFFFF"
            + "FFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
            + "0100000053505900000000000000000000000000000000000000000000000000000001000000535059000000000000000000000000000000"
            + "0000000001000000648A3401658A340131353134340000000000000000000000000000000000";

        // DbnBinaryBuilder writes an 8-byte prelude and a 120-byte metadata body before its records
        private const int BuilderPreludeAndMetadataBytes = 128;

        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);

        [TestMethod]
        [DataRow((byte)0x00, DisplayName = "zero padding is accepted")]
        [DataRow((byte)0x53, DisplayName = "a misread layout is refused")]
        public void FromBinaryReader_BytesAfterTheLastField_OnlyZeroPaddingIsAccepted(byte trailing)
        {
            // one more byte inside the declared metadata length, after the last mapping
            byte[] header = Convert.FromHexString(LiveXnasItchV1Header);
            BitConverter.GetBytes(BitConverter.ToUInt32(header, 4) + 1).CopyTo(header, 4);
            byte[] data = header.Concat(new[] { trailing }).ToArray();
            using BinaryReader reader = new BinaryReader(new MemoryStream(data));

            if(trailing == 0)
            {
                Assert.AreEqual("15144", DbnMetadata.FromBinaryReader(reader).Mappings.Single().Intervals.Single().Symbol);
            }
            else
            {
                Assert.ThrowsException<InvalidDataException>(() => DbnMetadata.FromBinaryReader(reader));
            }
        }

        [TestMethod]
        [DataRow((byte)0)]
        [DataRow((byte)4)]
        public void FromBinaryReader_UnknownVersion_IsRefused(byte version)
        {
            byte[] header = Convert.FromHexString(LiveXnasItchV1Header);
            header[3] = version;

            Assert.ThrowsException<InvalidDataException>(() => DbnMetadata.FromBinaryReader(new BinaryReader(new MemoryStream(header))));
        }

        [TestMethod]
        public void FromBinaryReader_LiveV1Header_ReadsEveryFieldAfterTheRecordCount()
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(Convert.FromHexString(LiveXnasItchV1Header)));

            DbnMetadata metadata = DbnMetadata.FromBinaryReader(reader);

            Assert.AreEqual(1, metadata.Version);
            Assert.AreEqual("XNAS.ITCH", metadata.Dataset);
            Assert.IsNull(metadata.Limit);
            Assert.AreEqual(DbnSType.RawSymbol, metadata.STypeIn);
            Assert.AreEqual(DbnSType.InstrumentId, metadata.STypeOut);
            Assert.IsFalse(metadata.TsOut);
            Assert.AreEqual((ushort)22, metadata.SymbolCStrLen);
            CollectionAssert.AreEqual(new[] { "SPY" }, metadata.Symbols);
            Assert.AreEqual(0, metadata.Partial.Count);
            Assert.AreEqual(0, metadata.NotFound.Count);
            DbnSymbolMapping mapping = metadata.Mappings.Single();
            Assert.AreEqual("SPY", mapping.RawSymbol);
            DbnMappingInterval interval = mapping.Intervals.Single();
            Assert.AreEqual(20220516u, interval.StartDate);
            Assert.AreEqual(20220517u, interval.EndDate);
            Assert.AreEqual("15144", interval.Symbol);
        }

        [TestMethod]
        public void ReadTrades_LiveV1Header_GivesTheRecordItsSymbol()
        {
            // the live header followed by one trade of instrument 15144 on the mapped date
            byte[] trade = DbnBinaryBuilder.BuildTradesStream(new TradesSeed
            {
                InstrumentId = 15144,
                TsEvent = Start,
                TsReceived = Start,
                Price = 401.25,
                Size = 100,
            });
            byte[] data = Convert.FromHexString(LiveXnasItchV1Header).Concat(trade.Skip(BuilderPreludeAndMetadataBytes)).ToArray();

            TradeRecordDbn record = DbnReader.ReadTrades(new MemoryStream(data)).Single();

            Assert.AreEqual("SPY", record.Symbol);
        }

        [TestMethod]
        public async Task GetTradesAsync_V1Stream_GivesRecordsTheirSymbol()
        {
            DateOnly day = DateOnly.FromDateTime(Start.UtcDateTime);
            byte[] bytes = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTradesStreamV1(new TradesSeed { InstrumentId = 800, TsEvent = Start, TsReceived = Start, Price = 1, Size = 1 }),
                new SymbolMappingSeed
                {
                    RawSymbol = "SPY",
                    Intervals = new[]
                    {
                        new SymbolMappingIntervalSeed
                        {
                            StartDate = day,
                            EndDate = day.AddDays(1),
                            Symbol = "800",
                        },
                    },
                });
            FakeTransport transport = new FakeTransport(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes),
            });
            using DatabentoClient client = new DatabentoClient(new DatabentoOptions { ApiKey = "test-key", MaxRetries = 0 }, transport.Object);

            TradeRecordDbn[] records = await client.GetTradesAsync("XNAS.ITCH", "SPY", Start, Start.AddHours(1));

            Assert.AreEqual("SPY", records.Single().Symbol);
        }
    }
}
