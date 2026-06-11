namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// Identifies which side initiated (aggressed) a trade.
    /// The byte value equals the ASCII character published by the venue.
    /// </summary>
    public enum TradeAggressor : byte
    {
        /// <summary>No aggressor identified, or not applicable ('N').</summary>
        None = (byte)'N',

        /// <summary>The buy side was the aggressor — a marketable buy order hit the ask ('B').</summary>
        Buyer = (byte)'B',

        /// <summary>The sell side was the aggressor — a marketable sell order hit the bid ('A').</summary>
        Seller = (byte)'A',
    }
}
