using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A Market-by-Order (Level 3) record deserialized from a DBN binary stream.
    /// Schema: <c>mbo</c> — rtype <c>Mbo</c> (0xA0).
    /// DBN v2 body is 48 bytes (total 64, length_byte = 16); DBN v1 body is 40 bytes (total 56, length_byte = 14).
    /// </summary>
    public sealed class MboRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Mbo"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this order event.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Unique order identifier assigned by the exchange.</summary>
        public ulong OrderId { get; set; }

        /// <summary>Order price (display-scaled).</summary>
        public double Price { get; set; }

        /// <summary>Order quantity in lots.</summary>
        public uint Size { get; set; }

        /// <summary>Message info flags.</summary>
        public MessageInfoBits Flags { get; set; }

        /// <summary>Publisher channel identifier.</summary>
        public ushort ChannelId { get; set; }

        /// <summary>Order-book action that triggered this update.</summary>
        public OrderBookAction Action { get; set; }

        /// <summary>Side of the order.</summary>
        public TradeAggressor Side { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        public int TsInDelta { get; set; }

        /// <summary>Venue sequence number for ordering within the same nanosecond.</summary>
        public uint Sequence { get; set; }

        /// <summary>
        /// Deserialises an MBO record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static MboRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                MboRecordDbn record = new MboRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    record.OrderId = body.ReadUInt64();
                    record.Price   = Utils.NanoToDouble(body.ReadInt64());
                    record.Size    = body.ReadUInt32();
                    record.Flags   = (MessageInfoBits)body.ReadByte();

                    if(bodyBytes.Length >= 48)
                    {
                        // DBN v2 layout (48-byte body):
                        // flags(1) + _pad(1) + channel_id(2) + action(1) + side(1) + _pad(6) + ts_recv(8) + ts_in_delta(4) + sequence(4)
                        body.ReadByte();  // _pad
                        record.ChannelId = body.ReadUInt16();
                        record.Action    = (OrderBookAction)body.ReadByte();
                        record.Side      = Utils.ReadSide(body.ReadByte());
                        body.ReadBytes(6); // _pad (align ts_recv to 8 bytes)
                    }
                    else
                    {
                        // DBN v1 layout (40-byte body): no channel_id, flags + action + side + _reserved(1)
                        // ts_recv falls naturally on an 8-byte boundary at body offset 24.
                        record.Action = (OrderBookAction)body.ReadByte();
                        record.Side   = Utils.ReadSide(body.ReadByte());
                        body.ReadByte(); // _reserved (alignment pad)
                    }

                    record.TsReceivedUtc = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.TsInDelta     = body.ReadInt32();
                    record.Sequence      = body.ReadUInt32();
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN MBO record.");
            }
        }

    }
}
