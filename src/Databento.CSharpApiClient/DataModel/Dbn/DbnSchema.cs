namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Schema identifier stored in <see cref="DbnMetadata"/>.
    /// Mirrors the <c>schema</c> query parameter accepted by <c>timeseries.get_range</c>.
    /// </summary>
    public enum DbnSchema : ushort
    {
        /// <summary>Market-by-order full depth.</summary>
        Mbo = 0,

        /// <summary>Market-by-price top-of-book (depth 1).</summary>
        Mbp1 = 1,

        /// <summary>Market-by-price (depth 10).</summary>
        Mbp10 = 2,

        /// <summary>Top-of-book snapshot captured at each trade.</summary>
        Tbbo = 3,

        /// <summary>Individual trade ticks.</summary>
        Trades = 4,

        /// <summary>OHLCV 1-second bars.</summary>
        Ohlcv1S = 5,

        /// <summary>OHLCV 1-minute bars.</summary>
        Ohlcv1M = 6,

        /// <summary>OHLCV 1-hour bars.</summary>
        Ohlcv1H = 7,

        /// <summary>OHLCV daily (UTC midnight) bars.</summary>
        Ohlcv1D = 8,

        /// <summary>Instrument definitions (point-in-time contract metadata).</summary>
        Definition = 9,

        /// <summary>Publisher statistics (settlement, open interest, volume).</summary>
        Statistics = 10,

        /// <summary>Exchange trading-status messages.</summary>
        Status = 11,

        /// <summary>Auction imbalance messages.</summary>
        Imbalance = 12,

        /// <summary>OHLCV end-of-day bars (exchange session boundary).</summary>
        OhlcvEod = 13,

        /// <summary>Consolidated market-by-price depth-1.</summary>
        Cmbp1 = 14,

        /// <summary>Consolidated BBO subsampled every second.</summary>
        Cbbo1S = 15,

        /// <summary>Consolidated BBO subsampled every minute.</summary>
        Cbbo1M = 16,

        /// <summary>Trade tick plus the CBBO immediately preceding the trade.</summary>
        Tcbbo = 17,

        /// <summary>Venue-local BBO subsampled every second.</summary>
        Bbo1S = 18,

        /// <summary>Venue-local BBO subsampled every minute.</summary>
        Bbo1M = 19,

        /// <summary>NULL sentinel — no schema specified (0xFFFF).</summary>
        Null = 0xFFFF,
    }
}
