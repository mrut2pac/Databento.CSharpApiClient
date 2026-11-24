using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// What a compaction run did. Filled by the groups of one run, which may be compacted at once; read once the run is done.
    /// </summary>
    public sealed class ArchiveCompactionReport
    {
        private readonly object updateLock = new object();

        private readonly List<ArchiveGroupFailure> failedGroups = new List<ArchiveGroupFailure>();

        private readonly List<UnreadableDayFile> unreadableDayFiles = new List<UnreadableDayFile>();

        private readonly List<string> filesLeft = new List<string>();

        private readonly List<string> orphanFiles = new List<string>();

        private readonly List<string> foldersSkipped = new List<string>();

        private readonly List<SuspectDayFile> suspects = new List<SuspectDayFile>();

        internal ArchiveCompactionReport(bool dryRun)
        {
            this.DryRun = dryRun;
        }

        /// <summary>
        /// Gets whether the run was a dry run, which changed nothing in the cache.
        /// </summary>
        public bool DryRun { get; }

        /// <summary>
        /// Gets whether the run was canceled before it was done: the report then tells what it did until then, and the groups it did not
        /// finish were left as they were.
        /// </summary>
        public bool Canceled { get; private set; }

        /// <summary>
        /// Gets the number of archive files written and verified; published too, unless a dry run.
        /// </summary>
        public int GroupsCompacted { get; private set; }

        /// <summary>
        /// Gets how many of the written files are full files: a group-month's first, or a rebuild.
        /// </summary>
        public int FullFilesWritten { get; private set; }

        /// <summary>
        /// Gets how many of the written files are delta generations.
        /// </summary>
        public int DeltasWritten { get; private set; }

        /// <summary>
        /// Gets how many deltas a rebuild folded into its new full file.
        /// </summary>
        public int DeltasFolded { get; private set; }

        /// <summary>
        /// Gets the number of day files folded into a written file.
        /// </summary>
        public long DayFilesAbsorbed { get; private set; }

        /// <summary>
        /// Gets the number of absorbed, matching or dropped day files deleted from the cache.
        /// </summary>
        public long DayFilesDeleted { get; private set; }

        /// <summary>
        /// Gets the number of symbol-days in the written files.
        /// </summary>
        public long SymbolDaysWritten { get; private set; }

        /// <summary>
        /// Gets the number of rows in the written files.
        /// </summary>
        public long RowsWritten { get; private set; }

        /// <summary>
        /// Gets the bytes the written files replace: their day files and previous versions.
        /// </summary>
        public long BytesBefore { get; private set; }

        /// <summary>
        /// Gets the bytes of the written files.
        /// </summary>
        public long BytesAfter { get; private set; }

        /// <summary>
        /// Gets the number of day files left for a later run because their day is younger than <see cref="ArchiveCompactorOptions.MinDayAgeDays"/>.
        /// </summary>
        public long DayFilesTooRecent { get; private set; }

        /// <summary>
        /// Gets the number of absorbed day files kept because they changed, or were gone, by the time the run would delete them; a day
        /// downloaded again is absorbed by the next run.
        /// </summary>
        public long DayFilesChangedDuringRun { get; private set; }

        /// <summary>
        /// Gets the number of day files left out of a delta because they held exactly what the archive already served for their day;
        /// deleted like the absorbed ones, unless a dry run.
        /// </summary>
        public long DayFilesMatchingArchive { get; private set; }

        /// <summary>
        /// Gets the number of empty day files left out of the archive under <see cref="EmptyDayArchiving.Drop"/>; deleted, unless a dry run.
        /// </summary>
        public long EmptyDayFilesDropped { get; private set; }

        /// <summary>
        /// Gets each group that failed, and why.
        /// </summary>
        public IReadOnlyList<ArchiveGroupFailure> FailedGroups => this.Snapshot(this.failedGroups);

        /// <summary>
        /// Gets each day file that could not be read, and why; left on disk and out of the archive.
        /// </summary>
        public IReadOnlyList<UnreadableDayFile> UnreadableDayFiles => this.Snapshot(this.unreadableDayFiles);

        /// <summary>
        /// Gets each file that could not be deleted, under the cache root, left for the next run.
        /// </summary>
        public IReadOnlyList<string> FilesLeft => this.Snapshot(this.filesLeft);

        /// <summary>
        /// Gets each archive file outside the current version that lists a day with records the version does not, or cannot be opened,
        /// under the cache root. It may hold a day's only copy, so it is left for a person to look at, and reported by every real run; a
        /// dry run, which deletes nothing, reports none.
        /// </summary>
        public IReadOnlyList<string> OrphanFiles => this.Snapshot(this.orphanFiles);

        /// <summary>
        /// Gets each folder the run skipped, under the cache root: a dataset folder whose name no dataset has, or a schema folder of a
        /// schema the archive has no columns for.
        /// </summary>
        public IReadOnlyList<string> FoldersSkipped => this.Snapshot(this.foldersSkipped);

        /// <summary>
        /// Gets the day files a day check found suspect, archived all the same, ordered by path.
        /// </summary>
        public IReadOnlyList<SuspectDayFile> Suspects
        {
            get
            {
                lock(this.updateLock)
                {
                    return this.suspects.OrderBy(suspect => suspect.RelativePath, StringComparer.Ordinal).ToList();
                }
            }
        }

        internal void GroupWritten(ArchiveFileKind kind, int dayFilesAbsorbed, bool deltaFolded, int symbolDays, long rows, long bytesBefore, long bytesAfter)
        {
            lock(this.updateLock)
            {
                ++this.GroupsCompacted;
                if(kind == ArchiveFileKind.Full)
                {
                    ++this.FullFilesWritten;
                }
                else
                {
                    ++this.DeltasWritten;
                }

                if(deltaFolded)
                {
                    ++this.DeltasFolded;
                }

                this.DayFilesAbsorbed += dayFilesAbsorbed;
                this.SymbolDaysWritten += symbolDays;
                this.RowsWritten += rows;
                this.BytesBefore += bytesBefore;
                this.BytesAfter += bytesAfter;
            }
        }

        internal void RunCanceled()
        {
            lock(this.updateLock)
            {
                this.Canceled = true;
            }
        }

        internal void GroupFailed(string group, Exception exception)
        {
            this.Add(this.failedGroups, new ArchiveGroupFailure(group, exception));
        }

        internal void DayFileUnreadable(string relativePath, string error)
        {
            this.Add(this.unreadableDayFiles, new UnreadableDayFile(relativePath, error));
        }

        internal void DayFileSuspect(SuspectDayFile suspect)
        {
            this.Add(this.suspects, suspect);
        }

        internal void FileLeft(string path)
        {
            this.Add(this.filesLeft, path);
        }

        internal void FileOrphaned(string relativePath)
        {
            this.Add(this.orphanFiles, relativePath);
        }

        internal void FolderSkipped(string relativePath)
        {
            this.Add(this.foldersSkipped, relativePath);
        }

        internal void DayFileTooRecent()
        {
            lock(this.updateLock)
            {
                ++this.DayFilesTooRecent;
            }
        }

        internal void DayFileDeleted()
        {
            lock(this.updateLock)
            {
                ++this.DayFilesDeleted;
            }
        }

        internal void DayFileChangedDuringRun()
        {
            lock(this.updateLock)
            {
                ++this.DayFilesChangedDuringRun;
            }
        }

        internal void DayFileMatchingArchive()
        {
            lock(this.updateLock)
            {
                ++this.DayFilesMatchingArchive;
            }
        }

        internal void EmptyDayFileDropped()
        {
            lock(this.updateLock)
            {
                ++this.EmptyDayFilesDropped;
            }
        }

        private void Add<T>(List<T> list, T item)
        {
            lock(this.updateLock)
            {
                list.Add(item);
            }
        }

        private IReadOnlyList<T> Snapshot<T>(List<T> list)
        {
            lock(this.updateLock)
            {
                return list.ToList();
            }
        }
    }
}
