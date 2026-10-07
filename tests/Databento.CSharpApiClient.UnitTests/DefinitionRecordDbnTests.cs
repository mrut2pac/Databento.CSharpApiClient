// Ignore Spelling: Databento Dbn Spxw

using System;
using System.IO;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Pins <see cref="DefinitionRecordDbn"/> against real instrument definitions captured from the Historical API. The API serves both
    /// DBN v1 (360-byte records, e.g. OPRA.PILLAR and XNAS.ITCH) and DBN v3 (520-byte records, e.g. GLBX.MDP3), whose layouts differ.
    /// </summary>
    [TestClass]
    public class DefinitionRecordDbnTests
    {
        // OPRA.PILLAR, SPXW 2022-02-07 4295 call, as served (DBN v1).
        private const string SpxwCallV1Hex =
            "5A131E00E5FF1300006846617187D116006846617187D116FFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7F00008AA6E957D116FFFFFFFFFFFFFFFFFFFFFFFFFFFFFF7F"
            + "FFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7FFFFFFFFFFFFFFF7FFFFFFF7FDD01000000000000FFFFFF7F"
            + "FFFFFF7FFFFFFFFFFFFFFFFFFFFFFF7FFFFFFF7FFFFFFF7FFFFFFFFF00000000FFFFFF7FFFFFFF7FFFFFFF7F00000000FFFFFF7FFFFFFFFFFFFF555344000000"
            + "0000000000000000535058572020323230323037433034323935303030000000000000000000000000000000000000000000004F505241005350585700000000"
            + "0000000000004F505400000000555344000000000000000000000000000000000000000000000000000000005350585700000000000000000000000000000000"
            + "00555344004300000006F301E803000000000000000020FFFFFFFFFFFF41FFFFFF4E7F7FFF000000";

        // GLBX.MDP3, ESH4 3350 put, as served (DBN v3).
        private const string EsPutV3Hex =
            "821301002FCA0200331D16F8276BBB1700003B0209C7BC17FFFFFFFFFFFFFF7F80969800000000000070F7933CF3BC1700B89A60DBA7C116FFFFFFFFFFFFFF7F"
            + "80F0FA0200000000FFFFFFFFFFFFFF7F00743BA40B0000000000000000000000FFFFFFFFFFFFFF7F005C8FFB0B0300002FCA020000000000FFFFFFFFFFFFFF7F"
            + "FFFFFFFFFFFFFF7F03200400B5420000000000000300000036000000B80B000000000000000000000000000001000000FFFFFF7FFFFFFF7FFFFFFF7F00000000"
            + "00000000000000000000000000000000000000003701E807FFFF0100000000005553440000000000000000000000455348342050333335300000000000000000"
            + "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000004557000000000000000000"
            + "0000000000000000000058434D450045530000000000000000004F5041465053004F4F460000000049504E540000000000000000000000000000000000000000"
            + "00000000000000455348340000000000000000000000000000000000555344000000000000000000000000000000000000000000000000000000000000000000"
            + "0000000000000000000000000000000000000000000000000000000000000000000000000000005046FFFFFF054103FFFF4E7F7F0F004E000000000000000000"
            + "0000000000000000";

        [TestMethod]
        public void ReadFromBytes_DbnV1OptionDefinition_DecodesEveryField()
        {
            DefinitionRecordDbn record = Decode(SpxwCallV1Hex);

            Assert.AreEqual(1310693u, record.InstrumentId);
            Assert.AreEqual("SPXW  220207C04295000", record.RawSymbol);
            Assert.AreEqual(4295.0, record.StrikePrice, 1e-9);
            Assert.AreEqual('C', record.InstrumentClass);
            Assert.AreEqual('A', record.Action);
            Assert.AreEqual("OPRA", record.Exchange);
            Assert.AreEqual("SPXW", record.Asset);
            Assert.AreEqual("OPT", record.SecurityType);
            Assert.AreEqual(new DateTime(2022, 2, 7, 0, 0, 0, DateTimeKind.Utc), record.Expiration);
            Assert.IsNull(record.Activation, "u64::MAX is DBN's undefined timestamp");

            // the rest of the record, as the JSON encoding of the same request reads it
            Assert.AreEqual(477u, record.UnderlyingId);
            Assert.AreEqual(0UL, record.RawInstrumentId);
            Assert.AreEqual("SPXW", record.Underlying);
            Assert.AreEqual("USD", record.Currency);
            Assert.AreEqual("USD", record.UnitOfMeasure);
            Assert.AreEqual("USD", record.StrikePriceCurrency);
            Assert.AreEqual(string.Empty, record.Group);
            Assert.AreEqual(int.MaxValue, record.InstrumentAttributeValue);
            Assert.AreEqual(uint.MaxValue, record.MaxTradeVolume);
            Assert.AreEqual(int.MaxValue, record.OriginalContractSize);
            Assert.IsTrue(double.IsNaN(record.ContractMultiplier));
            Assert.IsTrue(double.IsNaN(record.TradingReferencePrice));
            Assert.AreEqual(ushort.MaxValue, record.TradingReferenceDate);
            Assert.AreEqual((short)32767, record.ApplId);
            Assert.AreEqual(ushort.MaxValue, record.ChannelId);
            Assert.AreEqual(' ', record.MatchAlgorithm);
            Assert.AreEqual((byte)255, record.MdSecurityTradingStatus);
            Assert.AreEqual((byte)255, record.SettlementPriceType);
            Assert.AreEqual('N', record.UserDefinedInstrument);
            Assert.AreEqual((sbyte)127, record.ContractMultiplierUnit);
            Assert.AreEqual((byte)255, record.TickRule);
            Assert.AreEqual((ushort)0, record.LegCount, "v1 has no legs");
            Assert.IsTrue(double.IsNaN(record.LegPrice), "v1 has no legs");
        }

        [TestMethod]
        public void ReadFromBytes_DbnV3OptionDefinition_DecodesEveryField()
        {
            DefinitionRecordDbn record = Decode(EsPutV3Hex);

            Assert.AreEqual(182831u, record.InstrumentId);
            Assert.AreEqual("ESH4 P3350", record.RawSymbol);
            Assert.AreEqual(3350.0, record.StrikePrice, 1e-9);
            Assert.AreEqual('P', record.InstrumentClass);
            Assert.AreEqual('A', record.Action);
            Assert.AreEqual("XCME", record.Exchange);
            Assert.AreEqual("ES", record.Asset);
            Assert.AreEqual("OPAFPS", record.Cfi);
            Assert.AreEqual(new DateTime(2024, 3, 15, 13, 30, 0, DateTimeKind.Utc), record.Expiration);
            Assert.AreEqual("OOF", record.SecurityType);
            Assert.AreEqual(50.0, record.UnitOfMeasureQty, 1e-9);

            // the rest of the record, as the JSON encoding of the same request reads it
            Assert.AreEqual(17077u, record.UnderlyingId);
            Assert.AreEqual(182831UL, record.RawInstrumentId);
            Assert.AreEqual("ESH4", record.Underlying);
            Assert.AreEqual("EW", record.Group);
            Assert.AreEqual("IPNT", record.UnitOfMeasure);
            Assert.AreEqual(270339, record.InstrumentAttributeValue);
            Assert.AreEqual(3, record.MarketDepth);
            Assert.AreEqual(54u, record.MarketSegmentId);
            Assert.AreEqual(3000u, record.MaxTradeVolume);
            Assert.AreEqual(1u, record.MinTradeVolume);
            Assert.AreEqual(0.0, record.MinPriceIncrementAmount, 1e-9);
            Assert.IsTrue(double.IsNaN(record.ContractMultiplier));
            Assert.AreEqual((short)311, record.ApplId);
            Assert.AreEqual((ushort)2024, record.MaturityYear);
            Assert.AreEqual((ushort)1, record.ChannelId);
            Assert.AreEqual('F', record.MatchAlgorithm);
            Assert.AreEqual((byte)5, record.UnderlyingProduct);
            Assert.AreEqual((byte)3, record.MaturityMonth);
            Assert.AreEqual((byte)15, record.TickRule);
            Assert.AreEqual((ushort)0, record.LegCount);
            Assert.AreEqual('N', record.LegSide);
            Assert.IsNull(record.LegInstrumentClass);
            Assert.IsTrue(double.IsNaN(record.LegPrice));
            Assert.IsTrue(double.IsNaN(record.TradingReferencePrice), "v3 has no trading reference price");
            Assert.AreEqual(ushort.MaxValue, record.TradingReferenceDate, "v3 has no trading reference date");
        }

        [TestMethod]
        public void ReadFromBytes_UnknownRecordLength_IsRefused()
        {
            // A DBN v2 definition (400 bytes) has a layout no live response has been seen in, so it is refused rather than misread.
            byte[] record = new byte[400];
            record[0] = 400 / 4;
            record[1] = (byte)RType.InstrumentDef;

            Assert.ThrowsException<InvalidDataException>(() => Decode(Convert.ToHexString(record)));
        }

        [TestMethod]
        public void ReadFromBytes_TruncatedRecord_IsRefused()
        {
            // the v1 record less its last byte: the header still claims 360 bytes
            string truncated = SpxwCallV1Hex.Substring(0, SpxwCallV1Hex.Length - 2);

            Assert.ThrowsException<InvalidDataException>(() => Decode(truncated));
        }

        private static DefinitionRecordDbn Decode(string recordHex)
        {
            using MemoryStream stream = new MemoryStream(Convert.FromHexString(recordHex));
            using BinaryReader reader = new BinaryReader(stream);
            DbnRecordHeader header = DbnRecordHeader.ReadFromBytes(reader);

            return DefinitionRecordDbn.ReadFromBytes(header, reader);
        }
    }
}
