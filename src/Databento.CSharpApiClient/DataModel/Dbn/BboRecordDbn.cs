using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A venue-local Best Bid and Offer snapshot record deserialized from a DBN binary stream.
    /// Covers schemas <c>bbo-1s</c> (rtype <c>Bbo1S</c> = 0xC3) and <c>bbo-1m</c> (rtype <c>Bbo1M</c> = 0xC4).
    /// Record body is 64 bytes; total record = 80 bytes (length_byte = 20).
    /// </summary>
    public sealed class BboRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Bbo1S"/> or <see cref="RType.Bbo1M"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this snapshot.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Reference price at the time of the snapshot (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Reference size in lots at the time of the snapshot.</summary>
        public uint Size { get; set; }

        /// <summary>Side of the reference order.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Best bid and ask price level at the time of the snapshot.</summary>
        public Mbp1LevelDbn Level { get; set; }

        /// <summary>
        /// Deserialises a BBO record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static BboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                BboRecordDbn record = new BboRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // Layout (64 bytes):
                    // price(8) + size(4) + _reserved(1) + side(1) + flags(1) + _reserved(1)
                    // + ts_recv(8) + ts_in_delta(4) + sequence(4) + Mbp1Level(32)
                    record.Price     = Utils.NanoToDouble(body.ReadInt64());
                    record.Size      = body.ReadUInt32();
                    body.ReadByte();  // _reserved (action slot)
                    record.Side      = ReadSide(body.ReadByte());
                    record.Flags     = (MessageInfoBits)body.ReadByte();
                    body.ReadByte();  // _reserved (depth slot)
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence  = body.ReadUInt32();
                    record.Level     = Mbp1LevelDbn.ReadFromBytes(body);
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN BBO record.");
            }
        }

        private static TradeAggressor ReadSide(byte side)
        {
            switch((char)side)
            {
                case 'A': return TradeAggressor.Seller;
                case 'B': return TradeAggressor.Buyer;
                default:  return TradeAggressor.None;
            }
        }
    }
}
