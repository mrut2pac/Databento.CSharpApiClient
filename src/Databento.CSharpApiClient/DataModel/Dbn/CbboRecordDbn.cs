using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Consolidated/National Best Bid/Offer snapshot (CBBO-1s / CBBO-1m).
    /// </summary>
    public sealed class CbboRecordDbn
    {
        public RType RecordType { get; set; }

        public ushort PublisherId { get; set; }

        public uint InstrumentId { get; set; }

        public DateTime TsEventUtc { get; set; }

        public DateTime TsReceivedUtc { get; set; }

        public TradeAggressor Side { get; set; }

        public double Price { get; set; }

        public uint Size { get; set; }

        public MessageInfoBits Flags { get; set; }

        public double BidPrice { get; set; }

        public double AskPrice { get; set; }

        public uint BidSize { get; set; }

        public uint AskSize { get; set; }

        /// <summary>
        /// Publisher ID of venue contributing the best bid.
        /// </summary>
        public ushort BidPublisherId { get; set; }

        /// <summary>
        /// Publisher ID of venue contributing the best ask.
        /// </summary>
        public ushort AskPublisherId { get; set; }

        /// <summary>
        /// Deserializes the object from the specified bytes array
        /// </summary>
        /// <param name="header">Read header object</param>
        /// <param name="binaryReader">Binary reader</param>
        /// <returns>Deserialized object</returns>
        public static CbboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader binaryReader)
        {
            try
            {
                CbboRecordDbn cbboRecord = new CbboRecordDbn();

                // get from the header
                cbboRecord.RecordType = header.RecordType;
                cbboRecord.PublisherId = header.PublisherId;
                cbboRecord.InstrumentId = header.InstrumentId;
                cbboRecord.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = binaryReader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(MemoryStream memoryStream = new MemoryStream(bodyBytes, writable: false))
                using(BinaryReader bodyBytesReader = new BinaryReader(memoryStream))
                {
                    // read from the body
                    cbboRecord.Price = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.Size = bodyBytesReader.ReadUInt32();
                    bodyBytesReader.ReadByte();
                    cbboRecord.Side = ReadSide(bodyBytesReader.ReadByte());
                    cbboRecord.Flags = (MessageInfoBits)bodyBytesReader.ReadByte();
                    bodyBytesReader.ReadByte(); // skip 1 byte
                    cbboRecord.TsReceivedUtc = Utils.FromUnixNs(bodyBytesReader.ReadUInt64()).UtcDateTime;
                    cbboRecord.BidPrice = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.AskPrice = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.BidSize = bodyBytesReader.ReadUInt32();
                    cbboRecord.AskSize = bodyBytesReader.ReadUInt32();
                    cbboRecord.BidPublisherId = bodyBytesReader.ReadUInt16();
                    cbboRecord.AskPublisherId = bodyBytesReader.ReadUInt16();

                    // here might be also tsOut or not, we will skip it
                }

                return cbboRecord;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Couldn't read CBBO record but consumed all input bytes!");
            }
        }

        private static TradeAggressor ReadSide(byte side)
        {
            switch((char)side)
            {
                case 'A': return TradeAggressor.Seller;
                case 'B': return TradeAggressor.Buyer;
                case 'N': return TradeAggressor.None;
                default: throw new InvalidDataException("Unexpected side value: " + side);
            }
        }
    }
}
