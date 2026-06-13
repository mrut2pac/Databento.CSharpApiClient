using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An exchange trading-status message deserialized from a DBN binary stream.
    /// Schema: <c>status</c> — rtype <c>Status</c> (0x12).
    /// Record body is 24 bytes; total record = 40 bytes (length_byte = 10).
    /// </summary>
    public sealed class StatusRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Status"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this status message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>
        /// Trading-status action code. Common values: 0=None, 1=PreOpen, 2=PreCross,
        /// 3=Quoting, 4=Cross, 5=Rotation, 6=NewPriceIndication, 7=Trading,
        /// 8=Halt, 9=TradingRangeIndication.
        /// </summary>
        public ushort Action { get; set; }

        /// <summary>
        /// Reason code for the status change. Exchange-defined; varies by venue.
        /// </summary>
        public ushort Reason { get; set; }

        /// <summary>
        /// Exchange-specific trading event code that triggered this status.
        /// </summary>
        public ushort TradingEvent { get; set; }

        /// <summary><see langword="true"/> if trading is currently active for this instrument.</summary>
        public bool IsTrading { get; set; }

        /// <summary><see langword="true"/> if quoting is currently permitted for this instrument.</summary>
        public bool IsQuoting { get; set; }

        /// <summary><see langword="true"/> if short-sell restrictions are in effect.</summary>
        public bool IsShortSellRestricted { get; set; }

        /// <summary>
        /// Deserialises a status record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static StatusRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                StatusRecordDbn record = new StatusRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // Layout (24 bytes):
                    // ts_recv(8) + action(2) + reason(2) + trading_event(2)
                    // + is_trading(1) + is_quoting(1) + is_short_sell_restricted(1) + _reserved(7)
                    record.TsReceivedUtc          = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.Action                 = body.ReadUInt16();
                    record.Reason                 = body.ReadUInt16();
                    record.TradingEvent           = body.ReadUInt16();
                    record.IsTrading              = body.ReadByte() == (byte)'Y';
                    record.IsQuoting              = body.ReadByte() == (byte)'Y';
                    record.IsShortSellRestricted  = body.ReadByte() == (byte)'Y';
                    body.ReadBytes(7); // _reserved
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Status record.");
            }
        }
    }
}
