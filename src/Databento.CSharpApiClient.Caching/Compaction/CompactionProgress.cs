using System;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// Tells how far a compaction run is and how long it should still take, by the day files compacted so far against those the planning
    /// pass found. Safe for groups compacted at once.
    /// </summary>
    internal sealed class CompactionProgress
    {
        private readonly object updateLock = new object();

        private readonly int plannedGroups;

        private readonly long plannedDayFiles;

        private int groupsStarted;

        private long dayFilesDone;

        public CompactionProgress(int plannedGroups, long plannedDayFiles)
        {
            this.plannedGroups = plannedGroups;
            this.plannedDayFiles = plannedDayFiles;
        }

        /// <summary>
        /// Starts the next group and tells where the run is
        /// </summary>
        /// <param name="relativeDirectory">The group's month folder under the cache root</param>
        /// <param name="group">The group</param>
        /// <param name="dayFiles">The number of day files the group folds in</param>
        /// <param name="elapsed">Time the run has spent compacting so far</param>
        /// <returns>The progress</returns>
        public ArchiveCompactionProgress StartGroup(string relativeDirectory, string group, int dayFiles, TimeSpan elapsed)
        {
            lock(this.updateLock)
            {
                ++this.groupsStarted;
                TimeSpan? remaining = null;
                if(this.dayFilesDone > 0)
                {
                    // day files saved during the run can outgrow the plan, which must not read as a negative wait
                    long dayFilesLeft = Math.Max(0, this.plannedDayFiles - this.dayFilesDone);
                    remaining = TimeSpan.FromTicks((long)((double)elapsed.Ticks * dayFilesLeft / this.dayFilesDone));
                }

                return new ArchiveCompactionProgress(relativeDirectory, group, dayFiles, this.groupsStarted, Math.Max(this.groupsStarted, this.plannedGroups), this.dayFilesDone, this.plannedDayFiles, elapsed, remaining);
            }
        }

        /// <summary>
        /// Counts a finished group's day files, compacted or failed alike
        /// </summary>
        public void EndGroup(int dayFiles)
        {
            lock(this.updateLock)
            {
                this.dayFilesDone += dayFiles;
            }
        }
    }
}
