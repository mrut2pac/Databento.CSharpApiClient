using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// An auction imbalance message deserialized from a DBN binary stream.
    /// Schema: <c>imbalance</c> — rtype <c>Imbalance</c> (0x14).
    /// Record body is 96 bytes; total record = 112 bytes (length_byte = 28).
    /// </summary>
    public sealed class ImbalanceRecordDbn
    {
        /// <summary>DBN record-type discriminator (<see cref="RType.Imbalance"/>).</summary>
        public RType RecordType { get; set; }

        /// <summary>Publisher that sourced this imbalance message.</summary>
        public ushort PublisherId { get; set; }

        /// <summary>Databento numeric instrument identifier.</summary>
        public uint InstrumentId { get; set; }

        /// <summary>Event timestamp at the venue, in UTC.</summary>
        public DateTime TsEventUtc { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Reference price used by the exchange (display-scaled).</summary>
        public double RefPrice { get; set; }

        /// <summary>Scheduled auction time, in UTC.</summary>
        public DateTime AuctionTimeUtc { get; set; }

        /// <summary>Continuous-book clearing price (display-scaled).</summary>
        public double ContBookClrPrice { get; set; }

        /// <summary>Auction interest clearing price (display-scaled).</summary>
        public double AuctInterestClrPrice { get; set; }

        /// <summary>SSR filling price (display-scaled).</summary>
        public double SsrFillingPrice { get; set; }

        /// <summary>Indicative match price (display-scaled).</summary>
        public double IndMatchPrice { get; set; }

        /// <summary>Upper price collar (display-scaled).</summary>
        public double UpperCollar { get; set; }

        /// <summary>Lower price collar (display-scaled).</summary>
        public double LowerCollar { get; set; }

        /// <summary>Quantity of paired shares at the indicative match price.</summary>
        public uint PairedQty { get; set; }

        /// <summary>Total imbalance quantity (all unpaired shares).</summary>
        public uint TotalImbalanceQty { get; set; }

        /// <summary>Market-order imbalance quantity.</summary>
        public uint MarketImbalanceQty { get; set; }

        /// <summary>Quantity of unpaired shares at the current reference price.</summary>
        public uint UnpairedQty { get; set; }

        /// <summary>Auction type code (exchange-defined character).</summary>
        public char AuctionType { get; set; }

        /// <summary>Side of the imbalance ('B' = buy-side, 'S' = sell-side, 'N' = no imbalance).</summary>
        public char Side { get; set; }

        /// <summary>Auction status code.</summary>
        public byte AuctionStatus { get; set; }

        /// <summary>Freeze status indicator.</summary>
        public byte FreezeStatus { get; set; }

        /// <summary>Number of auction extensions that have occurred.</summary>
        public byte NumExtensions { get; set; }

        /// <summary>Side of the unpaired quantity at the current reference price.</summary>
        public char UnpairedSide { get; set; }

        /// <summary>Significant-imbalance indicator (exchange-defined character).</summary>
        public char SignificantImbalance { get; set; }

        /// <summary>
        /// Deserialises an imbalance record body from <paramref name="reader"/> using the
        /// already-parsed <paramref name="header"/>.
        /// </summary>
        /// <param name="header">Pre-read 16-byte record header.</param>
        /// <param name="reader">Reader positioned immediately after the header bytes.</param>
        public static ImbalanceRecordDbn ReadFromBytes(DbnRecordHeader header, BinaryReader reader)
        {
            try
            {
                ImbalanceRecordDbn record = new ImbalanceRecordDbn();
                record.RecordType = header.RecordType;
                record.PublisherId = header.PublisherId;
                record.InstrumentId = header.InstrumentId;
                record.TsEventUtc = header.TsEventUtc;

                byte[] bodyBytes = reader.ReadBytes(header.RecordLength - DbnRecordHeader.SizeBytes);

                using(System.IO.MemoryStream ms = new System.IO.MemoryStream(bodyBytes, writable: false))
                using(BinaryReader body = new BinaryReader(ms))
                {
                    // i64/u64 block (9 × 8 = 72 bytes)
                    record.TsReceivedUtc         = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.RefPrice              = Utils.NanoToDouble(body.ReadInt64());
                    record.AuctionTimeUtc        = Utils.FromUnixNs(body.ReadUInt64()).UtcDateTime;
                    record.ContBookClrPrice      = Utils.NanoToDouble(body.ReadInt64());
                    record.AuctInterestClrPrice  = Utils.NanoToDouble(body.ReadInt64());
                    record.SsrFillingPrice       = Utils.NanoToDouble(body.ReadInt64());
                    record.IndMatchPrice         = Utils.NanoToDouble(body.ReadInt64());
                    record.UpperCollar           = Utils.NanoToDouble(body.ReadInt64());
                    record.LowerCollar           = Utils.NanoToDouble(body.ReadInt64());

                    // u32 block (4 × 4 = 16 bytes)
                    record.PairedQty            = body.ReadUInt32();
                    record.TotalImbalanceQty    = body.ReadUInt32();
                    record.MarketImbalanceQty   = body.ReadUInt32();
                    record.UnpairedQty          = body.ReadUInt32();

                    // char/u8 block (8 bytes)
                    record.AuctionType          = (char)body.ReadByte();
                    record.Side                 = (char)body.ReadByte();
                    record.AuctionStatus        = body.ReadByte();
                    record.FreezeStatus         = body.ReadByte();
                    record.NumExtensions        = body.ReadByte();
                    record.UnpairedSide         = (char)body.ReadByte();
                    record.SignificantImbalance  = (char)body.ReadByte();
                    body.ReadByte(); // _reserved
                }

                return record;
            }
            catch(EndOfStreamException)
            {
                throw new InvalidDataException("Truncated DBN Imbalance record.");
            }
        }
    }
}
