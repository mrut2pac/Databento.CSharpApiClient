using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A publisher-statistics record deserialized from a DBN binary stream.
    /// Schema: <c>statistics</c> — rtype <c>Statistics</c> (0x18).
    /// Carries settlement price, open interest, and other per-instrument statistics.
    /// Decodes both layouts by record length: DBN v3 (80 bytes, 64-bit quantity) and the earlier 32-bit-quantity layout.
    /// </summary>
    public sealed class StatisticsRecordDbn : IStatisticsRecord
    {
        // Body size of a DBN v3 Statistics record: 80 bytes less the 16-byte header.
        private const int StatisticsV3BodyBytes = 64;

        /// <summary>DBN record-type discriminator (<see cref="RType.Statistics"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this statistic.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Reference timestamp associated with the statistic (e.g. session open), in UTC.</summary>
        public DateTime? TsRefUtc { get; set; }

        /// <summary>Statistic price value (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>
        /// Statistic quantity value (e.g. open interest). When the statistic carries no quantity it is <see cref="long.MaxValue"/> in DBN v3
        /// records and <see cref="int.MaxValue"/> in the older 32-bit layout.
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>Venue sequence number.</summary>
        public uint Sequence { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>
        /// Statistic type identifier.
        /// Common values: 1=OpeningPrice, 2=IndicativeOpeningPrice, 3=SettlementPrice,
        /// 4=TradingSessionHighPrice, 5=TradingSessionLowPrice, 6=ClearedVolume,
        /// 7=LowestOffer, 8=HighestBid, 9=OpenInterest, 10=FixingPrice.
        /// </summary>
        public ushort StatType { get; set; }

        /// <summary>Publisher channel identifier.</summary>
        public ushort ChannelId { get; set; }

        /// <summary>Action that generated this update: 1=Add, 2=Delete.</summary>
        public byte UpdateAction { get; set; }

        /// <summary>Statistics message info flags.</summary>
        public MessageInfoBits StatFlags { get; set; }

        /// <summary>
        /// Requested symbol this record belongs to, in the request's input symbology, from the symbol mappings in the stream's
        /// metadata; <see langword="null"/> when they do not map the record's instrument on its date.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Deserialises a statistics record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static StatisticsRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                StatisticsRecordDbn record = new StatisticsRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    ulong tsRefNs = body.ReadUInt64();
                    // u64::MAX is DBN's undefined timestamp, which the JSON encoding reports as null too
                    record.TsRefUtc = tsRefNs == 0 || tsRefNs == ulong.MaxValue ? (DateTime?)null : Utils.FromUnixNs(tsRefNs).UtcDateTime;
                    record.Price       = Utils.NanoToDouble(body.ReadInt64());
                    // DBN v3 widened quantity to 64 bits (80-byte record); reading it as 32 bits would shift every field after it
                    record.Quantity    = bodyBytes.Length >= StatisticsV3BodyBytes ? body.ReadInt64() : body.ReadInt32();
                    record.Sequence    = body.ReadUInt32();
                    record.TsInDelta   = body.ReadInt32();
                    record.StatType    = body.ReadUInt16();
                    record.ChannelId   = body.ReadUInt16();
                    record.UpdateAction = body.ReadByte();
                    record.StatFlags   = (MessageInfoBits)body.ReadByte();
                    body.ReadBytes(2); // _reserved
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Statistics record.");
            }
        }

        /// <summary>
        /// Writes this record, header first, in the layout <see cref="ReadFromBytes"/> reads for the DBN version: 64 bytes with a
        /// 32-bit quantity in v1 and v2, 80 bytes with a 64-bit quantity in v3. The undefined quantity is written as the version's:
        /// <see cref="long.MaxValue"/> as <see cref="int.MaxValue"/> in v1 and v2, and <see cref="int.MaxValue"/> as <see cref="long.MaxValue"/>
        /// in v3, as Databento converts between them. Nothing is written when a value is refused.
        /// </summary>
        /// <param name="writer">Writer to write the record to.</param>
        /// <param name="dbnVersion">DBN version of the stream: 1, 2 or 3.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dbnVersion"/> is not 1, 2 or 3.</exception>
        /// <exception cref="ArgumentException">A value is one the record's DBN field cannot hold, such as a quantity beyond 32 bits in v1 or v2.</exception>
        public void WriteToBytes(BinaryWriter writer, byte dbnVersion)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentOutOfRangeException.ThrowIfZero(dbnVersion);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(dbnVersion, (byte)3);

            bool wideQuantity = dbnVersion == 3;
            DbnRecordWriter record = new DbnRecordWriter(stackalloc byte[wideQuantity ? DbnRecordHeader.SizeBytes + StatisticsV3BodyBytes : 64]);
            record.Header(this.RecordType, this.PublisherId, this.InstrumentId, this.TsEventUtc);
            record.WriteTimestamp(this.TsReceivedUtc, "ts_recv");
            record.WriteTimestamp(this.TsRefUtc, "ts_ref");
            record.WritePrice(this.Price, "price");
            if(wideQuantity)
            {
                // int.MaxValue is the undefined quantity of v1 and v2, which the record interface reads as undefined in v3 too
                record.WriteInt64(this.Quantity == int.MaxValue ? long.MaxValue : this.Quantity);
            }
            else if(this.Quantity == long.MaxValue)
            {
                record.WriteInt32(int.MaxValue);
            }
            else if(this.Quantity < int.MinValue || this.Quantity > int.MaxValue)
            {
                throw DbnFieldWriter.Invalid("quantity", this.Quantity, "DBN v1 and v2 statistics hold a 32-bit quantity");
            }
            else
            {
                record.WriteInt32((int)this.Quantity);
            }

            record.WriteUInt32(this.Sequence);
            record.WriteInt32(this.TsInDelta);
            record.WriteUInt16(this.StatType);
            record.WriteUInt16(this.ChannelId);
            record.WriteByte(this.UpdateAction);
            record.WriteByte((byte)this.StatFlags);
            record.WriteTo(writer);
        }

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.TsEventUtc);

        double? IStatisticsRecord.Price => UndefinedValues.Price(this.Price);

        long? IStatisticsRecord.Quantity => UndefinedValues.Quantity(this.Quantity);

        DateTime? IStatisticsRecord.TsReceivedUtc => UndefinedValues.Timestamp(this.TsReceivedUtc);
    }
}
