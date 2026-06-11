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
        public RType RecordType { get; set; }

        public ushort PublisherId { get; set; }

        public uint InstrumentId { get; set; }

        public DateTime TsEventUtc { get; set; }

        public double Price { get; set; }

        public uint Size { get; set; }

        public OrderBookAction Action { get; set; }

        public TradeAggressor Side { get; set; }

        public MessageInfoBits Flags { get; set; }

        public uint Depth { get; set; }

        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond delta from venue receipt to gateway receipt.</summary>
        public long TsInDelta { get; set; }

        public uint Sequence { get; set; }

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

                using(MemoryStream ms = new MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.Price = Utils.NanoToDouble(body.ReadInt64());     // 8
                    record.Size = body.ReadUInt32();                          // 4
                    record.Action = (OrderBookAction)body.ReadByte();         // 1
                    record.Side = ReadSide(body.ReadByte());                  // 1
                    record.Flags = (MessageInfoBits)body.ReadByte();          // 1
                    record.Depth = body.ReadByte();                           // 1
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime; // 8
                    record.TsInDelta = body.ReadInt32();                      // 4
                    record.Sequence = body.ReadUInt32();                      // 4
                    // any remaining bytes are ts_out (8 bytes) or padding — skipped
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
