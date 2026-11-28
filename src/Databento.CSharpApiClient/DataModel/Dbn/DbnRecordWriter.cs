using System;
using System.Buffers.Binary;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Lays out one DBN record, little-endian, in a zeroed buffer of the record's length, so reserved bytes stay zero and a
    /// refused value leaves nothing written: the record reaches the <see cref="BinaryWriter"/> only once it is whole.
    /// </summary>
    internal ref struct DbnRecordWriter
    {
        private readonly Span<byte> record;

        private int position;

        public DbnRecordWriter(Span<byte> record)
        {
            this.record = record;
            this.position = 0;
        }

        /// <summary>Writes the 16-byte header, its length byte the record's length in 4-byte words.</summary>
        public void Header(RType recordType, ushort publisherId, uint instrumentId, DateTime tsEventUtc)
        {
            this.WriteByte((byte)(this.record.Length / 4));
            this.WriteByte((byte)recordType);
            this.WriteUInt16(publisherId);
            this.WriteUInt32(instrumentId);
            this.WriteUInt64(DbnFieldWriter.Timestamp(tsEventUtc, "ts_event"));
        }

        /// <summary>Moves to an offset of the body, counted from the end of the header, as the decoders' offsets are.</summary>
        public void Seek(int bodyOffset)
        {
            this.position = DbnRecordHeader.SizeBytes + bodyOffset;
        }

        public void Skip(int bytes)
        {
            this.position += bytes;
        }

        public void WriteByte(byte value)
        {
            this.record[this.position] = value;
            this.position += 1;
        }

        public void WriteUInt16(ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(this.record.Slice(this.position), value);
            this.position += 2;
        }

        public void WriteInt16(short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(this.record.Slice(this.position), value);
            this.position += 2;
        }

        public void WriteUInt32(uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(this.record.Slice(this.position), value);
            this.position += 4;
        }

        public void WriteInt32(int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(this.record.Slice(this.position), value);
            this.position += 4;
        }

        public void WriteUInt64(ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(this.record.Slice(this.position), value);
            this.position += 8;
        }

        public void WriteInt64(long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(this.record.Slice(this.position), value);
            this.position += 8;
        }

        public void WritePrice(double price, string field)
        {
            this.WriteInt64(DbnFieldWriter.Price(price, field));
        }

        public void WriteTimestamp(DateTime timestamp, string field)
        {
            this.WriteUInt64(DbnFieldWriter.Timestamp(timestamp, field));
        }

        public void WriteTimestamp(DateTime? timestamp, string field)
        {
            this.WriteUInt64(DbnFieldWriter.Timestamp(timestamp, field));
        }

        public void WriteSide(TradeAggressor side)
        {
            this.WriteByte(DbnFieldWriter.Side(side, "side"));
        }

        public void WriteCString(string value, int width, string field)
        {
            DbnFieldWriter.CString(this.record.Slice(this.position, width), value, field);
            this.position += width;
        }

        public readonly void WriteTo(BinaryWriter writer)
        {
            writer.Write(this.record);
        }
    }
}
