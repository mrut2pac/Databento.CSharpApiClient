using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A consolidated market-by-price depth-1 record deserialized from a DBN binary stream.
    /// Schema: <c>cmbp-1</c> — rtype <c>Cmbp1</c> (0xB1).
    /// Binary layout is identical to TCBBO (<see cref="TcbboRecordDbn"/>); only the rtype differs.
    /// Record body is 60 bytes (32-byte event + 28-byte consolidated level); total record = 76 bytes (length_byte = 19).
    /// </summary>
    public sealed class Cmbp1RecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Cmbp1"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Order price (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Order quantity in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the triggering order.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the event occurred.</summary>
        public byte Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Consolidated NBBO level after this event.</summary>
        public CbboLevelDbn Level { get; set; }

        /// <summary>
        /// Deserialises a CMBP-1 record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static Cmbp1RecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                Cmbp1RecordDbn record = new Cmbp1RecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // Event section (32 bytes):
                    // price(8) + size(4) + action(1) + side(1) + flags(1) + depth(1)
                    // + ts_recv(8) + ts_in_delta(4) + sequence(4)
                    record.Price     = Utils.NanoToDouble(body.ReadInt64());
                    record.Size      = body.ReadUInt32();
                    record.Action    = (OrderBookAction)body.ReadByte();
                    record.Side      = Utils.ReadSide(body.ReadByte());
                    record.Flags     = (MessageInfoBits)body.ReadByte();
                    record.Depth     = body.ReadByte();
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence  = body.ReadUInt32();

                    // Consolidated BBO level (28 bytes)
                    record.Level = CbboLevelDbn.ReadFromBytes(body);
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN CMBP-1 record.");
            }
        }

    }
}
