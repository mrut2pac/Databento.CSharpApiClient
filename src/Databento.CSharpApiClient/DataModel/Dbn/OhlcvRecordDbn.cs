using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An OHLCV bar record deserialized from a DBN binary stream.
    /// Covers schemas <c>ohlcv-1s</c>, <c>ohlcv-1m</c>, <c>ohlcv-1h</c>,
    /// <c>ohlcv-1d</c>, and <c>ohlcv-eod</c>.
    /// Record body is 40 bytes; total record = 56 bytes (length_byte = 14).
    /// </summary>
    public sealed class OhlcvRecordDbn : IOhlcvRecord
    {
        /// <summary>DBN record-type discriminator (one of the <c>Ohlcv*</c> RType values).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this bar.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Timestamp of the bar's open (start of interval), in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Open price of the bar interval (display-scaled).</summary>
        public double Open { get; set; }

        /// <summary>Highest traded price within the bar interval (display-scaled).</summary>
        public double High { get; set; }

        /// <summary>Lowest traded price within the bar interval (display-scaled).</summary>
        public double Low { get; set; }

        /// <summary>Close price of the bar interval (display-scaled).</summary>
        public double Close { get; set; }

        /// <summary>Total traded volume within the bar interval (in lots).</summary>
        public ulong Volume { get; set; }

        /// <summary>
        /// Requested symbol this record belongs to, in the request's input symbology, from the symbol mappings in the stream's
        /// metadata; <see langword="null"/> when they do not map the record's instrument on its date.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Deserialises an OHLCV record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static OhlcvRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                OhlcvRecordDbn record = new OhlcvRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.Open   = Utils.NanoToDouble(body.ReadInt64());
                    record.High   = Utils.NanoToDouble(body.ReadInt64());
                    record.Low    = Utils.NanoToDouble(body.ReadInt64());
                    record.Close  = Utils.NanoToDouble(body.ReadInt64());
                    record.Volume = body.ReadUInt64();
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN OHLCV record.");
            }
        }

        /// <summary>
        /// Writes this record, header first, in the 56-byte DBN layout <see cref="ReadFromBytes"/> reads. Undefined values are
        /// written as DBN's undefined values, and nothing is written when a value is refused.
        /// </summary>
        /// <param name="writer">Writer to write the record to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A value is one the record's DBN field cannot hold.</exception>
        public void WriteToBytes(BinaryWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            DbnRecordWriter record = new DbnRecordWriter(stackalloc byte[56]);
            record.Header(this.RecordType, this.PublisherId, this.InstrumentId, this.TsEventUtc);
            record.WritePrice(this.Open, "open");
            record.WritePrice(this.High, "high");
            record.WritePrice(this.Low, "low");
            record.WritePrice(this.Close, "close");
            record.WriteUInt64(this.Volume);
            record.WriteTo(writer);
        }

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.TsEventUtc);
    }
}
