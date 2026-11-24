using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Cache;
using Databento.CSharpApiClient.Caching.Compaction;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// Folds a cache's day files into Parquet archive files, one per dataset, schema, group and UTC month: a full file and at most one
    /// delta, by <see cref="ArchiveCompactorOptions.CompactionMode"/>. Each new file is checked by <see cref="ArchiveCompactorOptions.Verification"/>
    /// before it is published, and only then are the files it replaces and its day files deleted, so a reader always finds either the
    /// previous version or the new one. A group that fails is reported and left as it was while the run goes on. A run holds the cache
    /// root's lock, so a second compaction, backup or restore of the root is refused while it goes on.
    /// </summary>
    public sealed class DatabentoArchiveCompactor
    {
        private readonly ArchiveCompactorOptions options;

        /// <summary>
        /// Creates a compactor
        /// </summary>
        /// <param name="options">Run settings, copied: a later change to them does not reach the compactor</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">The cache root is not given, a setting is out of range, or a dry run's folder is not given,
        /// holds the cache root or is inside it</exception>
        public DatabentoArchiveCompactor(ArchiveCompactorOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.options = Copy(options);
            ArgumentException.ThrowIfNullOrEmpty(this.options.CacheRoot, nameof(options));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(this.options.MaxParallelGroups, nameof(options));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(this.options.MaxDeltaGeneration, nameof(options));
            ArgumentOutOfRangeException.ThrowIfNegative(this.options.MaxDeltaSizePercent, nameof(options));
            ArgumentOutOfRangeException.ThrowIfNegative(this.options.MinDayAgeDays, nameof(options));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(this.options.RetireAttempts, nameof(options));
            ArgumentOutOfRangeException.ThrowIfLessThan(this.options.RetireRetryDelay, TimeSpan.Zero, nameof(options));
            if(this.options.TimeProvider == null || this.options.DayChecks == null || this.options.DayChecks.Any(check => check == null))
            {
                throw new ArgumentException("TimeProvider and DayChecks are required, and DayChecks holds no null check.", nameof(options));
            }

            if(!Enum.IsDefined(this.options.CompactionMode) || !Enum.IsDefined(this.options.EmptyDays) || !Enum.IsDefined(this.options.Verification))
            {
                throw new ArgumentException("CompactionMode, EmptyDays and Verification take one of their enum's values.", nameof(options));
            }

            if(this.options.DryRun)
            {
                CacheFolders.ThrowIfDryRunFolderInvalid(this.options.DryRunFolder, this.options.CacheRoot);
            }
        }

        /// <summary>
        /// Gets or sets what is called with each temporary file's path once it is written, before it is verified, so a test can tamper with it.
        /// </summary>
        internal Action<string> TemporaryFileWritten { get; set; }

        /// <summary>
        /// Gets or sets what deletes each file the run retires, <see cref="File.Delete"/> when <c>null</c>, so a test can fail a delete on
        /// any platform.
        /// </summary>
        internal Action<string> DeleteFile { get; set; }

        /// <summary>
        /// Runs the compaction over the whole cache, holding the root's lock file <c>cache.lock</c> while it does. An existing root with
        /// nothing in it has nothing to compact. A canceled run returns what it did until then, with <see cref="ArchiveCompactionReport.Canceled"/> set.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token; checked between groups and symbol-days, in every group in flight</param>
        /// <returns>What the run did</returns>
        /// <exception cref="DirectoryNotFoundException">The cache root does not exist</exception>
        /// <exception cref="InvalidOperationException">A restore of the cache is in progress, or was stopped before it finished; another
        /// compaction, backup or restore holds the root's lock; or the root holds files but no <c>metadata.json</c></exception>
        /// <exception cref="InvalidDataException">The root's <c>metadata.json</c> is not one this package accepts</exception>
        public async Task<ArchiveCompactionReport> CompactAsync(CancellationToken cancellationToken = default)
        {
            CacheRestoreMarker.ThrowIfPresent(this.options.CacheRoot, "the compaction");
            if(!Directory.Exists(this.options.CacheRoot))
            {
                throw new DirectoryNotFoundException(Invariant($"The cache root {this.options.CacheRoot} does not exist."));
            }

            ArchiveCompactionReport report = new ArchiveCompactionReport(this.options.DryRun);
            CacheMetadata metadata = CacheMetadata.Read(this.options.CacheRoot);
            if(metadata == null)
            {
                // a lock file a killed run left, which only Linux leaves behind, is no file of a cache
                if(Directory.EnumerateFileSystemEntries(this.options.CacheRoot).Any(entry => !CacheRootLock.IsLockFile(entry)))
                {
                    throw new InvalidOperationException(Invariant($"{this.options.CacheRoot} holds files but no {CacheMetadata.FileName}, so it is no cache this package wrote."));
                }

                return report;
            }

            using(CacheRootLock.Take(this.options.CacheRoot, "the compaction"))
            {
                // a restore that started before the lock was taken
                CacheRestoreMarker.ThrowIfPresent(this.options.CacheRoot, "the compaction");
                try
                {
                    await this.RunAsync(metadata.DayCutoff, report, cancellationToken).ConfigureAwait(false);
                }
                catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
                {
                    report.RunCanceled();
                }
            }

            return report;
        }

        private static ArchiveCompactorOptions Copy(ArchiveCompactorOptions options)
        {
            return new ArchiveCompactorOptions
            {
                CacheRoot = options.CacheRoot,
                CompactionMode = options.CompactionMode,
                MaxDeltaGeneration = options.MaxDeltaGeneration,
                MaxDeltaSizePercent = options.MaxDeltaSizePercent,
                MinDayAgeDays = options.MinDayAgeDays,
                EmptyDays = options.EmptyDays,
                Verification = options.Verification,
                DayChecks = options.DayChecks?.ToArray(),
                DryRun = options.DryRun,
                DryRunFolder = options.DryRunFolder,
                RetireAttempts = options.RetireAttempts,
                RetireRetryDelay = options.RetireRetryDelay,
                MaxParallelGroups = options.MaxParallelGroups,
                TimeProvider = options.TimeProvider,
                Progress = options.Progress,
            };
        }

        /// <summary>
        /// The <c>yyyy\MM</c> folders under a schema folder, oldest first
        /// </summary>
        private static IEnumerable<(string Directory, DateTime Month)> EnumerateMonthDirectories(string folder)
        {
            foreach(string yearDirectory in Directory.EnumerateDirectories(folder).Order(StringComparer.Ordinal))
            {
                string yearName = Path.GetFileName(yearDirectory);
                if(yearName.Length != 4 || !int.TryParse(yearName, NumberStyles.None, CultureInfo.InvariantCulture, out int year) || year < 1900)
                {
                    continue;
                }

                foreach(string monthDirectory in Directory.EnumerateDirectories(yearDirectory).Order(StringComparer.Ordinal))
                {
                    string monthName = Path.GetFileName(monthDirectory);
                    if(monthName.Length == 2 && int.TryParse(monthName, NumberStyles.None, CultureInfo.InvariantCulture, out int month) && month >= 1 && month <= 12)
                    {
                        yield return (monthDirectory, new DateTime(year, month, 1));
                    }
                }
            }
        }

        /// <summary>
        /// Whether a folder name can be a Databento dataset's, e.g. <c>OPRA.PILLAR</c>: ASCII capitals, digits, <c>.</c> and <c>-</c>, as
        /// Databento names its datasets
        /// </summary>
        private static bool IsDatasetName(string name)
        {
            return name.Length > 0 && name.All(character => char.IsAsciiLetterUpper(character) || char.IsAsciiDigit(character) || character == '.' || character == '-');
        }

        /// <summary>
        /// Tells the progress, if any is to be told; an exception it throws is ignored, since progress is only diagnostic
        /// </summary>
        private static void TellProgress(IProgress<ArchiveCompactionProgress> progress, ArchiveCompactionProgress started)
        {
            try
            {
                progress?.Report(started);
            }
            catch(Exception)
            {
                // a failing progress handler must not stop the run, nor fail a group
            }
        }

        private static async Task CompactGroupAsync(GroupToCompact group, CompactionRun run, CancellationToken cancellationToken)
        {
            try
            {
                await group.Handler.CompactAsync(group.Request, run, cancellationToken).ConfigureAwait(false);
            }
            catch(Exception ex) when(ex is not OperationCanceledException)
            {
                run.Report.GroupFailed(group.Request.DisplayName, ex);
            }
        }

        /// <summary>
        /// Compacts every group of the cache, as many at once as <see cref="ArchiveCompactorOptions.MaxParallelGroups"/> allows
        /// </summary>
        /// <param name="cutoff">The cache's day cutoff tag</param>
        /// <param name="report">The run's report</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task RunAsync(string cutoff, ArchiveCompactionReport report, CancellationToken cancellationToken)
        {
            CompactionRun run = new CompactionRun(this.options, cutoff, report, this.TemporaryFileWritten, this.DeleteFile);
            List<MonthFolder> monthFolders = this.EnumerateMonthFolders(report).ToList();
            CompactionProgress progress = this.PlanRun(monthFolders, run);
            long startTimestamp = this.options.TimeProvider.GetTimestamp();
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.options.MaxParallelGroups,
                CancellationToken = cancellationToken,
            };

            // the groups come one at a time from a lazy listing, so each month folder is listed right before its groups start
            await Parallel.ForEachAsync(this.EnumerateGroups(monthFolders, run), parallelOptions, async (group, groupCancellationToken) =>
            {
                TellProgress(this.options.Progress, progress.StartGroup(group.Request.RelativeDirectory, group.Request.Group, group.Request.DayFiles.Count, this.options.TimeProvider.GetElapsedTime(startTimestamp)));
                await CompactGroupAsync(group, run, groupCancellationToken).ConfigureAwait(false);
                progress.EndGroup(group.Request.DayFiles.Count);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// The month folders of every dataset's schemas, in the order they are compacted; a folder of no dataset or no archived schema is
        /// reported as skipped
        /// </summary>
        private IEnumerable<MonthFolder> EnumerateMonthFolders(ArchiveCompactionReport report)
        {
            foreach(string datasetDirectory in Directory.EnumerateDirectories(this.options.CacheRoot).Order(StringComparer.Ordinal))
            {
                string dataset = Path.GetFileName(datasetDirectory);
                if(!IsDatasetName(dataset))
                {
                    report.FolderSkipped(dataset);
                    continue;
                }

                foreach(string schemaDirectory in Directory.EnumerateDirectories(datasetDirectory).Order(StringComparer.Ordinal))
                {
                    string schema = Path.GetFileName(schemaDirectory);
                    if(!ArchiveSchemaHandlers.TryGet(schema, out IArchiveSchemaHandler handler))
                    {
                        report.FolderSkipped(Path.Combine(dataset, schema));
                        continue;
                    }

                    foreach((string monthDirectory, DateTime month) in EnumerateMonthDirectories(schemaDirectory))
                    {
                        yield return new MonthFolder(handler, dataset, monthDirectory, month);
                    }
                }
            }
        }

        /// <summary>
        /// Lists each month folder's groups as they are asked for, so a group's day files are snapshotted just before it is compacted
        /// </summary>
        private IEnumerable<GroupToCompact> EnumerateGroups(List<MonthFolder> monthFolders, CompactionRun run)
        {
            foreach(MonthFolder monthFolder in monthFolders)
            {
                foreach(ArchiveGroupRequest request in this.TryFindGroups(monthFolder, run))
                {
                    yield return new GroupToCompact(monthFolder.Handler, request);
                }
            }
        }

        /// <summary>
        /// Counts the groups and day files the run will compact, so its progress can be told. Only listed here; a month folder that cannot
        /// be listed is reported when it is compacted.
        /// </summary>
        private CompactionProgress PlanRun(List<MonthFolder> monthFolders, CompactionRun run)
        {
            ArchiveCompactionReport planningReport = new ArchiveCompactionReport(this.options.DryRun);
            int groups = 0;
            long dayFiles = 0;
            foreach(MonthFolder monthFolder in monthFolders)
            {
                try
                {
                    foreach(ArchiveGroupRequest request in this.FindGroups(monthFolder, run.Cutoff, planningReport))
                    {
                        ++groups;
                        dayFiles += request.DayFiles.Count;
                    }
                }
                catch(Exception ex) when(ex is not OperationCanceledException)
                {
                    // reported when the folder is listed again to be compacted
                }
            }

            return new CompactionProgress(groups, dayFiles);
        }

        /// <summary>
        /// Lists a month folder's day files and groups them by the archive file they go into, adding the groups with no day files that still
        /// have work (<see cref="HasWorkWithoutDayFiles"/>). Each group keeps its archive and temporary files as listed, so its run lists
        /// nothing again. A file whose name is no day file of the cache's cutoff and the folder's month is left as it is, and so is a day
        /// file younger than <see cref="ArchiveCompactorOptions.MinDayAgeDays"/>.
        /// </summary>
        private List<ArchiveGroupRequest> FindGroups(MonthFolder monthFolder, string cutoff, ArchiveCompactionReport report)
        {
            int year = monthFolder.Month.Year;
            int month = monthFolder.Month.Month;
            string schema = monthFolder.Handler.Schema;
            DateTime newestEligibleDay = this.options.TimeProvider.GetUtcNow().UtcDateTime.Date.AddDays(-this.options.MinDayAgeDays);
            Dictionary<string, Dictionary<(string RawSymbol, int Day), FoundDayFile>> groups = new Dictionary<string, Dictionary<(string RawSymbol, int Day), FoundDayFile>>(StringComparer.Ordinal);
            Dictionary<string, List<string>> archiveFiles = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach(FileInfo file in new DirectoryInfo(monthFolder.Directory).EnumerateFiles("*", SearchOption.TopDirectoryOnly).OrderBy(file => file.Name, StringComparer.Ordinal))
            {
                if(ArchiveFileName.TryParseGroup(file.Name, monthFolder.Dataset, schema, year, month, cutoff, out string archiveGroup))
                {
                    if(!archiveFiles.TryGetValue(archiveGroup, out List<string> paths))
                    {
                        paths = new List<string>();
                        archiveFiles.Add(archiveGroup, paths);
                    }

                    paths.Add(file.FullName);
                    continue;
                }

                if(!DayFileName.TryParse(file.Name, cutoff, out string rawSymbol, out DateTime date) || date.Year != year || date.Month != month)
                {
                    continue;
                }

                if(this.options.MinDayAgeDays > 0 && date > newestEligibleDay)
                {
                    report.DayFileTooRecent();
                    continue;
                }

                string group = ArchiveGroup.Of(monthFolder.Dataset, rawSymbol);
                if(!groups.TryGetValue(group, out Dictionary<(string RawSymbol, int Day), FoundDayFile> dayFiles))
                {
                    dayFiles = new Dictionary<(string RawSymbol, int Day), FoundDayFile>();
                    groups.Add(group, dayFiles);
                }

                // a name stands for one symbol-day only, so a folder holds at most one day file of each
                dayFiles.Add((rawSymbol, date.Day), new FoundDayFile(file.FullName, rawSymbol, date, file.Length, file.LastWriteTimeUtc));
            }

            foreach(KeyValuePair<string, List<string>> group in archiveFiles.Where(group => !groups.ContainsKey(group.Key) && this.HasWorkWithoutDayFiles(monthFolder, group.Key, group.Value, cutoff)))
            {
                groups.Add(group.Key, new Dictionary<(string RawSymbol, int Day), FoundDayFile>());
            }

            string relativeDirectory = Path.GetRelativePath(this.options.CacheRoot, monthFolder.Directory);
            return groups
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new ArchiveGroupRequest(monthFolder.Directory, relativeDirectory, monthFolder.Dataset, schema, group.Key, year, month, group.Value.Values.ToList(), archiveFiles.GetValueOrDefault(group.Key) ?? new List<string>()))
                .ToList();
        }

        /// <summary>
        /// Gets whether a group with no day files is worth compacting: in <see cref="CompactionMode.Full"/> to fold its delta into a rebuild,
        /// and in a real run to clear what a killed run or a locked delete left (a temporary file, an older generation or version, an
        /// orphan delta)
        /// </summary>
        /// <param name="monthFolder">The group's month folder</param>
        /// <param name="group">The group</param>
        /// <param name="archiveFiles">The group's archive and temporary files, as the month folder's listing found them</param>
        /// <param name="cutoff">The cache's day cutoff tag</param>
        private bool HasWorkWithoutDayFiles(MonthFolder monthFolder, string group, IReadOnlyList<string> archiveFiles, string cutoff)
        {
            string prefix = ArchiveFileName.GetSearchPrefix(monthFolder.Dataset, monthFolder.Handler.Schema, group, monthFolder.Month.Year, monthFolder.Month.Month, cutoff);
            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(archiveFiles, prefix);
            if(this.options.CompactionMode == CompactionMode.Full && version?.DeltaPath != null)
            {
                return true;
            }

            return !this.options.DryRun
                && (ArchiveFileName.FindFiles(archiveFiles, prefix).Count > (version?.Paths.Count ?? 0)
                    || archiveFiles.Any(path => ArchiveFileName.IsTemporaryFileOf(Path.GetFileName(path), prefix)));
        }

        private List<ArchiveGroupRequest> TryFindGroups(MonthFolder monthFolder, CompactionRun run)
        {
            try
            {
                return this.FindGroups(monthFolder, run.Cutoff, run.Report);
            }
            catch(Exception ex) when(ex is not OperationCanceledException)
            {
                // a folder that cannot be listed fails alone, as a group does
                run.Report.GroupFailed(Path.GetRelativePath(this.options.CacheRoot, monthFolder.Directory), ex);
                return new List<ArchiveGroupRequest>();
            }
        }

        /// <summary>
        /// A <c>yyyy\MM</c> folder of a dataset's schema, and its month
        /// </summary>
        private readonly record struct MonthFolder(IArchiveSchemaHandler Handler, string Dataset, string Directory, DateTime Month);

        /// <summary>
        /// A group of day files and the schema it is compacted by
        /// </summary>
        private readonly record struct GroupToCompact(IArchiveSchemaHandler Handler, ArchiveGroupRequest Request);
    }
}
