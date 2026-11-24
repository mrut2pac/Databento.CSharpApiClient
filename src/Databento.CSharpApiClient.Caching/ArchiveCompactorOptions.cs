using System;
using System.Collections.Generic;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// Settings of a <see cref="DatabentoArchiveCompactor"/> run
    /// </summary>
    public sealed class ArchiveCompactorOptions
    {
        /// <summary>
        /// Gets or sets the cache root, which holds <c>metadata.json</c> and one folder per Databento dataset.
        /// </summary>
        public string CacheRoot { get; set; }

        /// <summary>
        /// Gets or sets what a run writes for a group-month that has a full file: a rebuild, the next delta generation, or either by the
        /// two thresholds below.
        /// </summary>
        public CompactionMode CompactionMode { get; set; } = CompactionMode.Auto;

        /// <summary>
        /// Gets or sets the delta generation <see cref="CompactionMode.Auto"/> never writes, rebuilding the full file instead; 1 or more.
        /// </summary>
        public int MaxDeltaGeneration { get; set; } = 9;

        /// <summary>
        /// Gets or sets the size, in percent of its full file's, past which <see cref="CompactionMode.Auto"/> folds a delta into a rebuild;
        /// 0 or more.
        /// </summary>
        public int MaxDeltaSizePercent { get; set; } = 25;

        /// <summary>
        /// Gets or sets how many days old a day must be before its day file is archived, by the UTC date; 0, the default, archives every
        /// day file, which the cache saves only for final days.
        /// </summary>
        public int MinDayAgeDays { get; set; }

        /// <summary>
        /// Gets or sets what happens to empty days: archived as empty and final, or dropped so they are asked for again.
        /// </summary>
        public EmptyDayArchiving EmptyDays { get; set; } = EmptyDayArchiving.Archive;

        /// <summary>
        /// Gets or sets how a written file is checked against its sources before it is published.
        /// </summary>
        public ArchiveVerification Verification { get; set; } = ArchiveVerification.EveryRecord;

        /// <summary>
        /// Gets or sets the checks run on the day-file days of each group-month before its file is published; empty by default. A day a
        /// check finds suspect is reported and archived all the same.
        /// </summary>
        public IReadOnlyList<IArchiveDayCheck> DayChecks { get; set; } = Array.Empty<IArchiveDayCheck>();

        /// <summary>
        /// Gets or sets whether the run only writes and verifies its new files into <see cref="DryRunFolder"/> and changes nothing in the cache.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets the folder a dry run writes into, in the cache's layout; it neither holds nor is inside the cache root.
        /// </summary>
        public string DryRunFolder { get; set; }

        /// <summary>
        /// Gets or sets how many times a file is tried to be deleted before it is left for the next run; 1 or more.
        /// </summary>
        public int RetireAttempts { get; set; } = 5;

        /// <summary>
        /// Gets or sets the wait after the first failed delete; each further failure doubles it, up to a minute.
        /// </summary>
        public TimeSpan RetireRetryDelay { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets or sets how many groups are compacted at once; 1, the default, compacts them one after another. Groups write separate
        /// files, so a run gets faster with more until the disks are the limit.
        /// </summary>
        public int MaxParallelGroups { get; set; } = 1;

        /// <summary>
        /// Gets or sets the clock of build times, day ages and retry waits.
        /// </summary>
        public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        /// <summary>
        /// Gets or sets what is told the run's progress as each group starts; <c>null</c> for nothing. With <see cref="MaxParallelGroups"/>
        /// above 1 it may be called from several threads at once, and out of group order. An exception it throws is ignored, since progress
        /// is only diagnostic.
        /// </summary>
        public IProgress<ArchiveCompactionProgress> Progress { get; set; }
    }
}
