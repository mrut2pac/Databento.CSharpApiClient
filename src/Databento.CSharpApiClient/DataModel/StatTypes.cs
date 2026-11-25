namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// The <c>stat_type</c> values of the <c>statistics</c> schema, which say what a record's price or quantity is. Compare
    /// <see cref="IStatisticsRecord.StatType"/> with them; a venue publishes only some of them, and Databento may add more.
    /// </summary>
    public static class StatTypes
    {
        /// <summary>The price of the first trade of the instrument.</summary>
        public const ushort OpeningPrice = 1;

        /// <summary>The probable opening price, published during the pre-open.</summary>
        public const ushort IndicativeOpeningPrice = 2;

        /// <summary>The official settlement price; the record's flags say whether it is final or preliminary, actual or theoretical.</summary>
        public const ushort SettlementPrice = 3;

        /// <summary>The lowest trade price of the trading session.</summary>
        public const ushort TradingSessionLowPrice = 4;

        /// <summary>The highest trade price of the trading session.</summary>
        public const ushort TradingSessionHighPrice = 5;

        /// <summary>The number of contracts cleared on the previous trading date, in the quantity.</summary>
        public const ushort ClearedVolume = 6;

        /// <summary>The lowest offer price of the trading session.</summary>
        public const ushort LowestOffer = 7;

        /// <summary>The highest bid price of the trading session.</summary>
        public const ushort HighestBid = 8;

        /// <summary>The number of outstanding contracts, in the quantity.</summary>
        public const ushort OpenInterest = 9;

        /// <summary>The volume-weighted average price over a fixing period.</summary>
        public const ushort FixingPrice = 10;

        /// <summary>The price of the last trade of the trading session.</summary>
        public const ushort ClosePrice = 11;

        /// <summary>The change from the previous session's close to the latest close.</summary>
        public const ushort NetChange = 12;

        /// <summary>The volume-weighted average price of the trading session.</summary>
        public const ushort Vwap = 13;

        /// <summary>The implied volatility tied to the settlement price.</summary>
        public const ushort Volatility = 14;

        /// <summary>The option delta tied to the settlement price.</summary>
        public const ushort Delta = 15;

        /// <summary>The price of an auction that is neither the official open nor the close.</summary>
        public const ushort UncrossingPrice = 16;

        /// <summary>The upper price limit of the trading session.</summary>
        public const ushort UpperPriceLimit = 17;

        /// <summary>The lower price limit of the trading session.</summary>
        public const ushort LowerPriceLimit = 18;

        /// <summary>The number of contracts traded in blocks, in the quantity.</summary>
        public const ushort BlockVolume = 19;

        /// <summary>The probable price of the session's last trade, published during the session.</summary>
        public const ushort IndicativeClosePrice = 20;

        /// <summary>The market-wide circuit breaker Level 1 threshold (7%), in S&amp;P 500 points.</summary>
        public const ushort MwcbLevel1 = 21;

        /// <summary>The market-wide circuit breaker Level 2 threshold (13%), in S&amp;P 500 points.</summary>
        public const ushort MwcbLevel2 = 22;

        /// <summary>The market-wide circuit breaker Level 3 threshold (20%), in S&amp;P 500 points.</summary>
        public const ushort MwcbLevel3 = 23;

        /// <summary>The reference price an auction collar's bounds are derived from.</summary>
        public const ushort AuctionCollarReferencePrice = 24;

        /// <summary>The upper bound of an auction collar.</summary>
        public const ushort AuctionCollarUpperPrice = 25;

        /// <summary>The lower bound of an auction collar.</summary>
        public const ushort AuctionCollarLowerPrice = 26;

        /// <summary>A volume the venue defines; see the venue's documentation.</summary>
        public const ushort VenueSpecificVolume1 = 10001;

        /// <summary>A price the venue defines; see the venue's documentation.</summary>
        public const ushort VenueSpecificPrice1 = 10002;
    }
}
