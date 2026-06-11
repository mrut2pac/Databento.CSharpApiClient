namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// A single date interval within a <see cref="DbnSymbolMapping"/> during which
    /// the output symbol is valid.
    /// </summary>
    public sealed class DbnMappingInterval
    {
        /// <summary>Inclusive start date of this mapping interval, encoded as YYYYMMDD.</summary>
        public uint StartDate { get; set; }

        /// <summary>Inclusive end date of this mapping interval, encoded as YYYYMMDD.</summary>
        public uint EndDate { get; set; }

        /// <summary>The resolved output symbol (e.g. instrument_id string) valid within this interval.</summary>
        public string Symbol { get; set; }
    }
}
