using System;
using System.IO;
using System.Text;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Builds a minimal valid DBN v2 binary stream for use in unit tests.
    /// The produced bytes can be wrapped in an HttpResponseMessage and fed to the clients
    /// without touching any network.
    /// </summary>
    internal static class DbnBinaryBuilder
    {
        // =====================================================================================
        // Record-level byte counts (header + body, no ts_out padding).
        // These match the exact field sequences in CbboRecordDbn / TradeRecordDbn / Mbp1RecordDbn.
        // Header is always 16 bytes (1+1+2+4+8).
        // =====================================================================================

        /// <summary>Total byte size of a CBBO record (header=16 + body=52).</summary>
        internal const int CbboRecordBytes = 68;

        /// <summary>Total byte size of a Trades record (header=16 + body=32).</summary>
        internal const int TradesRecordBytes = 48;

        /// <summary>Total byte size of an MBP-1 record (header=16 + body=64).</summary>
        internal const int Mbp1RecordBytes = 80;

        // =====================================================================================
        // Stream builders
        // =====================================================================================

        /// <summary>Returns a DBN v2 byte stream containing the supplied CBBO records.</summary>
        public static byte[] BuildCbboStream(params CbboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0xC0 /*Cbbo1S*/);
                foreach(CbboSeed rec in records)
                    WriteCbboRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied Trades records.</summary>
        public static byte[] BuildTradesStream(params TradesSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x00 /*Trades*/);
                foreach(TradesSeed rec in records)
                    WriteTradesRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied MBP-1 records.</summary>
        public static byte[] BuildMbp1Stream(params Mbp1Seed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x01 /*Mbp1*/);
                foreach(Mbp1Seed rec in records)
                    WriteMbp1Record(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        // =====================================================================================
        // Metadata block
        // =====================================================================================

        private static void WriteMetadata(BinaryWriter w, ushort schema)
        {
            // DBN v2 prelude: 'D','B','N', version=2
            w.Write((byte)'D');
            w.Write((byte)'B');
            w.Write((byte)'N');
            w.Write((byte)2);

            // metadata_length (u32 LE) — must match the body we write next (120 bytes)
            const uint metaLen = 120;
            w.Write(metaLen);

            // ---- metadata body (120 bytes) ----

            // dataset: char[16]
            WriteFixedString(w, "XNAS.ITCH", 16);

            // schema: u16
            w.Write(schema);

            // start: u64 ns (2022-05-16 13:30:00 UTC)
            w.Write(DateTimeOffsetToNs(new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero)));

            // end: u64 ns (same day + 1h); 0 means null but non-zero is fine
            w.Write(DateTimeOffsetToNs(new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero)));

            // limit: u64 (0 = none)
            w.Write(0UL);

            // stype_in: u8
            w.Write((byte)0x00);

            // stype_out: u8
            w.Write((byte)0x01);

            // ts_out: u8
            w.Write((byte)0x00);

            // symbol_cstr_len: u16 (v2 field)
            w.Write((ushort)22);

            // reserved: 53 bytes (v2)
            w.Write(new byte[53]);

            // schema_definition_length: u32 = 0
            w.Write(0u);

            // symbols_length: u32 = 0
            w.Write(0u);

            // partial_length: u32 = 0
            w.Write(0u);

            // not_found_length: u32 = 0
            w.Write(0u);

            // mappings_length: u32 = 0
            w.Write(0u);

            // Verify the metadata body is exactly 120 bytes.
            // 16+2+8+8+8+1+1+1+2+53+4+4+4+4+4 = 120 ✓
        }

        // =====================================================================================
        // Individual record writers
        // =====================================================================================

        private static void WriteCbboRecord(BinaryWriter w, CbboSeed rec)
        {
            // Header (16 bytes): length_byte, rtype, publisher_id, instrument_id, ts_event
            w.Write((byte)(CbboRecordBytes / 4)); // length_byte = 17
            w.Write((byte)0xC0);                  // rtype = Cbbo1S
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (52 bytes)
            w.Write(PriceToNano(rec.Price));       // 8
            w.Write(rec.Size);                      // 4
            w.Write((byte)0x00);                    // skip (1)
            w.Write((byte)'N');                     // side = None (1)
            w.Write((byte)0x00);                    // flags (1)
            w.Write((byte)0x00);                    // skip (1)
            w.Write(DateTimeOffsetToNs(rec.TsReceived)); // 8
            w.Write(PriceToNano(rec.BidPrice));     // 8
            w.Write(PriceToNano(rec.AskPrice));     // 8
            w.Write(rec.BidSize);                   // 4
            w.Write(rec.AskSize);                   // 4
            w.Write(rec.BidPublisherId);             // 2
            w.Write(rec.AskPublisherId);             // 2
        }

        private static void WriteTradesRecord(BinaryWriter w, TradesSeed rec)
        {
            // Header (16 bytes)
            w.Write((byte)(TradesRecordBytes / 4)); // length_byte = 12
            w.Write((byte)0x00);                    // rtype = Mbp0 (Trades)
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (32 bytes)
            w.Write(PriceToNano(rec.Price));        // 8
            w.Write(rec.Size);                       // 4
            w.Write((byte)0x54);                    // action = 'T' (Trade) (1)
            w.Write((byte)'B');                     // side = Buyer (1)
            w.Write((byte)0x00);                    // flags (1)
            w.Write((byte)0x00);                    // depth (1)
            w.Write(DateTimeOffsetToNs(rec.TsReceived)); // 8
            w.Write(rec.TsInDelta);                 // 4
            w.Write(rec.Sequence);                  // 4
        }

        private static void WriteMbp1Record(BinaryWriter w, Mbp1Seed rec)
        {
            // Header (16 bytes)
            w.Write((byte)(Mbp1RecordBytes / 4));   // length_byte = 20
            w.Write((byte)0x01);                    // rtype = Mbp1
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (64 bytes)
            w.Write(PriceToNano(rec.Price));        // 8
            w.Write(rec.Size);                       // 4
            w.Write((byte)0x54);                    // action (1)
            w.Write((byte)'N');                     // side = None (1)
            w.Write((byte)0x00);                    // flags (1)
            w.Write((byte)0x00);                    // depth (1)
            w.Write(DateTimeOffsetToNs(rec.TsReceived)); // 8
            w.Write(rec.TsInDelta);                 // 4
            w.Write(rec.Sequence);                  // 4
            w.Write(PriceToNano(rec.BidPrice));     // 8
            w.Write(PriceToNano(rec.AskPrice));     // 8
            w.Write(rec.BidSize);                   // 4
            w.Write(rec.AskSize);                   // 4
            w.Write(rec.BidCount);                  // 4
            w.Write(rec.AskCount);                  // 4
        }

        // =====================================================================================
        // Helpers
        // =====================================================================================

        private static long PriceToNano(double price) => (long)Math.Round(price * 1e9, MidpointRounding.AwayFromZero);

        private static ulong DateTimeOffsetToNs(DateTimeOffset dt)
        {
            if(dt == DateTimeOffset.MinValue)
                return 0UL;
            long ticks = (dt - DateTimeOffset.UnixEpoch).Ticks;
            return (ulong)(ticks * 100L); // 1 tick = 100 ns
        }

        private static void WriteFixedString(BinaryWriter w, string value, int width)
        {
            byte[] buf = new byte[width];
            if(!string.IsNullOrEmpty(value))
            {
                byte[] encoded = Encoding.ASCII.GetBytes(value);
                int copy = Math.Min(encoded.Length, width - 1); // leave room for NUL terminator
                Array.Copy(encoded, buf, copy);
            }

            w.Write(buf);
        }
    }

    // =====================================================================================
    // Seed data types — plain structs used as test inputs
    // =====================================================================================

    internal sealed class CbboSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public double Price { get; set; }
        public uint Size { get; set; }
        public double BidPrice { get; set; }
        public double AskPrice { get; set; }
        public uint BidSize { get; set; }
        public uint AskSize { get; set; }
        public ushort BidPublisherId { get; set; }
        public ushort AskPublisherId { get; set; }
    }

    internal sealed class TradesSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public double Price { get; set; }
        public uint Size { get; set; }
        public int TsInDelta { get; set; }
        public uint Sequence { get; set; }
    }

    internal sealed class Mbp1Seed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public double Price { get; set; }
        public uint Size { get; set; }
        public int TsInDelta { get; set; }
        public uint Sequence { get; set; }
        public double BidPrice { get; set; }
        public double AskPrice { get; set; }
        public uint BidSize { get; set; }
        public uint AskSize { get; set; }
        public uint BidCount { get; set; }
        public uint AskCount { get; set; }
    }
}
