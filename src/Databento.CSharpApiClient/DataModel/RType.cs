namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// DBN record types enumeration
    /// </summary>
    public enum RType : byte
    {
        Mbp0 = 0x00,  // Market-by-price depth 0 (for Trades)
        Mbp1 = 0x01,  // Market-by-price depth 1 (also used for TBBO)
        Mbp10 = 0x0A,  // Market-by-price depth 10

        OhlcvDeprecated = 0x11, // Deprecated Ohlcv

        Ohlcv1S = 0x20,  // OHLCV 1-second bars
        Ohlcv1M = 0x21,  // OHLCV 1-minute bars
        Ohlcv1H = 0x22,  // OHLCV 1-hour bars
        Ohlcv1D = 0x23,  // OHLCV daily (UTC)
        OhlcvEod = 0x24,  // OHLCV end-of-day bar

        Status = 0x12,  // Exchange status
        InstrumentDef = 0x13,  // Instrument definition
        Imbalance = 0x14,  // Order imbalance
        Error = 0x15,  // Gateway error
        SymbolMapping = 0x16,  // Symbol mapping
        System = 0x17,  // Gateway/system messages (heartbeats, etc.)
        Statistics = 0x18,  // Publisher statistics

        Mbo = 0xA0,  // Market-by-order full book
        Cmbp1 = 0xB1,  // Consolidated best-price book (depth=1)

        Cbbo1S = 0xC0,  // Consolidated BBO every second
        Cbbo1M = 0xC1,  // Consolidated BBO every minute
        Tcbbo = 0xC2,  // Trade + previous CBBO

        Bbo1S = 0xC3,  // Venue-local BBO every second
        Bbo1M = 0xC4,  // Venue-local BBO every minute
    }
}
