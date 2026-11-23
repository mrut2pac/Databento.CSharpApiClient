using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// String constants for Databento symbol type identifiers (stype_in / stype_out parameters). The Historical API takes
    /// <see cref="RawSymbol"/>, <see cref="Parent"/>, <see cref="InstrumentId"/> and <see cref="Continuous"/>; it refuses the
    /// obsolete ones with HTTP 400.
    /// </summary>
    public static class SymbolTypes
    {
        /// <summary>Venue-native ticker symbol as published (e.g. "SPY", "SPXW  250101C04000000").</summary>
        public const string RawSymbol = "raw_symbol";

        /// <summary>A parent symbol, standing for every instrument under it (e.g. "SPY.OPT" for every SPY option, "ES.FUT" for every ES future).</summary>
        public const string Parent = "parent";

        /// <summary>Databento numeric instrument identifier (uint32).</summary>
        public const string InstrumentId = "instrument_id";

        /// <summary>Continuous front-month contract (e.g. "ES.c.0").</summary>
        public const string Continuous = "continuous";

        /// <summary>Nasdaq-style symbol with expiry and strike encoding.</summary>
        [Obsolete("The Historical API refuses it: it takes only raw_symbol, parent, instrument_id and continuous.")]
        public const string Nasdaq = "nasdaq";

        /// <summary>OCC option symbol (e.g. "SPY   250101C00400000").</summary>
        [Obsolete("The Historical API refuses it: it takes only raw_symbol, parent, instrument_id and continuous. An OPRA raw symbol is the OCC symbol.")]
        public const string OccSymbol = "occ_symbol";

        /// <summary>CFI code (ISO 10962).</summary>
        [Obsolete("The Historical API refuses it: it takes only raw_symbol, parent, instrument_id and continuous.")]
        public const string Cfi = "cfi";

        /// <summary>ISIN (ISO 6166).</summary>
        [Obsolete("The Historical API refuses it: it takes only raw_symbol, parent, instrument_id and continuous.")]
        public const string Isin = "isin";
    }
}
