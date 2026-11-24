namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// What a compaction does with a day that holds no records
    /// </summary>
    public enum EmptyDayArchiving
    {
        /// <summary>
        /// Archived as empty and final, so it is never asked for again
        /// </summary>
        Archive,

        /// <summary>
        /// Left out of every file the run writes, and its day file deleted, so the day is asked for again; a day the archive already holds
        /// with records is then served from that copy
        /// </summary>
        Drop,
    }
}
