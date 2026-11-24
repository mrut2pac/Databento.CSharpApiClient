using System;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// A day file as a compaction run found it. The size and write time are the snapshot the file is deleted against, so a day downloaded
    /// again during the run is kept.
    /// </summary>
    internal sealed class FoundDayFile
    {
        public FoundDayFile(string path, string rawSymbol, DateTime date, long length, DateTime lastWriteUtc)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentException.ThrowIfNullOrEmpty(rawSymbol);

            this.Path = path;
            this.RawSymbol = rawSymbol;
            this.Date = date.Date;
            this.Length = length;
            this.LastWriteUtc = lastWriteUtc;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the Databento raw symbol the day is archived under.
        /// </summary>
        public string RawSymbol { get; }

        /// <summary>
        /// Gets the UTC day.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets the size in bytes when the run found it.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the last write time (UTC) when the run found it.
        /// </summary>
        public DateTime LastWriteUtc { get; }
    }
}
