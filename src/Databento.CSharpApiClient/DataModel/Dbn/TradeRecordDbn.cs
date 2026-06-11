using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single trade tick deserialized from a DBN binary stream.
    /// Schema: <c>trades</c> — rtype <c>Mbp0</c> (0x00).
    /// </summary>
    public sealed class TradeRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Mbp0"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this trade.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp (trade time) at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Trade price (converted from nano-integer to double).</summary>
        public double Price { get; set; }

        /// <summary>Trade size in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Order-book action; always <see cref="OrderBookAction.Trade"/> for this schema.</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side of the trade.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the trade occurred (usually 0).</summary>
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public long TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>
        /// Deserialises a trade record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static TradeRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                TradeRecordDbn record = new TradeRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.Price = Utils.NanoToDouble(body.ReadInt64());
                    record.Size = body.ReadUInt32();
                    record.Action = (OrderBookAction)body.ReadByte();
                    record.Side = ReadSide(body.ReadByte());
                    record.Flags = (MessageInfoBits)body.ReadByte();
                    record.Depth = body.ReadByte();
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence = body.ReadUInt32();
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN trade record.");
            }
        }

        private static TradeAggressor ReadSide(byte side)
        {
            switch((char)side)
            {
                case 'A': return TradeAggressor.Seller;
                case 'B': return TradeAggressor.Buyer;
                default: return TradeAggressor.None;
            }
        }
    }
}
