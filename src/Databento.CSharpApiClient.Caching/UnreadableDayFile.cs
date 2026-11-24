namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// A day file a compaction run could not read: left on disk and out of the archive
    /// </summary>
    public sealed class UnreadableDayFile
    {
        internal UnreadableDayFile(string relativePath, string error)
        {
            this.RelativePath = relativePath;
            this.Error = error;
        }

        /// <summary>
        /// Gets the day file's path under the cache root.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets why it could not be read.
        /// </summary>
        public string Error { get; }
    }
}
