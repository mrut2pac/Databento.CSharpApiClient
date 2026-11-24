using System;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// Where a compaction run is as it starts a group, told through <see cref="ArchiveCompactorOptions.Progress"/>. Groups differ in size by
    /// orders of magnitude, so the estimate goes by day files, not groups.
    /// </summary>
    public sealed class ArchiveCompactionProgress
    {
        internal ArchiveCompactionProgress(string relativeDirectory, string group, int dayFiles, int groupNumber, int plannedGroups, long dayFilesDone, long plannedDayFiles, TimeSpan elapsed, TimeSpan? estimatedRemaining)
        {
            this.RelativeDirectory = relativeDirectory;
            this.Group = group;
            this.DayFiles = dayFiles;
            this.GroupNumber = groupNumber;
            this.PlannedGroups = plannedGroups;
            this.DayFilesDone = dayFilesDone;
            this.PlannedDayFiles = plannedDayFiles;
            this.Elapsed = elapsed;
            this.EstimatedRemaining = estimatedRemaining;
        }

        /// <summary>
        /// Gets the month folder of the group starting, under the cache root, e.g. <c>OPRA.PILLAR\cbbo-1m\2026\10</c>.
        /// </summary>
        public string RelativeDirectory { get; }

        /// <summary>
        /// Gets the group starting, e.g. <c>SPXW.OPT</c>.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the number of day files the group starting folds in.
        /// </summary>
        public int DayFiles { get; }

        /// <summary>
        /// Gets the group's number in the run, from 1.
        /// </summary>
        public int GroupNumber { get; }

        /// <summary>
        /// Gets the groups the run planned, which day files saved during the run can outgrow.
        /// </summary>
        public int PlannedGroups { get; }

        /// <summary>
        /// Gets the day files of the groups finished so far, compacted or failed alike.
        /// </summary>
        public long DayFilesDone { get; }

        /// <summary>
        /// Gets the day files the run planned to compact.
        /// </summary>
        public long PlannedDayFiles { get; }

        /// <summary>
        /// Gets the time the run has spent compacting so far.
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Gets the time the run should still take, by its pace so far; <c>null</c> until a group with day files has finished.
        /// </summary>
        public TimeSpan? EstimatedRemaining { get; }
    }
}
