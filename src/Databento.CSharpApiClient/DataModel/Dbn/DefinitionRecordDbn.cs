using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An instrument-definition record deserialized from a DBN binary stream.
    /// Schema: <c>definition</c> — rtype <c>InstrumentDef</c> (0x13).
    /// Decodes every field of DBN v1 (360-byte) and v3 (520-byte) records, both of which the API serves; any other length is refused.
    /// </summary>
    /// <remarks>
    /// A venue that doesn't set an integer field sends the largest value of its type as "undefined"; an undefined price or
    /// <see cref="ContractMultiplier"/> reads as <see cref="double.NaN"/>.
    /// </remarks>
    public sealed class DefinitionRecordDbn
    {
        private const int V1RecordBytes = 360;

        private const int V3RecordBytes = 520;

        /// <summary>DBN record-type discriminator (<see cref="RType.InstrumentDef"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this definition.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Minimum allowed price increment (tick size), display-scaled.</summary>
        public double MinPriceIncrement { get; set; }

        /// <summary>Multiplier to convert the venue's raw price to a display price.</summary>
        public double DisplayFactor { get; set; }

        /// <summary>Contract expiry timestamp (UTC). <see langword="null"/> for non-expiring instruments.</summary>
        public DateTime? Expiration { get; set; }

        /// <summary>First trading date/time for this instrument (UTC).</summary>
        public DateTime? Activation { get; set; }

        /// <summary>Upper price limit (display-scaled).</summary>
        public double HighLimitPrice { get; set; }

        /// <summary>Lower price limit (display-scaled).</summary>
        public double LowLimitPrice { get; set; }

        /// <summary>Maximum price movement allowed between trades (display-scaled).</summary>
        public double MaxPriceVariation { get; set; }

        /// <summary>Contract unit-of-measure quantity (display-scaled).</summary>
        public double UnitOfMeasureQty { get; set; }

        /// <summary>Option strike price (display-scaled). <see cref="double.NaN"/> when undefined (e.g. non-option instruments).</summary>
        public double StrikePrice { get; set; }

        /// <summary>Venue-native symbol string.</summary>
        public string RawSymbol { get; set; }

        /// <summary>Exchange MIC code where this instrument trades.</summary>
        public string Exchange { get; set; }

        /// <summary>Underlying asset code (e.g. <c>"SPX"</c>).</summary>
        public string Asset { get; set; }

        /// <summary>CFI code (ISO 10962) classifying the instrument type.</summary>
        public string Cfi { get; set; }

        /// <summary>Exchange-specific security type code.</summary>
        public string SecurityType { get; set; }

        /// <summary>
        /// Single-character instrument class: 'F' futures, 'C' call option, 'P' put option,
        /// 'K' FX forward, 'S' stock, 'M' mixed spread. <see langword="null"/> when not set.
        /// </summary>
        public char? InstrumentClass { get; set; }

        /// <summary>Action that caused this definition message: 'A' add, 'M' modify, 'D' delete. <see langword="null"/> when not set.</summary>
        public char? Action { get; set; }

        /// <summary>Trading reference price (display-scaled); <see cref="double.NaN"/> when undefined, and in the DBN v3 layout, which has no such field.</summary>
        public double TradingReferencePrice { get; set; }

        /// <summary>Value of one <see cref="MinPriceIncrement"/> tick in the contract currency; <see cref="double.NaN"/> when undefined.</summary>
        public double MinPriceIncrementAmount { get; set; }

        /// <summary>Price ratio between the legs of a spread; <see cref="double.NaN"/> when undefined.</summary>
        public double PriceRatio { get; set; }

        /// <summary>Venue-defined bitmap of instrument eligibility attributes.</summary>
        public int InstrumentAttributeValue { get; set; }

        /// <summary>Instrument ID of the underlying instrument; 0 when there is none.</summary>
        public uint UnderlyingId { get; set; }

        /// <summary>The instrument ID the venue assigned (32 bits in the DBN v1 layout, 64 in v3).</summary>
        public ulong RawInstrumentId { get; set; }

        /// <summary>Implied book depth the venue publishes.</summary>
        public int MarketDepthImplied { get; set; }

        /// <summary>Outright book depth the venue publishes.</summary>
        public int MarketDepth { get; set; }

        /// <summary>Venue market segment.</summary>
        public uint MarketSegmentId { get; set; }

        /// <summary>Maximum order size.</summary>
        public uint MaxTradeVolume { get; set; }

        /// <summary>Minimum lot size.</summary>
        public int MinLotSize { get; set; }

        /// <summary>Minimum lot size for a block trade.</summary>
        public int MinLotSizeBlock { get; set; }

        /// <summary>Minimum lot size for a round lot.</summary>
        public int MinLotSizeRoundLot { get; set; }

        /// <summary>Minimum order size.</summary>
        public uint MinTradeVolume { get; set; }

        /// <summary>Contract size multiplier, of the type given by <see cref="ContractMultiplierUnit"/>; <see cref="double.NaN"/> when undefined.</summary>
        public double ContractMultiplier { get; set; }

        /// <summary>Contracts that decay daily, for a decaying contract.</summary>
        public int DecayQuantity { get; set; }

        /// <summary>Contract size of each instrument before decay.</summary>
        public int OriginalContractSize { get; set; }

        /// <summary>Trading session date of <see cref="TradingReferencePrice"/>, in days since the UNIX epoch; 65535 when undefined, and in the DBN v3 layout.</summary>
        public ushort TradingReferenceDate { get; set; }

        /// <summary>Venue channel (application) ID.</summary>
        public short ApplId { get; set; }

        /// <summary>Calendar year of the contract maturity.</summary>
        public ushort MaturityYear { get; set; }

        /// <summary>Date decay starts, in days since the UNIX epoch.</summary>
        public ushort DecayStartDate { get; set; }

        /// <summary>Feed channel the instrument is published on.</summary>
        public ushort ChannelId { get; set; }

        /// <summary>Currency of the prices (e.g. <c>"USD"</c>).</summary>
        public string Currency { get; set; }

        /// <summary>Settlement currency, when it differs from <see cref="Currency"/>.</summary>
        public string SettlementCurrency { get; set; }

        /// <summary>Strategy type of a spread.</summary>
        public string SecuritySubType { get; set; }

        /// <summary>Product group code.</summary>
        public string Group { get; set; }

        /// <summary>Unit of measure of the underlying (e.g. <c>"USD"</c>, <c>"IPNT"</c>).</summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>Underlying instrument symbol for derivatives.</summary>
        public string Underlying { get; set; }

        /// <summary>Currency of <see cref="StrikePrice"/>.</summary>
        public string StrikePriceCurrency { get; set; }

        /// <summary>Venue matching algorithm code.</summary>
        public char? MatchAlgorithm { get; set; }

        /// <summary>Trading status of the instrument; 255 when undefined, and in the DBN v3 layout.</summary>
        public byte MdSecurityTradingStatus { get; set; }

        /// <summary>Price denominator of the main fraction.</summary>
        public byte MainFraction { get; set; }

        /// <summary>Number of digits to the right of the tick mark, used for display.</summary>
        public byte PriceDisplayFormat { get; set; }

        /// <summary>Settlement price type; 255 when undefined, and in the DBN v3 layout.</summary>
        public byte SettlementPriceType { get; set; }

        /// <summary>Price denominator of the sub-fraction.</summary>
        public byte SubFraction { get; set; }

        /// <summary>Product complex of the instrument.</summary>
        public byte UnderlyingProduct { get; set; }

        /// <summary>Calendar month of the contract maturity.</summary>
        public byte MaturityMonth { get; set; }

        /// <summary>Calendar day of the contract maturity.</summary>
        public byte MaturityDay { get; set; }

        /// <summary>Calendar week of the contract maturity.</summary>
        public byte MaturityWeek { get; set; }

        /// <summary>'Y' for a user-defined instrument, 'N' otherwise.</summary>
        public char? UserDefinedInstrument { get; set; }

        /// <summary>Type of <see cref="ContractMultiplier"/>.</summary>
        public sbyte ContractMultiplierUnit { get; set; }

        /// <summary>Schedule for delivering electricity.</summary>
        public sbyte FlowScheduleType { get; set; }

        /// <summary>Tick rule of the spread.</summary>
        public byte TickRule { get; set; }

        /// <summary>Number of legs of a multi-leg instrument; 0 for an outright. The <c>Leg*</c> properties are in the DBN v3 layout only, and keep their defaults (0, NaN or <see langword="null"/>) for a v1 record.</summary>
        public ushort LegCount { get; set; }

        /// <summary>Index of the leg this record describes, for a multi-leg instrument.</summary>
        public ushort LegIndex { get; set; }

        /// <summary>Instrument ID of the leg.</summary>
        public uint LegInstrumentId { get; set; }

        /// <summary>Raw symbol of the leg.</summary>
        public string LegRawSymbol { get; set; }

        /// <summary>Instrument class of the leg; <see langword="null"/> for an outright.</summary>
        public char? LegInstrumentClass { get; set; }

        /// <summary>Side of the leg: 'A', 'B' or 'N' (none).</summary>
        public char? LegSide { get; set; }

        /// <summary>Price of the leg (display-scaled); <see cref="double.NaN"/> when undefined.</summary>
        public double LegPrice { get; set; }

        /// <summary>Delta of the leg; <see cref="double.NaN"/> when undefined.</summary>
        public double LegDelta { get; set; }

        /// <summary>Numerator of the leg price ratio.</summary>
        public int LegRatioPriceNumerator { get; set; }

        /// <summary>Denominator of the leg price ratio.</summary>
        public int LegRatioPriceDenominator { get; set; }

        /// <summary>Numerator of the leg quantity ratio.</summary>
        public int LegRatioQtyNumerator { get; set; }

        /// <summary>Denominator of the leg quantity ratio.</summary>
        public int LegRatioQtyDenominator { get; set; }

        /// <summary>Instrument ID of the leg underlying.</summary>
        public uint LegUnderlyingId { get; set; }

        /// <summary>
        /// Deserialises a definition record body from <paramref name="reader"/> using the already-parsed <paramref name="header"/>,
        /// choosing the DBN v1 or v3 layout by record length. Consumes the whole body, so the reader is left at the next record.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        /// <exception cref="InvalidDataException">If the record is truncated or its length matches no supported DBN version.</exception>
        public static DefinitionRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                DefinitionRecordDbn record = new DefinitionRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                // the API serves several DBN versions and each lays the record out differently, so pick the layout by record length -
                // before reading, so an unknown or corrupt length never reaches the read
                if(header.RecordLength != V1RecordBytes && header.RecordLength != V3RecordBytes)
                {
                    throw new InvalidDataException(
                        $"Unsupported DBN Definition record length {header.RecordLength}: only DBN v1 ({V1RecordBytes} bytes) and v3 ({V3RecordBytes} bytes) are decoded.");
                }

                byte[] body = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);
                if(body.Length != header.RecordLength - DbnRecordHeader.SizeBytes)
                {
                    throw new EndOfStreamException();
                }

                record.TsReceivedUtc     = Utils.FromUnixNs(BinaryPrimitives.ReadUInt64LittleEndian(body.AsSpan(0))).UtcDateTime;
                record.MinPriceIncrement = ReadPrice(body, 8);
                record.DisplayFactor     = ReadPrice(body, 16);
                record.Expiration        = ReadTimestamp(body, 24);
                record.Activation        = ReadTimestamp(body, 32);
                record.HighLimitPrice    = ReadPrice(body, 40);
                record.LowLimitPrice     = ReadPrice(body, 48);
                record.MaxPriceVariation = ReadPrice(body, 56);

                if(header.RecordLength == V1RecordBytes)
                {
                    ReadV1Fields(record, body);
                }
                else
                {
                    ReadV3Fields(record, body);
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Definition record.");
            }
        }

        // Body offsets (after the 16-byte header) of the DBN v1 layout; trading_reference_price sits in the i64 block, strike_price after the strings
        private static void ReadV1Fields(DefinitionRecordDbn record, byte[] body)
        {
            record.UnitOfMeasureQty = ReadPrice(body, 72);
            record.StrikePrice = ReadPrice(body, 312);
            record.RawSymbol = ReadAscii(body, 184, 22);
            record.Exchange = ReadAscii(body, 227, 5);
            record.Asset = ReadAscii(body, 232, 7);
            record.Cfi = ReadAscii(body, 239, 7);
            record.SecurityType = ReadAscii(body, 246, 7);
            record.InstrumentClass = ReadChar(body, 309);
            record.Action = ReadChar(body, 333);
            record.TradingReferencePrice = ReadPrice(body, 64);
            record.MinPriceIncrementAmount = ReadPrice(body, 80);
            record.PriceRatio = ReadPrice(body, 88);
            record.InstrumentAttributeValue = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(96));
            record.UnderlyingId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(100));
            record.RawInstrumentId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(104));
            record.MarketDepthImplied = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(108));
            record.MarketDepth = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(112));
            record.MarketSegmentId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(116));
            record.MaxTradeVolume = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(120));
            record.MinLotSize = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(124));
            record.MinLotSizeBlock = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(128));
            record.MinLotSizeRoundLot = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(132));
            record.MinTradeVolume = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(136));
            record.ContractMultiplier = ReadUndefinedInt32(body, 144);
            record.DecayQuantity = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(148));
            record.OriginalContractSize = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(152));
            record.TradingReferenceDate = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(160));
            record.ApplId = BinaryPrimitives.ReadInt16LittleEndian(body.AsSpan(162));
            record.MaturityYear = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(164));
            record.DecayStartDate = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(166));
            record.ChannelId = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(168));
            record.Currency = ReadAscii(body, 170, 4);
            record.SettlementCurrency = ReadAscii(body, 174, 4);
            record.SecuritySubType = ReadAscii(body, 178, 6);
            record.Group = ReadAscii(body, 206, 21);
            record.UnitOfMeasure = ReadAscii(body, 253, 31);
            record.Underlying = ReadAscii(body, 284, 21);
            record.StrikePriceCurrency = ReadAscii(body, 305, 4);
            record.MatchAlgorithm = ReadChar(body, 326);
            record.MdSecurityTradingStatus = body[327];
            record.MainFraction = body[328];
            record.PriceDisplayFormat = body[329];
            record.SettlementPriceType = body[330];
            record.SubFraction = body[331];
            record.UnderlyingProduct = body[332];
            record.MaturityMonth = body[334];
            record.MaturityDay = body[335];
            record.MaturityWeek = body[336];
            record.UserDefinedInstrument = ReadChar(body, 337);
            record.ContractMultiplierUnit = (sbyte)body[338];
            record.FlowScheduleType = (sbyte)body[339];
            record.TickRule = body[340];
            record.LegPrice = double.NaN;
            record.LegDelta = double.NaN;
        }

        // Body offsets of the DBN v3 layout: no trading reference fields, strike_price moved up, wider symbols, and the leg fields
        private static void ReadV3Fields(DefinitionRecordDbn record, byte[] body)
        {
            record.UnitOfMeasureQty = ReadPrice(body, 64);
            record.StrikePrice = ReadPrice(body, 88);
            record.RawSymbol = ReadAscii(body, 222, 71);
            record.Exchange = ReadAscii(body, 314, 5);
            record.Asset = ReadAscii(body, 319, 11);
            record.Cfi = ReadAscii(body, 330, 7);
            record.SecurityType = ReadAscii(body, 337, 7);
            record.InstrumentClass = ReadChar(body, 471);
            record.Action = ReadChar(body, 477);
            record.TradingReferencePrice = double.NaN;
            record.MinPriceIncrementAmount = ReadPrice(body, 72);
            record.PriceRatio = ReadPrice(body, 80);
            record.InstrumentAttributeValue = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(120));
            record.UnderlyingId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(124));
            record.RawInstrumentId = BinaryPrimitives.ReadUInt64LittleEndian(body.AsSpan(96));
            record.MarketDepthImplied = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(128));
            record.MarketDepth = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(132));
            record.MarketSegmentId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(136));
            record.MaxTradeVolume = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(140));
            record.MinLotSize = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(144));
            record.MinLotSizeBlock = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(148));
            record.MinLotSizeRoundLot = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(152));
            record.MinTradeVolume = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(156));
            record.ContractMultiplier = ReadUndefinedInt32(body, 160);
            record.DecayQuantity = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(164));
            record.OriginalContractSize = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(168));
            record.TradingReferenceDate = ushort.MaxValue;
            record.ApplId = BinaryPrimitives.ReadInt16LittleEndian(body.AsSpan(196));
            record.MaturityYear = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(198));
            record.DecayStartDate = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(200));
            record.ChannelId = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(202));
            record.Currency = ReadAscii(body, 208, 4);
            record.SettlementCurrency = ReadAscii(body, 212, 4);
            record.SecuritySubType = ReadAscii(body, 216, 6);
            record.Group = ReadAscii(body, 293, 21);
            record.UnitOfMeasure = ReadAscii(body, 344, 31);
            record.Underlying = ReadAscii(body, 375, 21);
            record.StrikePriceCurrency = ReadAscii(body, 396, 4);
            record.MatchAlgorithm = ReadChar(body, 472);
            record.MdSecurityTradingStatus = byte.MaxValue;
            record.MainFraction = body[473];
            record.PriceDisplayFormat = body[474];
            record.SettlementPriceType = byte.MaxValue;
            record.SubFraction = body[475];
            record.UnderlyingProduct = body[476];
            record.MaturityMonth = body[478];
            record.MaturityDay = body[479];
            record.MaturityWeek = body[480];
            record.UserDefinedInstrument = ReadChar(body, 481);
            record.ContractMultiplierUnit = (sbyte)body[482];
            record.FlowScheduleType = (sbyte)body[483];
            record.TickRule = body[484];
            record.LegCount = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(204));
            record.LegIndex = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(206));
            record.LegInstrumentId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(172));
            record.LegRawSymbol = ReadAscii(body, 400, 71);
            record.LegInstrumentClass = ReadChar(body, 485);
            record.LegSide = ReadChar(body, 486);
            record.LegPrice = ReadPrice(body, 104);
            record.LegDelta = ReadPrice(body, 112);
            record.LegRatioPriceNumerator = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(176));
            record.LegRatioPriceDenominator = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(180));
            record.LegRatioQtyNumerator = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(184));
            record.LegRatioQtyDenominator = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(188));
            record.LegUnderlyingId = BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(192));
        }

        private static double ReadUndefinedInt32(byte[] body, int offset)
        {
            int value = BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(offset));
            return value == int.MaxValue ? double.NaN : value;
        }

        private static double ReadPrice(byte[] body, int offset)
        {
            // DBN is little-endian whatever the host is, as the BinaryReader-based decoders read it
            return Utils.NanoToDouble(BinaryPrimitives.ReadInt64LittleEndian(body.AsSpan(offset)));
        }

        private static DateTime? ReadTimestamp(byte[] body, int offset)
        {
            // 0 and u64::MAX (DBN's undefined timestamp) both mean "not set"
            ulong ns = BinaryPrimitives.ReadUInt64LittleEndian(body.AsSpan(offset));
            return ns == 0 || ns == ulong.MaxValue ? (DateTime?)null : Utils.FromUnixNs(ns).UtcDateTime;
        }

        private static string ReadAscii(byte[] body, int offset, int width)
        {
            int end = Array.IndexOf(body, (byte)0, offset, width);
            if(end < 0)
            {
                end = offset + width;
            }

            while(end > offset && body[end - 1] == (byte)' ')
            {
                end--;
            }

            return end == offset ? string.Empty : Encoding.ASCII.GetString(body, offset, end - offset);
        }

        private static char? ReadChar(byte[] body, int offset)
        {
            byte b = body[offset];
            return b == 0 ? (char?)null : (char)b;
        }
    }
}
