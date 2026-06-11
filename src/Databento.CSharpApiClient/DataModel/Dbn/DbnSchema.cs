namespace Databento.CSharpApiClient.DataModel.Dbn
{
    public enum DbnSchema : ushort
    {
        Mbo = 0,   // Market by Order
        Mbp1 = 1,   // Market by Price, depth 1
        Mbp10 = 2,   // Market by Price, depth 10
        Tbbo = 3,   // Trade with Best Bid and Offer before execution
        Trades = 4,   // All trade events
        Ohlcv1S = 5,   // OHLC + Volume, 1-second aggregates
        Ohlcv1M = 6,   // OHLC + Volume, 1-minute aggregates
        Ohlcv1H = 7,   // OHLC + Volume, 1-hour aggregates
        Ohlcv1D = 8,   // OHLC + Volume, daily (UTC) aggregates
        Definition = 9,   // Instrument definitions
        Statistics = 10,  // Stats messages
        Status = 11,  // Exchange status events
        Imbalance = 12,  // Auction imbalance events
        OhlcvEod = 13,  // End-of-day OHLCV
        Cmbp1 = 14,  // Consolidated market-by-price, depth 1
        Cbbo1S = 15,  // Consolidated BBO, subsampled 1-second
        Cbbo1M = 16,  // Consolidated BBO, subsampled 1-minute
        Tcbbo = 17,  // Trade + consolidated BBO before trade
        Bbo1S = 18,  // BBO + trades, 1-second cadence
        Bbo1M = 19,  // BBO + trades, 1-minute cadence
        Null = 0xFFFF  // Sentinel for no schema
    }
}
