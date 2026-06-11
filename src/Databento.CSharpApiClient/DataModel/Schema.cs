namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// String constants for all Databento schema values accepted by the timeseries.get_range endpoint.
    /// </summary>
    public static class Schema
    {
        /// <summary>Market-by-order (Level 3 — full order book).</summary>
        public const string Mbo = "mbo";

        /// <summary>Market-by-price depth-1 (top-of-book events).</summary>
        public const string Mbp1 = "mbp-1";

        /// <summary>Market-by-price depth-10 (top 10 levels).</summary>
        public const string Mbp10 = "mbp-10";

        /// <summary>Top-of-book sampled in trade space (trade + BBO).</summary>
        public const string Tbbo = "tbbo";

        /// <summary>Individual trade ticks.</summary>
        public const string Trades = "trades";

        /// <summary>OHLCV bars aggregated over 1-second intervals.</summary>
        public const string Ohlcv1Sec = "ohlcv-1s";

        /// <summary>OHLCV bars aggregated over 1-minute intervals.</summary>
        public const string Ohlcv1Min = "ohlcv-1m";

        /// <summary>OHLCV bars aggregated over 1-hour intervals.</summary>
        public const string Ohlcv1Hour = "ohlcv-1h";

        /// <summary>OHLCV bars aggregated over calendar-day intervals.</summary>
        public const string Ohlcv1Day = "ohlcv-1d";

        /// <summary>End-of-day OHLCV bars (exchange settlement prices).</summary>
        public const string OhlcvEod = "ohlcv-eod";

        /// <summary>Instrument definitions (point-in-time metadata for each symbol).</summary>
        public const string Definition = "definition";

        /// <summary>Exchange status / trading-halt messages.</summary>
        public const string Status = "status";

        /// <summary>Auction imbalance messages.</summary>
        public const string Imbalance = "imbalance";

        /// <summary>Publisher statistics: volume, open interest, settlement prices.</summary>
        public const string Statistics = "statistics";

        /// <summary>Symbol mapping messages that associate instrument IDs to raw symbols.</summary>
        public const string SymbolMapping = "symbol_mapping";

        /// <summary>Consolidated/National BBO snapshots at 1-second intervals.</summary>
        public const string ConsolidatedBBO1Sec = "cbbo-1s";

        /// <summary>Consolidated/National BBO snapshots at 1-minute intervals.</summary>
        public const string ConsolidatedBBO1Min = "cbbo-1m";

        /// <summary>Trade tick combined with the previous CBBO (useful for inferring aggressor direction).</summary>
        public const string Tcbbo = "tcbbo";

        /// <summary>Consolidated MBP depth-1 sampled between trades.</summary>
        public const string Cmbp1 = "cmbp-1";

        /// <summary>Venue-local BBO snapshots at 1-second intervals.</summary>
        public const string Bbo1Sec = "bbo-1s";

        /// <summary>Venue-local BBO snapshots at 1-minute intervals.</summary>
        public const string Bbo1Min = "bbo-1m";
    }
}
