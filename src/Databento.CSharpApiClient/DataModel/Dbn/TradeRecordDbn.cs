using System;
using System.IO;

using Databento.CSharpApiClient.DataModel.Json;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single trade tick deserialized from a DBN binary stream.
    /// Schema: <c>trades</c> — rtype <c>Mbp0</c> (0x00).
    /// </summary>
    public sealed class TradeRecordDbn : ITradeRecord
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Mbp0"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this trade.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp (trade time) at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Trade price (converted from nano-integer to double).</summary>
        public double Price { get; set; }

        /// <summary>Trade size in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Order-book action; always <see cref="OrderBookAction.Trade"/> for this schema.</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side of the trade.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Price level at which the trade occurred (usually 0).</summary>
        public byte Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>
        /// Requested symbol this record belongs to, in the request's input symbology, from the symbol mappings in the stream's
        /// metadata; <see langword="null"/> when they do not map the record's instrument on its date.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Deserialises a trade record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static TradeRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                TradeRecordDbn record = new TradeRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.Price = Utils.NanoToDouble(body.ReadInt64());
                    record.Size = body.ReadUInt32();
                    record.Action = (OrderBookAction)body.ReadByte();
                    record.Side = Utils.ReadSide(body.ReadByte());
                    record.Flags = (MessageInfoBits)body.ReadByte();
                    record.Depth = body.ReadByte();
                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta = body.ReadInt32();
                    record.Sequence = body.ReadUInt32();
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN trade record.");
            }
        }

        /// <summary>
        /// Returns this record as a <see cref="TradeRecordJson"/>, with the values <see cref="DatabentoJsonClient"/> reads from the JSON
        /// encoding of the same record. An undefined event time becomes <see langword="null"/> and an undefined price stays
        /// <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        public TradeRecordJson ToJsonRecord()
        {
            return new TradeRecordJson
            {
                Header = DbnRecordHeader.ToJsonHeader(this.RecordType, this.PublisherId, this.InstrumentId, this.TsEventUtc),
                Price = this.Price,
                Size = this.Size,
                Action = this.Action,
                Side = this.Side,
                Flags = this.Flags,
                Depth = this.Depth,
                TsReceivedUtc = this.TsReceivedUtc,
                TsInDelta = this.TsInDelta,
                Sequence = this.Sequence,
                Symbol = this.Symbol,
            };
        }

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.TsEventUtc);

        double? ITradeRecord.Price => UndefinedValues.Price(this.Price);

        DateTime? ITradeRecord.TsReceivedUtc => UndefinedValues.Timestamp(this.TsReceivedUtc);
    }
}
