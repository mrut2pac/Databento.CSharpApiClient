using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An instrument-definition record deserialized from a DBN binary stream.
    /// Schema: <c>definition</c> — rtype <c>InstrumentDef</c> (0x13).
    /// This is a partial decoder: key pricing and identification fields are decoded and the rest of the body is skipped.
    /// Decodes DBN v1 (360-byte) and v3 (520-byte) records, both of which the API serves; any other length is refused.
    /// </summary>
    public sealed class DefinitionRecordDbn
    {
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
                DefinitionLayout layout = header.RecordLength switch
                {
                    DefinitionLayout.V1RecordBytes => DefinitionLayout.V1,
                    DefinitionLayout.V3RecordBytes => DefinitionLayout.V3,
                    _ => throw new InvalidDataException(
                        $"Unsupported DBN Definition record length {header.RecordLength}: only DBN v1 ({DefinitionLayout.V1RecordBytes} bytes) and v3 ({DefinitionLayout.V3RecordBytes} bytes) are decoded."),
                };

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
                record.UnitOfMeasureQty  = ReadPrice(body, layout.UnitOfMeasureQty);
                record.StrikePrice       = ReadPrice(body, layout.StrikePrice);
                record.RawSymbol         = ReadAscii(body, layout.RawSymbol, layout.RawSymbolWidth);
                record.Exchange          = ReadAscii(body, layout.Exchange, 5);
                record.Asset             = ReadAscii(body, layout.Asset, layout.AssetWidth);
                record.Cfi               = ReadAscii(body, layout.Cfi, 7);
                record.SecurityType      = ReadAscii(body, layout.SecurityType, 7);
                record.InstrumentClass   = ReadChar(body, layout.InstrumentClass);
                record.Action            = ReadChar(body, layout.SecurityUpdateAction);

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Definition record.");
            }
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

        /// <summary>
        /// Body offsets (after the 16-byte header) of the decoded fields in one DBN version's definition record. The leading block, from
        /// <c>ts_recv</c> to <c>max_price_variation</c>, is the same in every version and is read at fixed offsets.
        /// </summary>
        private sealed class DefinitionLayout
        {
            public const int V1RecordBytes = 360;
            public const int V3RecordBytes = 520;

            // v1 keeps trading_reference_price in the i64 block and strike_price after the strings; v3 drops the former and moves the latter up.
            public static readonly DefinitionLayout V1 = new DefinitionLayout
            {
                UnitOfMeasureQty = 72,
                StrikePrice = 312,
                RawSymbol = 184,
                RawSymbolWidth = 22,
                Exchange = 227,
                Asset = 232,
                AssetWidth = 7,
                Cfi = 239,
                SecurityType = 246,
                InstrumentClass = 309,
                SecurityUpdateAction = 333,
            };

            public static readonly DefinitionLayout V3 = new DefinitionLayout
            {
                UnitOfMeasureQty = 64,
                StrikePrice = 88,
                RawSymbol = 222,
                RawSymbolWidth = 71,
                Exchange = 314,
                Asset = 319,
                AssetWidth = 11,
                Cfi = 330,
                SecurityType = 337,
                InstrumentClass = 471,
                SecurityUpdateAction = 477,
            };

            public int UnitOfMeasureQty { get; private init; }

            public int StrikePrice { get; private init; }

            public int RawSymbol { get; private init; }

            public int RawSymbolWidth { get; private init; }

            public int Exchange { get; private init; }

            public int Asset { get; private init; }

            public int AssetWidth { get; private init; }

            public int Cfi { get; private init; }

            public int SecurityType { get; private init; }

            public int InstrumentClass { get; private init; }

            public int SecurityUpdateAction { get; private init; }
        }
    }
}
