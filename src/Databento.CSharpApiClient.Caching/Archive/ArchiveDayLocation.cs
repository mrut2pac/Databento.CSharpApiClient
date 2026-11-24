namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Where a symbol-day's records sit in an archive file
    /// </summary>
    internal readonly struct ArchiveDayLocation
    {
        public ArchiveDayLocation(long firstRow, int recordCount)
        {
            this.FirstRow = firstRow;
            this.RecordCount = recordCount;
        }

        /// <summary>
        /// Gets the file row of the day's first record, counted from 0 across all row groups.
        /// </summary>
        public long FirstRow { get; }

        /// <summary>
        /// Gets the number of records of the day; 0 means the day is cached as empty and final.
        /// </summary>
        public int RecordCount { get; }
    }
}
