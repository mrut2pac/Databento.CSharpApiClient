using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single consolidated BBO price level used by TCBBO and CMBP-1 DBN records.
    /// Carries publisher IDs (bid_pb / ask_pb) instead of order counts.
    /// Each level occupies exactly 28 bytes on the wire.
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

        /// <summary>Reads one 28-byte consolidated BBO level from <paramref name="reader"/>.</summary>
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
                level.AskPublisherId = reader.ReadUInt16();
                return level;
            }
            catch(System.IO.EndOfStreamException)
            {
                throw new System.IO.InvalidDataException("Truncated DBN consolidated BBO level.");
            }
        }
    }
}
