namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// DBN symbol-type discriminator used in <see cref="DbnMetadata"/> (<c>stype_in</c> / <c>stype_out</c>).
    /// Identifies the encoding convention used for symbol strings in the stream.
    /// </summary>
    public enum DbnSType : byte
    {
        /// <summary>NULL sentinel — no symbol type specified (0xFF).</summary>
        Null = 0xFF,

        /// <summary>Databento numeric instrument identifier (uint32 as string).</summary>
        InstrumentId = 0,

        /// <summary>Exchange-native (raw) ticker symbol.</summary>
        RawSymbol = 1,

        /// <summary>Continuous front-month contract symbol (e.g. "ES.c.0").</summary>
        Continuous = 2,

        /// <summary>Underlying / parent symbol (all strikes and expiries roll up to this).</summary>
        Parent = 3,

        /// <summary>ISO 10962 CFI code.</summary>
        Cfi = 4,

        /// <summary>ISO 6166 International Securities Identification Number.</summary>
        ISIN = 5,

        /// <summary>London Stock Exchange SEDOL code.</summary>
        SEDOL = 6,

        /// <summary>Refinitiv Instrument Code.</summary>
        RIC = 7,

        /// <summary>Bloomberg ticker.</summary>
        Bloomberg = 8,

        /// <summary>CUSIP identifier.</summary>
        Cusip = 9,

        /// <summary>OCC option clearing ID.</summary>
        Ocid = 10,

        /// <summary>Composite symbol spanning multiple venues.</summary>
        Composite = 11,

        /// <summary>Root symbol (e.g. futures root such as "ES").</summary>
        Root = 12,

        /// <summary>OCC standardised option symbol.</summary>
        OccSymbol = 13,

        /// <summary>Exchange-assigned security identifier.</summary>
        SecurityId = 14,

        /// <summary>Human-friendly ticker string.</summary>
        Ticker = 15,
    }
}
