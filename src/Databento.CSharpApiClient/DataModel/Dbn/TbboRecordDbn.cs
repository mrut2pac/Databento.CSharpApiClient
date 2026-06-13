using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A trade-plus-venue-BBO record deserialized from a DBN binary stream.
    /// Schema: <c>tbbo</c> — rtype <c>Tbbo</c> (0x03).
    /// Binary layout is identical to MBP-1 (<see cref="Mbp1RecordDbn"/>); only the rtype differs.
    /// Record body is 64 bytes; total record = 80 bytes (length_byte = 20).
    /// </summary>
    public sealed class TbboRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Tbbo"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Trade price (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Trade quantity in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Order-book action (typically Trade).</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side of the trade.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the trade occurred.</summary>
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Best bid and ask venue BBO immediately following the trade.</summary>
        public Mbp1LevelDbn Level { get; set; }

        /// <summary>
        /// Deserialises a TBBO record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static TbboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                TbboRecordDbn record = new TbboRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // Layout identical to MBP-1 (64 bytes):
                    // price(8) + size(4) + action(1) + side(1) + flags(1) + depth(1)
                    // + ts_recv(8) + ts_in_delta(4) + sequence(4) + Mbp1Level(32)
                    record.Price     = Utils.NanoToDouble(body.ReadInt64());
                    record.Size      = body.ReadUInt32();
                    record.Action    = (OrderBookAction)body.ReadByte();
                    record.Side      = ReadSide(body.ReadByte());
                    record.Flags     = (MessageInfoBits)body.ReadByte();
                    record.Depth     = body.ReadByte();
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence  = body.ReadUInt32();
                    record.Level     = Mbp1LevelDbn.ReadFromBytes(body);
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN TBBO record.");
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
