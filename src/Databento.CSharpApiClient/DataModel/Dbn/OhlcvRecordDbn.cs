using System;
using System.IO;

using Databento.CSharpApiClient.DataModel.Json;

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
        /// Returns this record as an <see cref="OhlcvRecordJson"/>, with the values <see cref="DatabentoJsonClient"/> reads from the JSON
        /// encoding of the same record. An undefined event time becomes <see langword="null"/> and an undefined price stays
        /// <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        /// <exception cref="OverflowException">The volume is beyond <see cref="long.MaxValue"/>, which the JSON model can't hold.</exception>
        public OhlcvRecordJson ToJsonRecord()
        {
            return new OhlcvRecordJson
            {
                Header = DbnRecordHeader.ToJsonHeader(this.RecordType, this.PublisherId, this.InstrumentId, this.TsEventUtc),
                Open = this.Open,
                High = this.High,
                Low = this.Low,
                Close = this.Close,
                Volume = checked((long)this.Volume),
                Symbol = this.Symbol,
            };
        }

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.TsEventUtc);
    }
}
