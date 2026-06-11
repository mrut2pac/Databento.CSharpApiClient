namespace Databento.CSharpApiClient.DataModel.Dbn
{
    public enum DbnSType : byte
    {
        Null = 0xFF,        // NULL_STYPE sentinel (no type)

        InstrumentId = 0,   // Internal numeric instrument ID
        RawSymbol = 1,      // Exchange-native symbol (string)
        Continuous = 2,     // Continuous symbol
        Parent = 3,         // Parent symbol
        Cfi = 4,            // ISO 10962 Classification of Financial Instruments
        ISIN = 5,           // ISO 6166 International Securities Identification Number
        SEDOL = 6,          // Stock Exchange Daily Official List
        RIC = 7,            // Refinitiv Instrument Code
        Bloomberg = 8,      // Bloomberg symbol
        Cusip = 9,          // Committee on Uniform Securities Identification Procedures
        Ocid = 10,          // Option Clearing Corporation ID
        Composite = 11,     // Composite symbol
        Root = 12,          // Root symbol (e.g. futures root)
        OccSymbol = 13,     // OCC option symbol
        SecurityId = 14,    // Exchange security ID
        Ticker = 15         // Human-friendly ticker
    }
}
