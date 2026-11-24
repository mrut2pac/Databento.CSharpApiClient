namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Which file of a version an archive file is
    /// </summary>
    internal enum ArchiveFileKind
    {
        /// <summary>
        /// The full file, generation 0: every symbol-day the version held when it was built
        /// </summary>
        Full,

        /// <summary>
        /// The delta of the full file of the same build, generation 1 and up: the symbol-days folded in since, winning over the full file's
        /// </summary>
        Delta,
    }
}
