using System;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// A day file a compaction run archived although an <see cref="IArchiveDayCheck"/> found it suspect. Downloading the day again saves a
    /// day file that is read ahead of the archive and absorbed by the next run.
    /// </summary>
    public sealed class SuspectDayFile
    {
        internal SuspectDayFile(string relativePath, ArchiveDaySuspect suspect, DateTime lastWriteUtc)
        {
            this.RelativePath = relativePath;
            this.Day = suspect.Day;
            this.Reason = suspect.Reason;
            this.ExpectedRecords = suspect.ExpectedRecords;
            this.Detail = suspect.Detail;
            this.LastWriteUtc = lastWriteUtc;
        }

        /// <summary>
        /// Gets the day file's path under the cache root, e.g. <c>OPRA.PILLAR\cbbo-1m\2026\10\SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz</c>.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the day: its raw symbol, date and record count.
        /// </summary>
        public ArchiveDay Day { get; }

        /// <summary>
        /// Gets why it is suspect, the check's code.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the record count it was judged against; 0 when the check counts none.
        /// </summary>
        public double ExpectedRecords { get; }

        /// <summary>
        /// Gets what the check found; <c>null</c> when the reason says it all.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Gets when the day file was last written, UTC.
        /// </summary>
        public DateTime LastWriteUtc { get; }
    }
}
