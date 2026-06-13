using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single venue-local price level used by MBP-10, BBO, and TBBO DBN records.
    /// Each level occupies exactly 32 bytes on the wire.
    /// </summary>
    public sealed class Mbp1LevelDbn
    {
        /// <summary>Best bid price at this level (display-scaled).</summary>
        public double BidPrice { get; set; }

        /// <summary>Best ask price at this level (display-scaled).</summary>
        public double AskPrice { get; set; }

        /// <summary>Quantity available at the best bid (in lots).</summary>
        public uint BidSize { get; set; }

        /// <summary>Quantity available at the best ask (in lots).</summary>
        public uint AskSize { get; set; }

        /// <summary>Number of orders resting at the best bid.</summary>
        public uint BidCount { get; set; }

        /// <summary>Number of orders resting at the best ask.</summary>
        public uint AskCount { get; set; }

        /// <summary>Reads one 32-byte BBO level from <paramref name="reader"/>.</summary>
        /// <param name="reader">Reader positioned at the start of the level.</param>
        public static Mbp1LevelDbn ReadFromBytes(BinaryReader reader)
        {
            Mbp1LevelDbn level = new Mbp1LevelDbn();
            level.BidPrice = Utils.NanoToDouble(reader.ReadInt64());
            level.AskPrice = Utils.NanoToDouble(reader.ReadInt64());
            level.BidSize = reader.ReadUInt32();
            level.AskSize = reader.ReadUInt32();
            level.BidCount = reader.ReadUInt32();
            level.AskCount = reader.ReadUInt32();
            return level;
        }
    }
}
