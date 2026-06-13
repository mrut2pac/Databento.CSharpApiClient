namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// DBN record-type discriminator stored in each record header.
    /// The <c>rtype</c> byte determines the concrete record layout that follows the 16-byte header.
    /// </summary>
    public enum RType : byte
    {
        /// <summary>Market-by-price depth-0 record — used for trade ticks (<c>trades</c> schema).</summary>
        Mbp0 = 0x00,

        /// <summary>Market-by-price depth-1 record — top-of-book bid/ask after each event (<c>mbp-1</c>).</summary>
        Mbp1 = 0x01,

        /// <summary>Market-by-price depth-10 record (<c>mbp-10</c>).</summary>
        Mbp10 = 0x0A,

        /// <summary>
        /// Trade plus venue BBO snapshot (<c>tbbo</c> schema).
        /// Note: the live Databento API sends TBBO records with rtype <see cref="Mbp1"/> (0x01),
        /// sharing the MBP-1 binary layout.
        /// </summary>
        Tbbo = 0x03,

        /// <summary>Deprecated OHLCV record type. Use <see cref="Ohlcv1S"/>–<see cref="OhlcvEod"/> instead.</summary>
        OhlcvDeprecated = 0x11,

        /// <summary>OHLCV 1-second bar (<c>ohlcv-1s</c>).</summary>
        Ohlcv1S = 0x20,

        /// <summary>OHLCV 1-minute bar (<c>ohlcv-1m</c>).</summary>
        Ohlcv1M = 0x21,

        /// <summary>OHLCV 1-hour bar (<c>ohlcv-1h</c>).</summary>
        Ohlcv1H = 0x22,

        /// <summary>OHLCV daily bar, UTC midnight boundary (<c>ohlcv-1d</c>).</summary>
        Ohlcv1D = 0x23,

        /// <summary>OHLCV end-of-day bar using exchange session close (<c>ohlcv-eod</c>).</summary>
        OhlcvEod = 0x24,

        /// <summary>Exchange trading-status message (<c>status</c> schema).</summary>
        Status = 0x12,

        /// <summary>Instrument-definition record (<c>definition</c> schema).</summary>
        InstrumentDef = 0x13,

        /// <summary>Order-imbalance message (<c>imbalance</c> schema).</summary>
        Imbalance = 0x14,

        /// <summary>Gateway error message.</summary>
        Error = 0x15,

        /// <summary>Symbol-mapping message.</summary>
        SymbolMapping = 0x16,

        /// <summary>Gateway or system message (heartbeat, etc.).</summary>
        System = 0x17,

        /// <summary>Publisher-statistics message (settlement, open interest, etc.) (<c>statistics</c> schema).</summary>
        Statistics = 0x18,

        /// <summary>Market-by-order full depth (<c>mbo</c> schema).</summary>
        Mbo = 0xA0,

        /// <summary>Consolidated market-by-price depth-1 (<c>cmbp-1</c> schema).</summary>
        Cmbp1 = 0xB1,

        /// <summary>Consolidated BBO subsampled every second (<c>cbbo-1s</c> schema).</summary>
        Cbbo1S = 0xC0,

        /// <summary>Consolidated BBO subsampled every minute (<c>cbbo-1m</c> schema).</summary>
        Cbbo1M = 0xC1,

        /// <summary>Trade tick plus the CBBO immediately before the trade (<c>tcbbo</c> schema).</summary>
        Tcbbo = 0xC2,

        /// <summary>Venue-local BBO subsampled every second (<c>bbo-1s</c> schema).</summary>
        Bbo1S = 0xC3,

        /// <summary>Venue-local BBO subsampled every minute (<c>bbo-1m</c> schema).</summary>
        Bbo1M = 0xC4,
    }
}
