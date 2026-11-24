using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.Caching.Cache;
using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// Writes the next file of one archive file, by <see cref="CompactionMode"/>: a full file of the day files when there is none, else the
    /// next delta generation (the previous delta overlaid by the day files, leaving out those holding exactly what the archive serves) or a
    /// rebuild (the full file and its delta overlaid by the day files). It is checked by <see cref="ArchiveVerification"/> before it is
    /// published; only then are the files it replaces, any other file outside the current version whose days the version lists, and the
    /// absorbed day files retired. Anything that fails before publishing leaves the folder as it was.
    /// </summary>
    /// <typeparam name="TSource">Record interface</typeparam>
    /// <typeparam name="TRecord">Record type an archive read returns</typeparam>
    /// <typeparam name="TJson">Record type a day file holds</typeparam>
    internal sealed class ArchiveGroupCompaction<TSource, TRecord, TJson>
        where TSource : class, IRecord
        where TRecord : class, TSource
        where TJson : class, TSource
    {
        private static readonly Comparer<(string Symbol, int Day)> IndexOrder = Comparer<(string Symbol, int Day)>.Create(CompareInIndexOrder);

        private readonly ArchiveSchemaHandler<TSource, TRecord, TJson> handler;

        private readonly ArchiveRecordSchema<TSource, TRecord> schema;

        private readonly ArchiveGroupRequest request;

        private readonly CompactionRun run;

        private readonly ArchiveCompactorOptions options;

        private readonly IReadOnlyList<IArchiveDayCheck> checks;

        /// <summary>
        /// Where each symbol-day of the new file comes from: its day file, or <c>null</c> for the current version's copy
        /// </summary>
        private readonly SortedDictionary<(string Symbol, int Day), FoundDayFile> sources = new SortedDictionary<(string Symbol, int Day), FoundDayFile>(IndexOrder);

        /// <summary>
        /// The current version's files, the full file first, then its current delta; opened on every run, so one that cannot be read fails the group
        /// </summary>
        private readonly List<ArchiveFileReader<TSource, TRecord>> currentFiles = new List<ArchiveFileReader<TSource, TRecord>>();

        /// <summary>
        /// The day files left out of a delta because they hold exactly what the version already serves for their day; retired like the absorbed ones
        /// </summary>
        private readonly List<FoundDayFile> matchingDayFiles = new List<FoundDayFile>();

        /// <summary>
        /// The empty day files left out under <see cref="EmptyDayArchiving.Drop"/>; retired whether or not a file is published
        /// </summary>
        private readonly List<FoundDayFile> droppedDayFiles = new List<FoundDayFile>();

        /// <summary>
        /// Each check's summaries of the days the new file holds, in the order of <see cref="checks"/>
        /// </summary>
        private readonly Dictionary<(string Symbol, DateOnly Date), object>[] summaries;

        /// <summary>
        /// Whether the run writes a new full file of the current version's symbol-days rather than the next delta generation
        /// </summary>
        private bool rebuild;

        public ArchiveGroupCompaction(ArchiveSchemaHandler<TSource, TRecord, TJson> handler, ArchiveGroupRequest request, CompactionRun run)
        {
            this.handler = handler;
            this.schema = handler.RecordSchema;
            this.request = request;
            this.run = run;
            this.options = run.Options;
            this.checks = run.GetChecks(request.Dataset, request.Schema);
            this.summaries = this.checks.Select(_ => new Dictionary<(string Symbol, DateOnly Date), object>()).ToArray();
        }

        private bool DropsEmptyDays => this.options.EmptyDays == EmptyDayArchiving.Drop;

        /// <summary>
        /// Writes the next file and, unless a dry run, publishes it and retires what it replaces
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="InvalidDataException">A file of the current version cannot be read, or the new one does not match its sources; nothing was changed</exception>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            // the files the month folder's listing found: only a run holding the cache root's lock writes archive files
            string prefix = ArchiveFileName.GetSearchPrefix(this.request.Dataset, this.request.Schema, this.request.Group, this.request.Year, this.request.Month, this.run.Cutoff);
            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.request.ArchiveFiles, prefix);
            IReadOnlyList<string> files = ArchiveFileName.FindFiles(this.request.ArchiveFiles, prefix);
            if(!this.options.DryRun)
            {
                this.DeleteLeftTemporaryFiles(prefix);
            }

            PublishedFile published = null;
            try
            {
                foreach(string path in version == null ? Enumerable.Empty<string>() : version.Paths)
                {
                    this.currentFiles.Add(await ArchiveFile.OpenAsync(path, this.schema).ConfigureAwait(false));
                }

                this.rebuild = this.ChooseRebuild(version);
                this.PlanSources();
                if(version != null && !this.rebuild)
                {
                    await this.SetAsideMatchingDayFilesAsync(cancellationToken).ConfigureAwait(false);
                }

                if(this.sources.Values.Any(dayFile => dayFile != null) || this.FoldsDelta(version))
                {
                    published = await this.WriteAndPublishAsync(prefix, version, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                // the handles go before the retire deletes the files
                foreach(ArchiveFileReader<TSource, TRecord> currentFile in this.currentFiles)
                {
                    await currentFile.DisposeAsync().ConfigureAwait(false);
                }
            }

            if(!this.options.DryRun)
            {
                // the version as it stands now: a new full file alone, a new delta generation over the full file, or the version unchanged
                HashSet<string> kept = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<ArchiveIndex> currentVersion = new List<ArchiveIndex>();
                if(published != null)
                {
                    kept.Add(published.Path);
                    currentVersion.Add(published.Index);
                }

                if(version != null && published == null)
                {
                    kept.UnionWith(version.Paths);
                    currentVersion.AddRange(this.currentFiles.Select(currentFile => currentFile.Index));
                }
                else if(version != null && !this.rebuild)
                {
                    kept.Add(version.FullPath);
                    currentVersion.Add(this.currentFiles[0].Index);
                }

                // the indexes of the files the run opened, the files a new one replaces among them, so those are not opened again
                Dictionary<string, ArchiveIndex> openedIndexes = new Dictionary<string, ArchiveIndex>(StringComparer.OrdinalIgnoreCase);
                for(int i = 0; i < this.currentFiles.Count; ++i)
                {
                    openedIndexes[version.Paths[i]] = this.currentFiles[i].Index;
                }

                IEnumerable<FoundDayFile> absorbed = published != null ? this.sources.Values.Where(dayFile => dayFile != null) : Enumerable.Empty<FoundDayFile>();
                await this.RetireAsync(files.Where(path => !kept.Contains(path)).ToList(), currentVersion, openedIndexes, absorbed.Concat(this.matchingDayFiles).Concat(this.droppedDayFiles).ToList(), cancellationToken).ConfigureAwait(false);
            }
        }

        private static int CompareInIndexOrder((string Symbol, int Day) first, (string Symbol, int Day) second)
        {
            int bySymbol = string.CompareOrdinal(first.Symbol, second.Symbol);
            return bySymbol != 0 ? bySymbol : first.Day.CompareTo(second.Day);
        }

        /// <summary>
        /// Deletes the temporary files a killed run left; only one run writes a folder at a time, so none is still being written
        /// </summary>
        private void DeleteLeftTemporaryFiles(string prefix)
        {
            foreach(string path in this.request.ArchiveFiles.Where(path => ArchiveFileName.IsTemporaryFileOf(Path.GetFileName(path), prefix)))
            {
                ArchiveFileOperations.TryDeleteOnce(path);
            }
        }

        /// <summary>
        /// A new full file's build time: now, or a second after the newest build any file in the folder carries if that is not in the past,
        /// since only the newest full file is read and a left delta must never pass for one of the new full file
        /// </summary>
        private DateTime GetBuildTime(DateTime? latestBuildUtc)
        {
            DateTime now = this.options.TimeProvider.GetUtcNow().UtcDateTime;
            DateTime builtUtc = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            if(latestBuildUtc.HasValue && builtUtc <= latestBuildUtc.Value)
            {
                builtUtc = latestBuildUtc.Value.AddSeconds(1);
            }

            return builtUtc;
        }

        /// <summary>
        /// Writes the new file under a temporary name, checks it and publishes it
        /// </summary>
        /// <returns>The published file, or <c>null</c> when no day file could be read or kept, so there was nothing to publish</returns>
        private async Task<PublishedFile> WriteAndPublishAsync(string prefix, ArchiveVersion version, CancellationToken cancellationToken)
        {
            ArchiveFileIdentity identity = version == null || this.rebuild
                ? new ArchiveFileIdentity(this.request.Dataset, this.request.Schema, this.request.Group, this.request.Year, this.request.Month, this.run.Cutoff, this.GetBuildTime(ArchiveFileName.FindLatestBuildTime(this.request.ArchiveFiles, prefix)))
                : new ArchiveFileIdentity(this.request.Dataset, this.request.Schema, this.request.Group, this.request.Year, this.request.Month, this.run.Cutoff, version.BuiltUtc, version.DeltaGeneration + 1);
            string outputDirectory = this.options.DryRun ? Path.Combine(this.options.DryRunFolder, this.request.RelativeDirectory) : this.request.Directory;
            Directory.CreateDirectory(outputDirectory);
            string publishedPath = Path.Combine(outputDirectory, identity.FileName);
            string temporaryPath = Path.Combine(outputDirectory, ArchiveFileName.GetTemporaryFileName(identity.FileName));

            ArchiveIndex index;
            int dayFilesAbsorbed;
            List<SuspectDayFile> suspects;
            try
            {
                index = await this.WriteAsync(temporaryPath, identity, cancellationToken).ConfigureAwait(false);
                this.run.TemporaryFileWritten?.Invoke(temporaryPath);
                await this.VerifyAsync(temporaryPath, index, cancellationToken).ConfigureAwait(false);

                dayFilesAbsorbed = this.sources.Values.Count(dayFile => dayFile != null);
                if(dayFilesAbsorbed == 0 && !this.FoldsDelta(version))
                {
                    // every day file was unreadable or dropped, so the new file would add nothing to the current version
                    return null;
                }

                suspects = this.FindSuspects(index);

                // a dry run's folder is never read, so its file is named as a real one would be, for a query to find
                File.Move(temporaryPath, publishedPath, overwrite: this.options.DryRun);
            }
            finally
            {
                ArchiveFileOperations.TryDeleteOnce(temporaryPath);
            }

            // reported once the days are published, so a group that fails lists none of them
            foreach(SuspectDayFile suspect in suspects)
            {
                this.run.Report.DayFileSuspect(suspect);
            }

            // the files the new one replaces: the whole version for a rebuild, the previous generation for a delta
            IEnumerable<string> replaced = version == null ? Enumerable.Empty<string>() : this.rebuild ? version.Paths : version.Paths.Skip(1);
            long bytesBefore = this.sources.Values.Where(dayFile => dayFile != null).Sum(dayFile => dayFile.Length) + replaced.Sum(path => new FileInfo(path).Length);
            bool deltaFolded = this.rebuild && this.currentFiles.Count > 1;
            this.run.Report.GroupWritten(identity.Kind, dayFilesAbsorbed, deltaFolded, index.Entries.Count, index.RowCount, bytesBefore, new FileInfo(publishedPath).Length);
            return new PublishedFile(publishedPath, index);
        }

        /// <summary>
        /// Runs the checks on the new file's day-file days, each against every day the group-month serves once the file is published. The
        /// suspect days are archived all the same: downloading one again saves a day file that is read ahead of the archive.
        /// </summary>
        /// <returns>The day files the suspect days came from, each once</returns>
        private List<SuspectDayFile> FindSuspects(ArchiveIndex index)
        {
            List<SuspectDayFile> found = new List<SuspectDayFile>();
            if(this.checks.Count == 0)
            {
                return found;
            }

            // a new delta generation is served over the full file, in place of the previous one; a new full file alone
            Dictionary<(string Symbol, int Day), ArchiveIndexEntry> served = new Dictionary<(string Symbol, int Day), ArchiveIndexEntry>();
            foreach(ArchiveIndexEntry entry in (this.rebuild ? Enumerable.Empty<ArchiveFileReader<TSource, TRecord>>() : this.currentFiles.Take(1)).SelectMany(file => file.Index.Entries).Concat(index.Entries))
            {
                served[(entry.Symbol, entry.Day)] = entry;
            }

            List<ArchiveDay> servedDays = served.Values.Select(this.ToArchiveDay).ToList();
            List<ArchiveDay> dayFileDays = index.Entries.Where(entry => this.sources[(entry.Symbol, entry.Day)] != null).Select(this.ToArchiveDay).ToList();
            HashSet<(string Symbol, int Day)> reported = new HashSet<(string Symbol, int Day)>();
            for(int i = 0; i < this.checks.Count; ++i)
            {
                ArchiveGroupDays days = new ArchiveGroupDays(this.request.Dataset, this.request.Schema, this.request.Group, this.request.Year, this.request.Month, servedDays, dayFileDays, this.summaries[i]);
                foreach(ArchiveDaySuspect suspect in this.checks[i].FindSuspects(days) ?? Enumerable.Empty<ArchiveDaySuspect>())
                {
                    if(suspect == null || suspect.Day.Date.Year != this.request.Year || suspect.Day.Date.Month != this.request.Month)
                    {
                        continue;
                    }

                    (string Symbol, int Day) key = (suspect.Day.Symbol, suspect.Day.Date.Day);
                    if(!this.sources.TryGetValue(key, out FoundDayFile dayFile) || dayFile == null || !reported.Add(key))
                    {
                        // not a day-file day, or found by an earlier check
                        continue;
                    }

                    found.Add(new SuspectDayFile(this.ToRelativePath(dayFile), suspect, dayFile.LastWriteUtc));
                }
            }

            return found;
        }

        private ArchiveDay ToArchiveDay(ArchiveIndexEntry entry)
        {
            return new ArchiveDay(entry.Symbol, new DateOnly(this.request.Year, this.request.Month, entry.Day), entry.RecordCount);
        }

        private string ToRelativePath(FoundDayFile dayFile)
        {
            return this.ToRelativePath(dayFile.Path);
        }

        private string ToRelativePath(string path)
        {
            return Path.GetRelativePath(this.options.CacheRoot, path);
        }

        /// <summary>
        /// Decides between a rebuild and the next delta generation by the compaction mode; <see cref="CompactionMode.Auto"/> rebuilds once the
        /// next generation would reach <see cref="ArchiveCompactorOptions.MaxDeltaGeneration"/> or the delta outgrew its share of the full file
        /// </summary>
        /// <returns><c>false</c> when there is no full file, so the run writes one of the day files</returns>
        private bool ChooseRebuild(ArchiveVersion version)
        {
            if(version == null || this.options.CompactionMode == CompactionMode.Delta)
            {
                return false;
            }

            if(this.options.CompactionMode == CompactionMode.Full || version.DeltaGeneration + 1 >= this.options.MaxDeltaGeneration)
            {
                return true;
            }

            if(version.DeltaPath == null)
            {
                return false;
            }

            long deltaLength = new FileInfo(version.DeltaPath).Length;
            long fullLength = new FileInfo(version.FullPath).Length;
            return deltaLength * 100 > fullLength * this.options.MaxDeltaSizePercent;
        }

        /// <summary>
        /// Gets whether the run folds a delta into a new full file even with no day file to add, which only <see cref="CompactionMode.Full"/>
        /// does; <see cref="CompactionMode.Auto"/> rebuilds only to take new day files
        /// </summary>
        private bool FoldsDelta(ArchiveVersion version)
        {
            return this.rebuild && this.options.CompactionMode == CompactionMode.Full && version?.DeltaPath != null;
        }

        /// <summary>
        /// Plans the new file's symbol-days: a rebuild's are the full file's and its delta's, the next delta generation's the previous delta's,
        /// each overlaid by the day files
        /// </summary>
        private void PlanSources()
        {
            foreach(ArchiveFileReader<TSource, TRecord> currentFile in this.rebuild ? this.currentFiles : this.currentFiles.Skip(1))
            {
                foreach(ArchiveIndexEntry entry in currentFile.Index.Entries)
                {
                    this.sources[(entry.Symbol, entry.Day)] = null;
                }
            }

            foreach(FoundDayFile dayFile in this.request.DayFiles)
            {
                this.sources[(dayFile.RawSymbol, dayFile.Date.Day)] = dayFile;
            }
        }

        /// <summary>
        /// Finds the current version's file the new file copies a symbol-day from: for a rebuild the delta when it lists the day, else the
        /// full file; for the next delta generation the previous delta alone
        /// </summary>
        /// <returns>The file, or <c>null</c> when none of those lists the day</returns>
        private ArchiveFileReader<TSource, TRecord> FindCurrentCopy((string Symbol, int Day) symbolDay)
        {
            if(this.rebuild)
            {
                return this.FindServedCopy(symbolDay);
            }

            return this.currentFiles.Count > 1 && this.currentFiles[1].Index.TryLocate(symbolDay.Symbol, symbolDay.Day, out _) ? this.currentFiles[1] : null;
        }

        /// <summary>
        /// Finds the current version's file the archive serves a symbol-day from: the delta when it lists the day, else the full file
        /// </summary>
        /// <returns>The file, or <c>null</c> when none lists the day</returns>
        private ArchiveFileReader<TSource, TRecord> FindServedCopy((string Symbol, int Day) symbolDay)
        {
            for(int i = this.currentFiles.Count - 1; i >= 0; --i)
            {
                if(this.currentFiles[i].Index.TryLocate(symbolDay.Symbol, symbolDay.Day, out _))
                {
                    return this.currentFiles[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Takes out of a delta's sources the day files holding exactly, record by record, what the version serves for their day (the delta's
        /// copy, else the full file's), e.g. a download again that brought back the same data; they are retired without another copy, and a
        /// day the previous delta holds keeps its copy. One that cannot be read or holds a record that cannot be written stays, for the
        /// write to report, and so does an empty one under <see cref="EmptyDayArchiving.Drop"/>, for the write to drop.
        /// </summary>
        private async Task SetAsideMatchingDayFilesAsync(CancellationToken cancellationToken)
        {
            foreach(KeyValuePair<(string Symbol, int Day), FoundDayFile> source in this.sources.Where(planned => planned.Value != null).ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();

                ArchiveFileReader<TSource, TRecord> served = this.FindServedCopy(source.Key);
                if(served == null)
                {
                    continue;
                }

                TJson[] dayFileRecords;
                try
                {
                    dayFileRecords = await DayFile.ReadAsync(source.Value.Path, this.handler.ReadDayFile, cancellationToken).ConfigureAwait(false);
                }
                catch(Exception ex) when(IsUnreadable(ex))
                {
                    continue;
                }

                if((dayFileRecords.Length == 0 && this.DropsEmptyDays) || dayFileRecords.Any(record => record != null && this.schema.FindUnwritableValue(record) != null))
                {
                    // comparing an unwritable record could throw, as reading its value does
                    continue;
                }

                TRecord[] servedRecords = await served.ReadSymbolDayAsync(source.Key.Symbol, new DateTime(this.request.Year, this.request.Month, source.Key.Day)).ConfigureAwait(false);
                if(this.HoldSameRecords(dayFileRecords, servedRecords))
                {
                    if(this.FindCurrentCopy(source.Key) != null)
                    {
                        this.sources[source.Key] = null;
                    }
                    else
                    {
                        this.sources.Remove(source.Key);
                    }

                    this.matchingDayFiles.Add(source.Value);
                    this.run.Report.DayFileMatchingArchive();
                }
            }
        }

        private static bool IsUnreadable(Exception ex)
        {
            return ex is InvalidDataException || ex is JsonException || ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException;
        }

        private bool HoldSameRecords(TJson[] dayFileRecords, TRecord[] servedRecords)
        {
            if(servedRecords == null || dayFileRecords.Length != servedRecords.Length || dayFileRecords.Any(record => record == null))
            {
                return false;
            }

            for(int i = 0; i < dayFileRecords.Length; ++i)
            {
                if(this.schema.FindDifferentColumn(dayFileRecords[i], servedRecords[i]) != null)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<ArchiveIndex> WriteAsync(string temporaryPath, ArchiveFileIdentity identity, CancellationToken cancellationToken)
        {
            FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            await using(stream.ConfigureAwait(false))
            {
                ArchiveIndex index = await ArchiveFile.WriteInOrderAsync(stream, this.schema, identity, this.LoadSymbolDaysAsync(cancellationToken)).ConfigureAwait(false);

                // on disk before it is published and anything it replaces is deleted, not only in the cache a power loss would empty
                stream.Flush(flushToDisk: true);
                return index;
            }
        }

        /// <summary>
        /// Loads the new file's symbol-days one at a time, in index order, and has the checks summarize each. A day file that cannot be read
        /// or written is left on disk and out of the new file, which keeps the copy it would have replaced: the previous delta's for a delta,
        /// the version's for a rebuild, if any. Under <see cref="EmptyDayArchiving.Drop"/> an empty day file is left out the same way, and
        /// deleted, and an empty copy is left out of the new file.
        /// </summary>
        private async IAsyncEnumerable<ArchiveSymbolDay<TSource>> LoadSymbolDaysAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach(KeyValuePair<(string Symbol, int Day), FoundDayFile> source in this.sources.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();

                DateTime date = new DateTime(this.request.Year, this.request.Month, source.Key.Day);
                if(source.Value != null)
                {
                    ArchiveSymbolDay<TSource> dayFileDay = await this.TryReadDayFileAsync(source.Value, cancellationToken).ConfigureAwait(false);
                    if(dayFileDay != null && dayFileDay.Records.Length == 0 && this.DropsEmptyDays)
                    {
                        // dropped like an unreadable one: the day keeps the copy it would have replaced, if any
                        this.droppedDayFiles.Add(source.Value);
                        this.run.Report.EmptyDayFileDropped();
                    }
                    else if(dayFileDay != null)
                    {
                        this.Summarize(dayFileDay);
                        yield return dayFileDay;
                        continue;
                    }

                    if(this.FindCurrentCopy(source.Key) == null)
                    {
                        this.sources.Remove(source.Key);
                        continue;
                    }

                    this.sources[source.Key] = null;
                }

                TRecord[] copiedRecords = await this.FindCurrentCopy(source.Key).ReadSymbolDayAsync(source.Key.Symbol, date).ConfigureAwait(false);
                if(copiedRecords.Length == 0 && this.DropsEmptyDays)
                {
                    this.sources.Remove(source.Key);
                    continue;
                }

                ArchiveSymbolDay<TSource> copiedDay = new ArchiveSymbolDay<TSource>(source.Key.Symbol, date, copiedRecords);
                this.Summarize(copiedDay);
                yield return copiedDay;
            }
        }

        private void Summarize(ArchiveSymbolDay<TSource> symbolDay)
        {
            if(this.checks.Count == 0)
            {
                return;
            }

            // the records go on to be written, so a check gets them read-only
            ArchiveDay day = new ArchiveDay(symbolDay.Symbol, DateOnly.FromDateTime(symbolDay.Date), symbolDay.Records.Length);
            IReadOnlyList<IRecord> records = Array.AsReadOnly(symbolDay.Records);
            for(int i = 0; i < this.checks.Count; ++i)
            {
                object summary = this.checks[i].Summarize(day, records);
                if(summary != null)
                {
                    this.summaries[i][(day.Symbol, day.Date)] = summary;
                }
            }
        }

        private async Task<ArchiveSymbolDay<TSource>> TryReadDayFileAsync(FoundDayFile dayFile, CancellationToken cancellationToken)
        {
            try
            {
                TJson[] records = await DayFile.ReadAsync(dayFile.Path, this.handler.ReadDayFile, cancellationToken).ConfigureAwait(false);
                foreach(TJson record in records)
                {
                    string problem = this.schema.FindUnwritableValue(record);
                    if(problem != null)
                    {
                        throw new InvalidDataException(problem);
                    }
                }

                // refuses records of another symbol
                return new ArchiveSymbolDay<TSource>(dayFile.RawSymbol, dayFile.Date, records);
            }
            catch(Exception ex) when(IsUnreadable(ex))
            {
                this.run.Report.DayFileUnreadable(this.ToRelativePath(dayFile), ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Checks the new file by <see cref="ArchiveCompactorOptions.Verification"/>: its index read back against the index written and the
        /// symbol-days merged, and for <see cref="ArchiveVerification.EveryRecord"/> each symbol-day record by record, column by column,
        /// against its source read again
        /// </summary>
        private async Task VerifyAsync(string temporaryPath, ArchiveIndex index, CancellationToken cancellationToken)
        {
            if(this.options.Verification == ArchiveVerification.None)
            {
                return;
            }

            FileStream stream = new FileStream(temporaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using(stream.ConfigureAwait(false))
            {
                ArchiveFileReader<TSource, TRecord> written = await ArchiveFile.OpenAsync(stream, this.schema).ConfigureAwait(false);
                await using(written.ConfigureAwait(false))
                {
                    if(!string.Equals(written.Index.ToJson(), index.ToJson(), StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("The index read back is not the index written.");
                    }

                    if(!written.Index.Entries.Select(entry => (entry.Symbol, entry.Day)).SequenceEqual(this.sources.Keys))
                    {
                        throw new InvalidDataException("The file does not hold exactly the symbol-days merged.");
                    }

                    if(this.options.Verification == ArchiveVerification.CountsOnly)
                    {
                        return;
                    }

                    foreach(ArchiveIndexEntry entry in written.Index.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        DateTime date = new DateTime(this.request.Year, this.request.Month, entry.Day);
                        FoundDayFile dayFile = this.sources[(entry.Symbol, entry.Day)];
                        TSource[] expected = dayFile != null
                            ? await DayFile.ReadAsync(dayFile.Path, this.handler.ReadDayFile, cancellationToken).ConfigureAwait(false)
                            : await this.FindCurrentCopy((entry.Symbol, entry.Day)).ReadSymbolDayAsync(entry.Symbol, date).ConfigureAwait(false);
                        TRecord[] actual = await written.ReadSymbolDayAsync(entry.Symbol, date).ConfigureAwait(false);
                        if(expected == null || actual == null || expected.Length != actual.Length)
                        {
                            throw new InvalidDataException(Invariant($"{entry.Symbol} {date:yyyy-MM-dd} reads back {actual?.Length} records, its source holds {expected?.Length}."));
                        }

                        for(int i = 0; i < expected.Length; ++i)
                        {
                            string column = this.schema.FindDifferentColumn(expected[i], actual[i]);
                            if(column != null)
                            {
                                throw new InvalidDataException(Invariant($"{entry.Symbol} {date:yyyy-MM-dd} record {i} reads back a different {column} than its source."));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retires the files outside the current version whose days it lists (the files the published one replaces, and what a killed run
        /// left) and the day files the version now holds (absorbed, or matching what it served) or that were dropped, each day file only
        /// while still as the run found it. The published file was checked; until a backup holds it, it is the only copy of the absorbed
        /// day files, as they were before the run.
        /// </summary>
        /// <param name="outsideFiles">The full files and deltas outside the version as it stands after the run</param>
        /// <param name="currentVersion">The indexes of the version's files as it stands after the run</param>
        /// <param name="openedIndexes">The indexes of the files the run opened, by path</param>
        /// <param name="dayFiles">The day files to retire</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task RetireAsync(IReadOnlyList<string> outsideFiles, IReadOnlyList<ArchiveIndex> currentVersion, IReadOnlyDictionary<string, ArchiveIndex> openedIndexes, IReadOnlyList<FoundDayFile> dayFiles, CancellationToken cancellationToken)
        {
            foreach(string path in outsideFiles)
            {
                if(!await this.IsListedByVersionAsync(path, currentVersion, openedIndexes).ConfigureAwait(false))
                {
                    // it may hold a day's only copy, such as a delta whose full file is gone
                    this.run.Report.FileOrphaned(this.ToRelativePath(path));
                    continue;
                }

                if(await this.run.FileOperations.DeleteAsync(path, null, cancellationToken).ConfigureAwait(false) != DeleteResult.Deleted)
                {
                    this.run.Report.FileLeft(this.ToRelativePath(path));
                }
            }

            foreach(FoundDayFile dayFile in dayFiles)
            {
                // checked before every attempt: a day downloaded again during a retry's wait is the next run's to absorb
                switch(await this.run.FileOperations.DeleteAsync(dayFile.Path, () => IsAsFound(dayFile), cancellationToken).ConfigureAwait(false))
                {
                    case DeleteResult.Deleted:
                        this.run.Report.DayFileDeleted();
                        break;
                    case DeleteResult.Changed:
                        this.run.Report.DayFileChangedDuringRun();
                        break;
                    default:
                        this.run.Report.FileLeft(this.ToRelativePath(dayFile));
                        break;
                }
            }
        }

        /// <summary>
        /// Gets whether a day file is still there as the run found it: of the size and last write time it had
        /// </summary>
        private static bool IsAsFound(FoundDayFile dayFile)
        {
            FileInfo file = new FileInfo(dayFile.Path);
            return file.Exists && file.Length == dayFile.Length && file.LastWriteTimeUtc == dayFile.LastWriteUtc;
        }

        /// <summary>
        /// Gets whether the current version lists every symbol-day a file outside it lists with records, so deleting the file loses no
        /// data. The files a rebuild or a delta replaces do by construction. A file that cannot be opened does not.
        /// </summary>
        private async Task<bool> IsListedByVersionAsync(string path, IReadOnlyList<ArchiveIndex> currentVersion, IReadOnlyDictionary<string, ArchiveIndex> openedIndexes)
        {
            if(!openedIndexes.TryGetValue(path, out ArchiveIndex index))
            {
                try
                {
                    ArchiveFileReader<TSource, TRecord> reader = await ArchiveFile.OpenAsync(path, this.schema).ConfigureAwait(false);
                    await using(reader.ConfigureAwait(false))
                    {
                        index = reader.Index;
                    }
                }
                catch(Exception ex) when(ex is InvalidDataException || ex is IOException || ex is UnauthorizedAccessException)
                {
                    return false;
                }
            }

            return index.Entries
                .Where(entry => entry.RecordCount > 0)
                .All(entry => currentVersion.Any(currentFile => currentFile.TryLocate(entry.Symbol, entry.Day, out _)));
        }

        /// <summary>
        /// A file the run published, and the index it was written with
        /// </summary>
        private sealed record PublishedFile(string Path, ArchiveIndex Index);
    }
}
