using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    public class DbnRecordHeader
    {
        /// <summary>
        /// Size in bytes of the fixed DBN record header (length, rtype, publisher_id, instrument_id, ts_event).
        /// The header's <c>length</c> field encodes the TOTAL record size (header + body), so the body size
        /// is <see cref="RecordLength"/> minus this value.
        /// </summary>
        public const int SizeBytes = 16;

        public int RecordLength { get; set; }

        public RType RecordType { get; set; }

        public ushort PublisherId { get; set; }

        public uint InstrumentId { get; set; }

        public DateTime TsEventUtc { get; set; }

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
