using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Fixed 16-byte header that precedes every DBN record body.
    /// All DBN record types share this layout; the <see cref="RecordType"/> byte
    /// identifies the concrete record that follows.
    /// </summary>
    public class DbnRecordHeader
    {
        /// <summary>
        /// Size in bytes of the fixed DBN record header (length, rtype, publisher_id, instrument_id, ts_event).
        /// The header's <c>length</c> field encodes the TOTAL record size (header + body), so the body size
        /// is <see cref="RecordLength"/> minus this value.
        /// </summary>
        public const int SizeBytes = 16;

        /// <summary>
        /// Total record size in bytes (header + body), derived from the 1-byte
        /// <c>length</c> field multiplied by 4.
        /// </summary>
        public int RecordLength { get; set; }

        /// <summary>DBN record-type discriminator; determines the body layout.</summary>
        public RType RecordType { get; set; }

        /// <summary>
        /// Databento publisher identifier. Resolve to venue name via
        /// <c>metadata.list_publishers</c>.
        /// </summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier (uint32).</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, converted to UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>
        /// Deserialises a record header from <paramref name="binaryReader"/>.
        /// Returns <see langword="null"/> at end of stream.
        /// </summary>
        /// <param name="binaryReader">Reader positioned at the start of a record header.</param>
        public static DbnRecordHeader ReadFromBytes(BinaryReader binaryReader)
        {
            try
            {
                DbnRecordHeader header = new DbnRecordHeader();
                header.RecordLength = binaryReader.ReadByte() * 4;
                header.RecordType = (RType)binaryReader.ReadByte();
                header.PublisherId = binaryReader.ReadUInt16();
                header.InstrumentId = binaryReader.ReadUInt32();
                header.TsEventUtc = Utils.FromUnixNs(binaryReader.ReadUInt64()).UtcDateTime;
                return header;
            }
            catch(EndOfStreamException)
            {
                return null;
            }
        }
    }
}
