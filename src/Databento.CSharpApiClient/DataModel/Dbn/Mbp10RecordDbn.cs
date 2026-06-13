using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A Market-by-Price depth-10 record deserialized from a DBN binary stream.
    /// Schema: <c>mbp-10</c> — rtype <c>Mbp10</c> (0x0A).
    /// Record body is 352 bytes (32-byte event + 10 × 32-byte level); total record = 368 bytes (length_byte = 92).
    /// </summary>
    public sealed class Mbp10RecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Mbp10"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Order price that triggered this event (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Order quantity in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the triggering order.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the event occurred (0 = top of book).</summary>
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Top 10 price levels resulting from this event (index 0 = best bid/ask).</summary>
        public Mbp1LevelDbn[] Levels { get; set; }

        /// <summary>
        /// Deserialises an MBP-10 record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static Mbp10RecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                Mbp10RecordDbn record = new Mbp10RecordDbn();
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
                    record.Side      = ReadSide(body.ReadByte());
                    record.Flags     = (MessageInfoBits)body.ReadByte();
                    record.Depth     = body.ReadByte();
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence  = body.ReadUInt32();

                    // 10 price levels (32 bytes each)
                    record.Levels = new Mbp1LevelDbn[10];
                    for(int i = 0; i < 10; i++)
                    {
                        record.Levels[i] = Mbp1LevelDbn.ReadFromBytes(body);
                    }
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN MBP-10 record.");
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
