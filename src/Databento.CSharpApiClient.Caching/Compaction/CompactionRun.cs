using System;
using System.Collections.Generic;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// What every group of one compaction run shares: its settings, the cache's day cutoff, its file operations and its report
    /// </summary>
    internal sealed class CompactionRun
    {
        public CompactionRun(ArchiveCompactorOptions options, string cutoff, ArchiveCompactionReport report, Action<string> temporaryFileWritten, Action<string> deleteFile)
        {
            this.Options = options;
            this.Cutoff = cutoff;
            this.Report = report;
            this.FileOperations = new ArchiveFileOperations(options, deleteFile);
            this.TemporaryFileWritten = temporaryFileWritten;
        }

        /// <summary>
        /// Gets the run's settings.
        /// </summary>
        public ArchiveCompactorOptions Options { get; }

        /// <summary>
        /// Gets the cache's day cutoff tag, which every file name carries.
        /// </summary>
        public string Cutoff { get; }

        /// <summary>
        /// Gets the run's report.
        /// </summary>
        public ArchiveCompactionReport Report { get; }

        /// <summary>
        /// Gets the run's file deletes.
        /// </summary>
        public ArchiveFileOperations FileOperations { get; }

        /// <summary>
        /// Gets what is called with each temporary file's path once it is written, before it is verified; <c>null</c> for nothing.
        /// </summary>
        public Action<string> TemporaryFileWritten { get; }

        /// <summary>
        /// Gets the day checks that judge a dataset's schema
        /// </summary>
        public IReadOnlyList<IArchiveDayCheck> GetChecks(string dataset, string schema)
        {
            List<IArchiveDayCheck> checks = new List<IArchiveDayCheck>();
            foreach(IArchiveDayCheck check in this.Options.DayChecks)
            {
                if(check.AppliesTo(dataset, schema))
                {
                    checks.Add(check);
                }
            }

            return checks;
        }
    }
}
