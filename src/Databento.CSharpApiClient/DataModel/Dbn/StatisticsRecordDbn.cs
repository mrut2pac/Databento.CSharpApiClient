using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A publisher-statistics record deserialized from a DBN binary stream.
    /// Schema: <c>statistics</c> — rtype <c>Statistics</c> (0x18).
    /// Carries settlement price, open interest, and other per-instrument statistics.
    /// Record body is 44 bytes; total record = 60 bytes (length_byte = 15).
    /// </summary>
    public sealed class StatisticsRecordDbn
    {
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

        /// <summary>Statistic quantity value (e.g. open interest).</summary>
        public int Quantity { get; set; }

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
                    record.TsRefUtc = tsRefNs == 0 ? (DateTime?)null : Utils.FromUnixNs(tsRefNs).UtcDateTime;
                    record.Price       = Utils.NanoToDouble(body.ReadInt64());
                    record.Quantity    = body.ReadInt32();
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
    }
}
