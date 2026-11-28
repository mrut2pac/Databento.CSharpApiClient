using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A venue-local Best Bid and Offer snapshot record deserialized from a DBN binary stream.
    /// Covers schemas <c>bbo-1s</c> (rtype <c>Bbo1S</c> = 0xC3) and <c>bbo-1m</c> (rtype <c>Bbo1M</c> = 0xC4).
    /// Record body is 64 bytes; total record = 80 bytes (length_byte = 20).
    /// </summary>
    public sealed class BboRecordDbn : IBboRecord
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Bbo1S"/> or <see cref="RType.Bbo1M"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this snapshot.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Reference price at the time of the snapshot (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Reference size in lots at the time of the snapshot.</summary>
        public uint Size { get; set; }

        /// <summary>Side of the reference order.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Always 0: the BBO records have no ts_in_delta; DBN reserves these bytes.</summary>
        [Obsolete("The bbo-1s and bbo-1m records have no ts_in_delta, so this is always 0.")]
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>Best bid and ask price level at the time of the snapshot.</summary>
        public Mbp1LevelDbn Level { get; set; }

        /// <summary>
        /// Requested symbol this record belongs to, in the request's input symbology, from the symbol mappings in the stream's
        /// metadata; <see langword="null"/> when they do not map the record's instrument on its date.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Deserialises a BBO record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static BboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                BboRecordDbn record = new BboRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // Layout (64 bytes):
                    // price(8) + size(4) + _reserved(1) + side(1) + flags(1) + _reserved(1)
                    // + ts_recv(8) + _reserved(4) + sequence(4) + Mbp1Level(32)
                    record.Price     = Utils.NanoToDouble(body.ReadInt64());
                    record.Size      = body.ReadUInt32();
                    body.ReadByte();  // _reserved (action slot)
                    record.Side      = Utils.ReadSide(body.ReadByte());
                    record.Flags     = (MessageInfoBits)body.ReadByte();
                    body.ReadByte();  // _reserved (depth slot)
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    body.ReadInt32();  // _reserved
                    record.Sequence  = body.ReadUInt32();
                    record.Level     = Mbp1LevelDbn.ReadFromBytes(body);
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN BBO record.");
            }
        }

        /// <summary>
        /// Writes this record, header first, in the 80-byte DBN layout <see cref="ReadFromBytes"/> reads. Undefined values are
        /// written as DBN's undefined values, and nothing is written when a value is refused.
        /// </summary>
        /// <param name="writer">Writer to write the record to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A value is one the record's DBN field cannot hold.</exception>
        public void WriteToBytes(BinaryWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            DbnRecordWriter record = new DbnRecordWriter(stackalloc byte[80]);
            record.Header(this.RecordType, this.PublisherId, this.InstrumentId, this.TsEventUtc);
            record.WritePrice(this.Price, "price");
            record.WriteUInt32(this.Size);
            record.Skip(1); // reserved (action slot)
            record.WriteSide(this.Side);
            record.WriteByte((byte)this.Flags);
            record.Skip(1); // reserved (depth slot)
            record.WriteTimestamp(this.TsReceivedUtc, "ts_recv");
            record.Skip(4); // reserved
            record.WriteUInt32(this.Sequence);
            Mbp1LevelDbn.Required(this.Level, "level").Write(ref record);
            record.WriteTo(writer);
        }

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.TsEventUtc);

        double? IBboRecord.Price => UndefinedValues.Price(this.Price);

        double? IBboRecord.BidPrice => UndefinedValues.Price(this.Level?.BidPrice);

        double? IBboRecord.AskPrice => UndefinedValues.Price(this.Level?.AskPrice);

        uint IBboRecord.BidSize => this.Level?.BidSize ?? 0;

        uint IBboRecord.AskSize => this.Level?.AskSize ?? 0;

        uint IBboRecord.BidCount => this.Level?.BidCount ?? 0;

        uint IBboRecord.AskCount => this.Level?.AskCount ?? 0;

        DateTime? IBboRecord.TsReceivedUtc => UndefinedValues.Timestamp(this.TsReceivedUtc);
    }
}
