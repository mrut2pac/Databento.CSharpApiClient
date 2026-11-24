namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// How a compaction checks a file it wrote before publishing it; a file that fails is deleted and its group left as it was
    /// </summary>
    public enum ArchiveVerification
    {
        /// <summary>
        /// The file is read back and every record compared, column by column, with its source read again
        /// </summary>
        EveryRecord,

        /// <summary>
        /// The file's index is read back and compared with the index written: the same days, each with the same record count
        /// </summary>
        CountsOnly,

        /// <summary>
        /// The file is published as written
        /// </summary>
        None,
    }
}
