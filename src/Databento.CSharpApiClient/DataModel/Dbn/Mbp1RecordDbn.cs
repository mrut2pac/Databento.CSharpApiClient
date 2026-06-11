using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A Market-by-Price depth-1 record deserialized from a DBN binary stream.
    /// Schema: <c>mbp-1</c> — rtype <c>Mbp1</c> (0x01). Each record captures a single
    /// order-book event (add/modify/delete/trade) plus the resulting best bid and offer.
    /// </summary>
    public sealed class Mbp1RecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Mbp1"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Order price (converted from nano-integer to double).</summary>
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
        public long TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Best bid price after applying this event.</summary>
        public double BidPrice { get; set; }

        /// <summary>Best ask price after applying this event.</summary>
        public double AskPrice { get; set; }

        /// <summary>Quantity available at the best bid.</summary>
        public uint BidSize { get; set; }

        /// <summary>Quantity available at the best ask.</summary>
        public uint AskSize { get; set; }

        /// <summary>Number of orders resting at the best bid.</summary>
        public uint BidCount { get; set; }

        /// <summary>Number of orders resting at the best ask.</summary>
        public uint AskCount { get; set; }

        /// <summary>
        /// Deserialises an MBP-1 record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static Mbp1RecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                Mbp1RecordDbn record = new Mbp1RecordDbn();
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
                    record.BidPrice = Utils.NanoToDouble(body.ReadInt64());
                    record.AskPrice = Utils.NanoToDouble(body.ReadInt64());
                    record.BidSize = body.ReadUInt32();
                    record.AskSize = body.ReadUInt32();
                    record.BidCount = body.ReadUInt32();
                    record.AskCount = body.ReadUInt32();
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN MBP-1 record.");
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
