using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Consolidated/National Best Bid and Offer (CBBO) snapshot record, deserialized from
    /// a DBN binary stream. Corresponds to schemas <c>cbbo-1s</c> and <c>cbbo-1m</c>.
    /// </summary>
    public sealed class CbboRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Cbbo1S"/> or <see cref="RType.Cbbo1M"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that contributed this snapshot.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Aggressor side of the last trade that triggered this update.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Last trade price (converted from nano-integer to double).</summary>
        public double Price { get; set; }

        /// <summary>Last trade size in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>National best bid price.</summary>
        public double BidPrice { get; set; }

        /// <summary>National best ask price.</summary>
        public double AskPrice { get; set; }

        /// <summary>Quantity available at the national best bid.</summary>
        public uint BidSize { get; set; }

        /// <summary>Quantity available at the national best ask.</summary>
        public uint AskSize { get; set; }

        /// <summary>Publisher ID of the venue contributing the best bid.</summary>
        public ushort BidPublisherId { get; set; }

        /// <summary>Publisher ID of the venue contributing the best ask.</summary>
        public ushort AskPublisherId { get; set; }

        /// <summary>
        /// Deserialises a CBBO record body from <paramref name="binaryReader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="binaryReader">Reader positioned immediately after the header bytes.</param>
        public static CbboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader binaryReader)
        {
            try
            {
                CbboRecordDbn cbboRecord = new CbboRecordDbn();

                cbboRecord.RecordType = header.RecordType;
                cbboRecord.PublisherId = header.PublisherId;
                cbboRecord.InstrumentId = header.InstrumentId;
                cbboRecord.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = binaryReader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader bodyBytesReader = new BinaryReader(memoryStream))
                {
                    cbboRecord.Price = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.Size = bodyBytesReader.ReadUInt32();
                    bodyBytesReader.ReadByte();
                    cbboRecord.Side = ReadSide(bodyBytesReader.ReadByte());
                    cbboRecord.Flags = (MessageInfoBits)bodyBytesReader.ReadByte();
                    bodyBytesReader.ReadByte();
                    cbboRecord.TsReceivedUtc = Utils.FromUnixNs(bodyBytesReader.ReadUInt64()).UtcDateTime;
                    cbboRecord.BidPrice = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.AskPrice = Utils.NanoToDouble(bodyBytesReader.ReadInt64());
                    cbboRecord.BidSize = bodyBytesReader.ReadUInt32();
                    cbboRecord.AskSize = bodyBytesReader.ReadUInt32();
                    cbboRecord.BidPublisherId = bodyBytesReader.ReadUInt16();
                    cbboRecord.AskPublisherId = bodyBytesReader.ReadUInt16();
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
