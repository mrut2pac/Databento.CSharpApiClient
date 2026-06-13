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
        // Header is always 16 bytes (1+1+2+4+8).
        // =====================================================================================

        /// <summary>Total byte size of a CBBO record (header=16 + body=52).</summary>
        internal const int CbboRecordBytes = 68;

        /// <summary>Total byte size of a Trades record (header=16 + body=32).</summary>
        internal const int TradesRecordBytes = 48;

        /// <summary>Total byte size of an MBP-1 record (header=16 + body=64).</summary>
        internal const int Mbp1RecordBytes = 80;

        /// <summary>Total byte size of an MBO record (header=16 + body=48).</summary>
        internal const int MboRecordBytes = 64;

        /// <summary>Total byte size of an MBP-10 record (header=16 + body=352).</summary>
        internal const int Mbp10RecordBytes = 368;

        /// <summary>Total byte size of a BBO record (header=16 + body=64).</summary>
        internal const int BboRecordBytes = 80;

        /// <summary>Total byte size of a TBBO record (header=16 + body=64).</summary>
        internal const int TbboRecordBytes = 80;

        /// <summary>Total byte size of a TCBBO record (header=16 + body=60).</summary>
        internal const int TcbboRecordBytes = 76;

        /// <summary>Total byte size of a CMBP-1 record (header=16 + body=60).</summary>
        internal const int Cmbp1RecordBytes = 76;

        /// <summary>Total byte size of an OHLCV record (header=16 + body=40).</summary>
        internal const int OhlcvRecordBytes = 56;

        /// <summary>Total byte size of a Statistics record (header=16 + body=44).</summary>
        internal const int StatisticsRecordBytes = 60;

        /// <summary>Total byte size of a Status record (header=16 + body=24).</summary>
        internal const int StatusRecordBytes = 40;

        /// <summary>Total byte size of an Imbalance record (header=16 + body=96).</summary>
        internal const int ImbalanceRecordBytes = 112;

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

        /// <summary>Returns a DBN v2 byte stream containing the supplied MBO records.</summary>
        public static byte[] BuildMboStream(params MboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0xA0 /*Mbo*/);
                foreach(MboSeed rec in records)
                    WriteMboRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied MBP-10 records.</summary>
        public static byte[] BuildMbp10Stream(params Mbp10Seed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x0A /*Mbp10*/);
                foreach(Mbp10Seed rec in records)
                    WriteMbp10Record(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied BBO records (bbo-1s or bbo-1m).</summary>
        public static byte[] BuildBboStream(byte rtype, params BboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: rtype);
                foreach(BboSeed rec in records)
                    WriteBboRecord(w, rtype, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied TBBO records.</summary>
        public static byte[] BuildTbboStream(params TbboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x03 /*Tbbo*/);
                foreach(TbboSeed rec in records)
                    WriteTbboRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied TCBBO records.</summary>
        public static byte[] BuildTcbboStream(params TcbboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0xC2 /*Tcbbo*/);
                foreach(TcbboSeed rec in records)
                    WriteTcbboRecord(w, 0xC2 /*Tcbbo*/, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied CMBP-1 records.</summary>
        public static byte[] BuildCmbp1Stream(params TcbboSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0xB1 /*Cmbp1*/);
                foreach(TcbboSeed rec in records)
                    WriteTcbboRecord(w, 0xB1 /*Cmbp1*/, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied OHLCV records.</summary>
        public static byte[] BuildOhlcvStream(byte rtype, params OhlcvSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: rtype);
                foreach(OhlcvSeed rec in records)
                    WriteOhlcvRecord(w, rtype, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied Statistics records.</summary>
        public static byte[] BuildStatisticsStream(params StatisticsSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x18 /*Statistics*/);
                foreach(StatisticsSeed rec in records)
                    WriteStatisticsRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied Status records.</summary>
        public static byte[] BuildStatusStream(params StatusSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x12 /*Status*/);
                foreach(StatusSeed rec in records)
                    WriteStatusRecord(w, rec);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Returns a DBN v2 byte stream containing the supplied Imbalance records.</summary>
        public static byte[] BuildImbalanceStream(params ImbalanceSeed[] records)
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
            {
                WriteMetadata(w, schema: 0x14 /*Imbalance*/);
                foreach(ImbalanceSeed rec in records)
                    WriteImbalanceRecord(w, rec);
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
            w.Write((byte)(CbboRecordBytes / 4)); // length_byte = 17
            w.Write((byte)0xC0);                  // rtype = Cbbo1S
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (52 bytes)
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x00);                    // skip
            w.Write((byte)'N');                     // side = None
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // skip
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(PriceToNano(rec.BidPrice));
            w.Write(PriceToNano(rec.AskPrice));
            w.Write(rec.BidSize);
            w.Write(rec.AskSize);
            w.Write(rec.BidPublisherId);
            w.Write(rec.AskPublisherId);
        }

        private static void WriteTradesRecord(BinaryWriter w, TradesSeed rec)
        {
            w.Write((byte)(TradesRecordBytes / 4)); // length_byte = 12
            w.Write((byte)0x00);                    // rtype = Mbp0 (Trades)
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (32 bytes)
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x54);                    // action = 'T' (Trade)
            w.Write((byte)'B');                     // side = Buyer
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // depth
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);
        }

        private static void WriteMbp1Record(BinaryWriter w, Mbp1Seed rec)
        {
            w.Write((byte)(Mbp1RecordBytes / 4));   // length_byte = 20
            w.Write((byte)0x01);                    // rtype = Mbp1
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (64 bytes)
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x54);                    // action
            w.Write((byte)'N');                     // side = None
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // depth
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);
            w.Write(PriceToNano(rec.BidPrice));
            w.Write(PriceToNano(rec.AskPrice));
            w.Write(rec.BidSize);
            w.Write(rec.AskSize);
            w.Write(rec.BidCount);
            w.Write(rec.AskCount);
        }

        private static void WriteMboRecord(BinaryWriter w, MboSeed rec)
        {
            w.Write((byte)(MboRecordBytes / 4));    // length_byte = 16
            w.Write((byte)0xA0);                    // rtype = Mbo
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (48 bytes):
            // order_id(8)+price(8)+size(4)+flags(1)+_pad(1)+channel_id(2)+action(1)+side(1)+_pad(6)+ts_recv(8)+ts_in_delta(4)+sequence(4)
            w.Write(rec.OrderId);
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)rec.Flags);
            w.Write((byte)0x00);                    // _pad
            w.Write(rec.ChannelId);
            w.Write((byte)rec.Action);
            w.Write((byte)'B');                     // side = Buyer
            w.Write(new byte[6]);                   // _pad (align ts_recv to 8 bytes)
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);
        }

        private static void WriteMbp10Record(BinaryWriter w, Mbp10Seed rec)
        {
            w.Write((byte)(Mbp10RecordBytes / 4));  // length_byte = 92
            w.Write((byte)0x0A);                    // rtype = Mbp10
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Event section (32 bytes)
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x54);                    // action = Trade
            w.Write((byte)'B');                     // side = Buyer
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // depth
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);

            // 10 price levels (32 bytes each)
            for(int i = 0; i < 10; i++)
            {
                LevelSeed lev = (rec.Levels != null && i < rec.Levels.Length) ? rec.Levels[i] : new LevelSeed();
                w.Write(PriceToNano(lev.BidPrice));
                w.Write(PriceToNano(lev.AskPrice));
                w.Write(lev.BidSize);
                w.Write(lev.AskSize);
                w.Write(lev.BidCount);
                w.Write(lev.AskCount);
            }
        }

        private static void WriteBboRecord(BinaryWriter w, byte rtype, BboSeed rec)
        {
            w.Write((byte)(BboRecordBytes / 4));    // length_byte = 20
            w.Write(rtype);                         // 0xC3=Bbo1S or 0xC4=Bbo1M
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (64 bytes): same layout as MBP-1 but action/depth are reserved
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x00);                    // _reserved (action slot)
            w.Write((byte)'N');                     // side
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // _reserved (depth slot)
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);
            w.Write(PriceToNano(rec.BidPrice));
            w.Write(PriceToNano(rec.AskPrice));
            w.Write(rec.BidSize);
            w.Write(rec.AskSize);
            w.Write(rec.BidCount);
            w.Write(rec.AskCount);
        }

        private static void WriteTbboRecord(BinaryWriter w, TbboSeed rec)
        {
            w.Write((byte)(TbboRecordBytes / 4));   // length_byte = 20
            w.Write((byte)0x01);                    // rtype = Mbp1 (TBBO schema sends Mbp1 rtype)
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (64 bytes): identical layout to MBP-1
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x54);                    // action = Trade
            w.Write((byte)'B');                     // side = Buyer
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // depth
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);
            w.Write(PriceToNano(rec.BidPrice));
            w.Write(PriceToNano(rec.AskPrice));
            w.Write(rec.BidSize);
            w.Write(rec.AskSize);
            w.Write(rec.BidCount);
            w.Write(rec.AskCount);
        }

        private static void WriteTcbboRecord(BinaryWriter w, byte rtype, TcbboSeed rec)
        {
            w.Write((byte)(TcbboRecordBytes / 4)); // length_byte = 19
            w.Write(rtype);                         // 0xC2=Tcbbo or 0xB1=Cmbp1
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Event section (32 bytes)
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Size);
            w.Write((byte)0x54);                    // action = Trade
            w.Write((byte)'B');                     // side = Buyer
            w.Write((byte)0x00);                    // flags
            w.Write((byte)0x00);                    // depth
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.TsInDelta);
            w.Write(rec.Sequence);

            // CbboLevel (28 bytes)
            w.Write(PriceToNano(rec.BidPrice));
            w.Write(PriceToNano(rec.AskPrice));
            w.Write(rec.BidSize);
            w.Write(rec.AskSize);
            w.Write(rec.BidPublisherId);
            w.Write(rec.AskPublisherId);
        }

        private static void WriteOhlcvRecord(BinaryWriter w, byte rtype, OhlcvSeed rec)
        {
            w.Write((byte)(OhlcvRecordBytes / 4));  // length_byte = 14
            w.Write(rtype);                         // 0x20=Ohlcv1S, 0x21=1M, 0x22=1H, 0x23=1D, 0x24=Eod
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (40 bytes): open(8)+high(8)+low(8)+close(8)+volume(8)
            w.Write(PriceToNano(rec.Open));
            w.Write(PriceToNano(rec.High));
            w.Write(PriceToNano(rec.Low));
            w.Write(PriceToNano(rec.Close));
            w.Write(rec.Volume);
        }

        private static void WriteStatisticsRecord(BinaryWriter w, StatisticsSeed rec)
        {
            w.Write((byte)(StatisticsRecordBytes / 4)); // length_byte = 15
            w.Write((byte)0x18);                         // rtype = Statistics
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (44 bytes):
            // ts_recv(8)+ts_ref(8)+price(8)+quantity(4)+sequence(4)+ts_in_delta(4)+stat_type(2)+channel_id(2)+update_action(1)+stat_flags(1)+_reserved(2)
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(DateTimeOffsetToNs(rec.TsRef));
            w.Write(PriceToNano(rec.Price));
            w.Write(rec.Quantity);
            w.Write(rec.Sequence);
            w.Write(rec.TsInDelta);
            w.Write(rec.StatType);
            w.Write(rec.ChannelId);
            w.Write(rec.UpdateAction);
            w.Write((byte)0x00); // stat_flags
            w.Write((ushort)0);  // _reserved (2 bytes)
        }

        private static void WriteStatusRecord(BinaryWriter w, StatusSeed rec)
        {
            w.Write((byte)(StatusRecordBytes / 4)); // length_byte = 10
            w.Write((byte)0x12);                    // rtype = Status
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // Body (24 bytes):
            // ts_recv(8)+action(2)+reason(2)+trading_event(2)+is_trading(1)+is_quoting(1)+is_short_sell_restricted(1)+_reserved(7)
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(rec.Action);
            w.Write(rec.Reason);
            w.Write(rec.TradingEvent);
            w.Write((byte)(rec.IsTrading ? 'Y' : 'N'));
            w.Write((byte)(rec.IsQuoting ? 'Y' : 'N'));
            w.Write((byte)(rec.IsShortSellRestricted ? 'Y' : 'N'));
            w.Write(new byte[7]); // _reserved
        }

        private static void WriteImbalanceRecord(BinaryWriter w, ImbalanceSeed rec)
        {
            w.Write((byte)(ImbalanceRecordBytes / 4)); // length_byte = 28
            w.Write((byte)0x14);                        // rtype = Imbalance
            w.Write(rec.PublisherId);
            w.Write(rec.InstrumentId);
            w.Write(DateTimeOffsetToNs(rec.TsEvent));

            // i64/u64 block (9 × 8 = 72 bytes)
            w.Write(DateTimeOffsetToNs(rec.TsReceived));
            w.Write(PriceToNano(rec.RefPrice));
            w.Write(DateTimeOffsetToNs(rec.AuctionTime));
            w.Write(PriceToNano(rec.ContBookClrPrice));
            w.Write(PriceToNano(rec.AuctInterestClrPrice));
            w.Write(PriceToNano(rec.SsrFillingPrice));
            w.Write(PriceToNano(rec.IndMatchPrice));
            w.Write(PriceToNano(rec.UpperCollar));
            w.Write(PriceToNano(rec.LowerCollar));

            // u32 block (4 × 4 = 16 bytes)
            w.Write(rec.PairedQty);
            w.Write(rec.TotalImbalanceQty);
            w.Write(rec.MarketImbalanceQty);
            w.Write(rec.UnpairedQty);

            // char/u8 block (8 bytes)
            w.Write((byte)(rec.AuctionType == 0 ? 'O' : rec.AuctionType));
            w.Write((byte)(rec.Side == 0 ? 'N' : rec.Side));
            w.Write(rec.AuctionStatus);
            w.Write(rec.FreezeStatus);
            w.Write(rec.NumExtensions);
            w.Write((byte)'N'); // unpaired_side
            w.Write((byte)'N'); // significant_imbalance
            w.Write((byte)0x00); // _reserved
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
    // Seed data types — plain classes used as test inputs
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

    internal sealed class MboSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public ulong OrderId { get; set; }
        public double Price { get; set; }
        public uint Size { get; set; }
        public byte Flags { get; set; }
        public ushort ChannelId { get; set; }
        public byte Action { get; set; }
        public int TsInDelta { get; set; }
        public uint Sequence { get; set; }
    }

    /// <summary>A single bid/ask level for MBP-10 test seeds.</summary>
    internal sealed class LevelSeed
    {
        public double BidPrice { get; set; }
        public double AskPrice { get; set; }
        public uint BidSize { get; set; }
        public uint AskSize { get; set; }
        public uint BidCount { get; set; }
        public uint AskCount { get; set; }
    }

    internal sealed class Mbp10Seed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public double Price { get; set; }
        public uint Size { get; set; }
        public int TsInDelta { get; set; }
        public uint Sequence { get; set; }
        public LevelSeed[] Levels { get; set; }
    }

    internal sealed class BboSeed
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

    internal sealed class TbboSeed
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

    internal sealed class TcbboSeed
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
        public ushort BidPublisherId { get; set; }
        public ushort AskPublisherId { get; set; }
    }

    internal sealed class OhlcvSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public ulong Volume { get; set; }
    }

    internal sealed class StatisticsSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public DateTimeOffset TsRef { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public uint Sequence { get; set; }
        public int TsInDelta { get; set; }
        public ushort StatType { get; set; }
        public ushort ChannelId { get; set; }
        public byte UpdateAction { get; set; }
    }

    internal sealed class StatusSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public ushort Action { get; set; }
        public ushort Reason { get; set; }
        public ushort TradingEvent { get; set; }
        public bool IsTrading { get; set; }
        public bool IsQuoting { get; set; }
        public bool IsShortSellRestricted { get; set; }
    }

    internal sealed class ImbalanceSeed
    {
        public ushort PublisherId { get; set; }
        public uint InstrumentId { get; set; }
        public DateTimeOffset TsEvent { get; set; }
        public DateTimeOffset TsReceived { get; set; }
        public double RefPrice { get; set; }
        public DateTimeOffset AuctionTime { get; set; }
        public double ContBookClrPrice { get; set; }
        public double AuctInterestClrPrice { get; set; }
        public double SsrFillingPrice { get; set; }
        public double IndMatchPrice { get; set; }
        public double UpperCollar { get; set; }
        public double LowerCollar { get; set; }
        public uint PairedQty { get; set; }
        public uint TotalImbalanceQty { get; set; }
        public uint MarketImbalanceQty { get; set; }
        public uint UnpairedQty { get; set; }
        public char AuctionType { get; set; }
        public char Side { get; set; }
        public byte AuctionStatus { get; set; }
        public byte FreezeStatus { get; set; }
        public byte NumExtensions { get; set; }
    }
}
