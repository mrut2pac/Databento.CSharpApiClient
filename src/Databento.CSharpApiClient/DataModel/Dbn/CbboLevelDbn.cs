using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single consolidated BBO price level used by TCBBO and CMBP-1 DBN records.
    /// Carries publisher IDs (bid_pb / ask_pb) instead of order counts.
    /// Each level occupies 32 bytes on the wire: each publisher ID is followed by 2 reserved bytes.
    /// </summary>
    public sealed class CbboLevelDbn
    {
        /// <summary>National best bid price (display-scaled).</summary>
        public double BidPrice { get; set; }

        /// <summary>National best ask price (display-scaled).</summary>
        public double AskPrice { get; set; }

        /// <summary>Quantity available at the national best bid (in lots).</summary>
        public uint BidSize { get; set; }

        /// <summary>Quantity available at the national best ask (in lots).</summary>
        public uint AskSize { get; set; }

        /// <summary>Publisher ID of the venue contributing the best bid.</summary>
        public ushort BidPublisherId { get; set; }

        /// <summary>Publisher ID of the venue contributing the best ask.</summary>
        public ushort AskPublisherId { get; set; }

        /// <summary>Reads one 32-byte consolidated BBO level from <paramref name="reader"/>.</summary>
        /// <param name="reader">Reader positioned at the start of the level.</param>
        public static CbboLevelDbn ReadFromBytes(BinaryReader reader)
        {
            try
            {
                CbboLevelDbn level = new CbboLevelDbn();
                level.BidPrice = Utils.NanoToDouble(reader.ReadInt64());
                level.AskPrice = Utils.NanoToDouble(reader.ReadInt64());
                level.BidSize = reader.ReadUInt32();
                level.AskSize = reader.ReadUInt32();
                level.BidPublisherId = reader.ReadUInt16();
                reader.ReadUInt16(); // reserved
                level.AskPublisherId = reader.ReadUInt16();
                reader.ReadUInt16(); // reserved
                return level;
            }
            catch(System.IO.EndOfStreamException)
            {
                throw new System.IO.InvalidDataException("Truncated DBN consolidated BBO level.");
            }
        }

        /// <summary>
        /// Writes this level in the 32-byte layout <see cref="ReadFromBytes"/> reads; a <see cref="double.NaN"/> price is written as
        /// DBN's undefined price, and nothing is written when a price is refused.
        /// </summary>
        /// <param name="writer">Writer to write the level to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A price is out of the range of a DBN price.</exception>
        public void WriteToBytes(BinaryWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            DbnRecordWriter level = new DbnRecordWriter(stackalloc byte[32]);
            this.Write(ref level);
            level.WriteTo(writer);
        }

        internal static CbboLevelDbn Required(CbboLevelDbn level, string field)
        {
            return level ?? throw DbnFieldWriter.Invalid(field, "null", "a record's level must be set");
        }

        internal void Write(ref DbnRecordWriter record)
        {
            record.WritePrice(this.BidPrice, "bid_px");
            record.WritePrice(this.AskPrice, "ask_px");
            record.WriteUInt32(this.BidSize);
            record.WriteUInt32(this.AskSize);
            record.WriteUInt16(this.BidPublisherId);
            record.Skip(2); // reserved
            record.WriteUInt16(this.AskPublisherId);
            record.Skip(2); // reserved
        }
    }
}
