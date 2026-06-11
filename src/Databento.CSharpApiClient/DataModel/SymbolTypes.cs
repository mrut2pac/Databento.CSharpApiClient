namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// String constants for Databento symbol type identifiers (stype_in / stype_out parameters).
    /// </summary>
    public static class SymbolTypes
    {
        /// <summary>Venue-native ticker symbol as published (e.g. "SPY", "SPXW  250101C04000000").</summary>
        public const string RawSymbol = "raw_symbol";

        /// <summary>Underlying / parent contract root (e.g. "SPY" for all SPY options).</summary>
        public const string Parent = "parent";

        /// <summary>Databento numeric instrument identifier (uint32).</summary>
        public const string InstrumentId = "instrument_id";

        /// <summary>Continuous front-month contract (e.g. "ES.c.0").</summary>
        public const string Continuous = "continuous";

        /// <summary>Nasdaq-style symbol with expiry and strike encoding.</summary>
        public const string Nasdaq = "nasdaq";

        /// <summary>OCC option symbol (e.g. "SPY   250101C00400000").</summary>
        public const string OccSymbol = "occ_symbol";

        /// <summary>CFI code (ISO 10962).</summary>
        public const string Cfi = "cfi";

        /// <summary>ISIN (ISO 6166).</summary>
        public const string Isin = "isin";
    }
}
