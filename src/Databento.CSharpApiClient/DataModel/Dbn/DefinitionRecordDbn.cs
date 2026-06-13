using System;
using System.IO;
using System.Text;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An instrument-definition record deserialized from a DBN binary stream.
    /// Schema: <c>definition</c> — rtype <c>InstrumentDef</c> (0x13).
    /// This is a partial decoder: key pricing and identification fields are decoded;
    /// the remaining body bytes (v2 total body ≈ 500 bytes) are silently dropped since
    /// the full body is buffered before parsing begins.
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

        /// <summary>Option strike price (display-scaled). Zero for non-option instruments.</summary>
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
        /// Deserialises a definition record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>. Decodes the key fields and silently
        /// drops any remaining body bytes beyond what is decoded here.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static DefinitionRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                DefinitionRecordDbn record = new DefinitionRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                // Buffer the entire body; the sub-reader may stop before the end — that is safe.
                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // ── i64 block (13 × 8 = 104 bytes) ──────────────────────────
                    record.TsReceivedUtc    = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.MinPriceIncrement = Utils.NanoToDouble(body.ReadInt64());
                    record.DisplayFactor     = Utils.NanoToDouble(body.ReadInt64());

                    ulong expNs = body.ReadUInt64();
                    record.Expiration = expNs == 0 ? (DateTime?)null : Utils.FromUnixNs(expNs).UtcDateTime;

                    ulong actNs = body.ReadUInt64();
                    record.Activation = actNs == 0 ? (DateTime?)null : Utils.FromUnixNs(actNs).UtcDateTime;

                    record.HighLimitPrice   = Utils.NanoToDouble(body.ReadInt64());
                    record.LowLimitPrice    = Utils.NanoToDouble(body.ReadInt64());
                    record.MaxPriceVariation = Utils.NanoToDouble(body.ReadInt64());
                    body.ReadInt64();  // trading_reference_price
                    record.UnitOfMeasureQty  = Utils.NanoToDouble(body.ReadInt64());
                    body.ReadInt64();  // min_price_increment_amount
                    body.ReadInt64();  // price_ratio
                    record.StrikePrice = Utils.NanoToDouble(body.ReadInt64());

                    // ── i32/u32 block (14 × 4 = 56 bytes) ─ skip ────────────────
                    body.ReadBytes(56);

                    // ── u16 block (5 × 2 = 10 bytes) ─ skip ─────────────────────
                    body.ReadBytes(10);

                    // ── string fields ────────────────────────────────────────────
                    body.ReadBytes(4);  // currency[4] - skip
                    body.ReadBytes(10); // settl_currency[4] + secsubtype[6] - skip
                    record.RawSymbol    = ReadAscii(body, 22);
                    body.ReadBytes(21); // group[21] - skip
                    record.Exchange     = ReadAscii(body, 5);
                    record.Asset        = ReadAscii(body, 7);
                    record.Cfi          = ReadAscii(body, 7);
                    record.SecurityType = ReadAscii(body, 7);
                    body.ReadBytes(73); // unit_of_measure[31] + underlying[21] + related[21] - skip

                    // ── single-byte fields ───────────────────────────────────────
                    body.ReadBytes(7);  // match_algorithm through underlying_product - skip
                    record.Action = ReadChar(body); // security_update_action
                    body.ReadBytes(7);  // maturity_month/day/week + user_defined_instrument + multiplier_unit + flow_schedule_type + tick_rule - skip
                    record.InstrumentClass = ReadChar(body);
                    // remaining body bytes dropped when MemoryStream is disposed
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Definition record.");
            }
        }

        private static string ReadAscii(BinaryReader r, int width)
        {
            byte[] buf = r.ReadBytes(width);
            int end = Array.IndexOf(buf, (byte)0);
            if(end < 0)
            {
                end = width;
            }

            while(end > 0 && buf[end - 1] == (byte)' ')
            {
                end--;
            }

            return end == 0 ? string.Empty : Encoding.ASCII.GetString(buf, 0, end);
        }

        private static char? ReadChar(BinaryReader r)
        {
            byte b = r.ReadByte();
            return b == 0 ? (char?)null : (char)b;
        }
    }
}
