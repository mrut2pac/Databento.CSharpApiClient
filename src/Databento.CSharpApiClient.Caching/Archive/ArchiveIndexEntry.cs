namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// One symbol-day an archive file holds
    /// </summary>
    internal sealed record ArchiveIndexEntry
    {
        public ArchiveIndexEntry(string symbol, int day, int recordCount)
        {
            this.Symbol = symbol;
            this.Day = day;
            this.RecordCount = recordCount;
        }

        /// <summary>
        /// Gets the Databento raw symbol, in the dataset's own symbology, e.g. <c>SPXW  261016C06600000</c>.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the day of the month, 1 to 31.
        /// </summary>
        public int Day { get; }

        /// <summary>
        /// Gets the number of records of the day; 0 means the day is cached as empty and final.
        /// </summary>
        public int RecordCount { get; }
    }
}
