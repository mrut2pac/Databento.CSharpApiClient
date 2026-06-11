using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A Market-by-Price depth-1 record deserialized from a DBN binary stream.
    /// Schema: <c>mbp-1</c> — rtype <c>Mbp1</c> (0x01).
    /// </summary>
    public sealed class Mbp1RecordDbn
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

        public long TsInDelta { get; set; }

        public uint Sequence { get; set; }

        // BidAskPair level
        public double BidPrice { get; set; }

        public double AskPrice { get; set; }

        public uint BidSize { get; set; }

        public uint AskSize { get; set; }

        public uint BidCount { get; set; }

        public uint AskCount { get; set; }

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
                    // BidAskPair: bid_px ask_px bid_sz ask_sz bid_ct ask_ct
                    record.BidPrice = Utils.NanoToDouble(body.ReadInt64());   // 8
                    record.AskPrice = Utils.NanoToDouble(body.ReadInt64());   // 8
                    record.BidSize = body.ReadUInt32();                       // 4
                    record.AskSize = body.ReadUInt32();                       // 4
                    record.BidCount = body.ReadUInt32();                      // 4
                    record.AskCount = body.ReadUInt32();                      // 4
                    // any remaining bytes are ts_out or padding — skipped
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
