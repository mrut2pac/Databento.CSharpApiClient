namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// String constants for all Databento schema values accepted by the timeseries.get_range endpoint.
    /// </summary>
    public static class Schema
    {
        // Market-by-order (Level 3 — full order book)
        public const string Mbo = "mbo";

        // Market-by-price (Level 2)
        public const string Mbp1 = "mbp-1";
        public const string Mbp10 = "mbp-10";

        // Top-of-book sampled in trade space
        public const string Tbbo = "tbbo";

        // Individual trade ticks
        public const string Trades = "trades";

        // OHLCV bar schemas
        public const string Ohlcv1Sec = "ohlcv-1s";
        public const string Ohlcv1Min = "ohlcv-1m";
        public const string Ohlcv1Hour = "ohlcv-1h";
        public const string Ohlcv1Day = "ohlcv-1d";
        public const string OhlcvEod = "ohlcv-eod";

        // Instrument definitions (point-in-time metadata)
        public const string Definition = "definition";

        // Exchange status messages
        public const string Status = "status";

        // Auction imbalance messages
        public const string Imbalance = "imbalance";

        // Publisher statistics (volume, open interest, settlement)
        public const string Statistics = "statistics";

        // Symbol mapping messages
        public const string SymbolMapping = "symbol_mapping";

        // Consolidated BBO (NBBO snapshots)
        public const string ConsolidatedBBO1Sec = "cbbo-1s";
        public const string ConsolidatedBBO1Min = "cbbo-1m";

        // Trade + previous CBBO (useful for inferring direction)
        public const string Tcbbo = "tcbbo";

        // Consolidated MBP depth-1 (sampled between trades)
        public const string Cmbp1 = "cmbp-1";

        // Venue-local BBO snapshots
        public const string Bbo1Sec = "bbo-1s";
        public const string Bbo1Min = "bbo-1m";
    }
}
