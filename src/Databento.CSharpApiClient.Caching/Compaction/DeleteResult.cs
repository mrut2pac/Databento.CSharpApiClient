namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// How a delete of a retired file ended
    /// </summary>
    internal enum DeleteResult
    {
        /// <summary>
        /// The file is gone.
        /// </summary>
        Deleted,

        /// <summary>
        /// Every attempt failed, so the file was left for the next run.
        /// </summary>
        Left,

        /// <summary>
        /// The file may no longer be deleted, such as a day file saved again, so it was kept.
        /// </summary>
        Changed,
    }
}
