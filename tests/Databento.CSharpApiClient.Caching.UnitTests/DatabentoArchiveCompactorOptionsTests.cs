// Ignore Spelling: Cbbo

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.Caching.UnitTests.Archive;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// What each setting of <see cref="ArchiveCompactorOptions"/> does to a run, and which settings the compactor refuses
    /// </summary>
    [TestClass]
    public class DatabentoArchiveCompactorOptionsTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private const string OtherCall = "SPXW  261016C06700000";

        private const string NewFullFile = "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021530.full.parquet";

        private const string PreviousFullFile = "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet";

        private static readonly DateTime October15 = new DateTime(2026, 10, 15);

        private static readonly DateTime October16 = new DateTime(2026, 10, 16);

        private static readonly DateTime PreviousBuiltUtc = new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc);

        private static readonly DateTime NowUtc = new DateTime(2026, 11, 19, 2, 15, 30, DateTimeKind.Utc);

        private CacheFixture cache;

        private FakeTimeProvider clock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.cache = new CacheFixture();
            this.clock = new FakeTimeProvider(NowUtc);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.cache.Dispose();
        }

        [TestMethod]
        public void Constructor_NoOptions_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DatabentoArchiveCompactor(null));
        }

        [TestMethod]
        public void Constructor_NoCacheRoot_Throws()
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);

            options.CacheRoot = null;
            Assert.AreEqual("options", Assert.ThrowsException<ArgumentNullException>(() => new DatabentoArchiveCompactor(options)).ParamName);

            options.CacheRoot = string.Empty;
            Assert.AreEqual("options", Assert.ThrowsException<ArgumentException>(() => new DatabentoArchiveCompactor(options)).ParamName);
        }

        [TestMethod]
        [DataRow("MaxParallelGroups")]
        [DataRow("MaxDeltaGeneration")]
        [DataRow("MaxDeltaSizePercent")]
        [DataRow("MinDayAgeDays")]
        [DataRow("RetireAttempts")]
        [DataRow("RetireRetryDelay")]
        public void Constructor_SettingOutOfRange_Throws(string setting)
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            switch(setting)
            {
                case "MaxParallelGroups":
                    options.MaxParallelGroups = 0;
                    break;
                case "MaxDeltaGeneration":
                    options.MaxDeltaGeneration = 0;
                    break;
                case "MaxDeltaSizePercent":
                    options.MaxDeltaSizePercent = -1;
                    break;
                case "MinDayAgeDays":
                    options.MinDayAgeDays = -1;
                    break;
                case "RetireAttempts":
                    options.RetireAttempts = 0;
                    break;
                default:
                    options.RetireRetryDelay = TimeSpan.FromTicks(-1);
                    break;
            }

            ArgumentOutOfRangeException exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DatabentoArchiveCompactor(options));

            Assert.AreEqual("options", exception.ParamName);
        }

        [TestMethod]
        [DataRow("no clock")]
        [DataRow("no checks")]
        [DataRow("a null check")]
        [DataRow("an undefined compaction mode")]
        [DataRow("an undefined empty-day choice")]
        [DataRow("an undefined verification")]
        public void Constructor_MissingOrUndefinedSetting_Throws(string setting)
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            switch(setting)
            {
                case "no clock":
                    options.TimeProvider = null;
                    break;
                case "no checks":
                    options.DayChecks = null;
                    break;
                case "a null check":
                    options.DayChecks = new IArchiveDayCheck[] { new FakeArchiveDayCheck(), null };
                    break;
                case "an undefined compaction mode":
                    options.CompactionMode = (CompactionMode)3;
                    break;
                case "an undefined empty-day choice":
                    options.EmptyDays = (EmptyDayArchiving)2;
                    break;
                default:
                    options.Verification = (ArchiveVerification)3;
                    break;
            }

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => new DatabentoArchiveCompactor(options));

            Assert.AreEqual("options", exception.ParamName);
        }

        [TestMethod]
        [DataRow("inside the cache root")]
        [DataRow("the cache root itself")]
        [DataRow("holding the cache root")]
        [DataRow("empty")]
        public void Constructor_DryRunFolderTheCacheWouldSee_Throws(string dryRunFolder)
        {
            // a reader of the cache would pick up what a dry run wrote into it
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            options.DryRun = true;
            options.DryRunFolder = dryRunFolder switch
            {
                "inside the cache root" => Path.Combine(this.cache.Root, "DryRun"),
                "the cache root itself" => this.cache.Root + Path.DirectorySeparatorChar,
                "holding the cache root" => this.cache.Folder,
                _ => string.Empty,
            };

            Assert.ThrowsException<ArgumentException>(() => new DatabentoArchiveCompactor(options));
        }

        [TestMethod]
        public void Constructor_DryRunWithoutAFolder_Throws()
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            options.DryRun = true;
            options.DryRunFolder = null;

            Assert.ThrowsException<ArgumentNullException>(() => new DatabentoArchiveCompactor(options));
        }

        [TestMethod]
        public void Constructor_RealRun_DoesNotCheckTheDryRunFolder()
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            options.DryRunFolder = Path.Combine(this.cache.Root, "DryRun");

            Assert.IsNotNull(new DatabentoArchiveCompactor(options));
        }

        [TestMethod]
        public void Constructor_SettingsAtTheirLimitsAndAMissingRoot_AreAccepted()
        {
            ArchiveCompactorOptions options = new ArchiveCompactorOptions
            {
                CacheRoot = Path.Combine(this.cache.Folder, "Missing"),
                MaxParallelGroups = 1,
                MaxDeltaGeneration = 1,
                MaxDeltaSizePercent = 0,
                MinDayAgeDays = 0,
                RetireAttempts = 1,
                RetireRetryDelay = TimeSpan.Zero,
                DayChecks = Array.Empty<IArchiveDayCheck>(),
            };

            Assert.IsNotNull(new DatabentoArchiveCompactor(options));
        }

        [TestMethod]
        public async Task Compact_OptionsChangedAfterConstruction_DoNotReachTheRun()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            FakeArchiveDayCheck kept = new FakeArchiveDayCheck();
            FakeArchiveDayCheck added = new FakeArchiveDayCheck();
            FakeArchiveDayCheck replacing = new FakeArchiveDayCheck();
            List<IArchiveDayCheck> checks = new List<IArchiveDayCheck> { kept };
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            options.DayChecks = checks;
            DatabentoArchiveCompactor compactor = new DatabentoArchiveCompactor(options);

            string otherRoot = Path.Combine(this.cache.Folder, "Other");
            options.CacheRoot = otherRoot;
            options.CompactionMode = CompactionMode.Full;
            options.DryRun = true;
            options.Verification = ArchiveVerification.None;
            checks.Add(added);
            ArchiveCompactionReport report = await compactor.CompactAsync();
            options.DayChecks = new IArchiveDayCheck[] { replacing };

            Assert.IsFalse(report.DryRun);
            Assert.AreEqual(1, report.DeltasWritten, "the delta mode it was created with");
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsFalse(File.Exists(dayFile));
            Assert.AreEqual(1, kept.SummarizedDays.Count);
            Assert.AreEqual(0, added.SummarizedDays.Count, "a check added to the list given is not run");
            Assert.AreEqual(0, replacing.SummarizedDays.Count);
            Assert.IsFalse(Directory.Exists(otherRoot));
        }

        [TestMethod]
        public async Task Compact_DayYoungerThanMinDayAge_IsLeftForALaterRun()
        {
            // on November 19, UTC, a day must be November 16 or earlier to be 3 days old
            const string December = "SPXW  261218C06600000";
            string[] dayFiles = Enumerable.Range(16, 4).Select(day => this.cache.SaveCbbo(December, new DateTime(2026, 11, day), day)).ToArray();

            ArchiveCompactionReport report = await this.CompactAsync(options => options.MinDayAgeDays = 3);

            Assert.AreEqual(3L, report.DayFilesTooRecent, "November 17, 18 and 19, each counted once");
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.IsFalse(File.Exists(dayFiles[0]));
            Assert.IsTrue(dayFiles.Skip(1).All(File.Exists));
            CollectionAssert.AreEqual(
                new[] { "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-11.UTC_20261119T021530.full.parquet" },
                this.cache.ArchiveFileNames(this.cache.MonthDirectory(CacheFixture.Opra, CacheFixture.Cbbo, 2026, 11)));
            Assert.AreEqual(16, (await this.cache.ReadServedCbboAsync(December, new DateTime(2026, 11, 16))).Single().Price);
            Assert.IsNull(await this.cache.ReadServedCbboAsync(December, new DateTime(2026, 11, 17)));

            this.clock.Advance(TimeSpan.FromDays(1));
            ArchiveCompactionReport next = await this.CompactAsync(options => options.MinDayAgeDays = 3);

            Assert.AreEqual(2L, next.DayFilesTooRecent);
            Assert.AreEqual(1L, next.DayFilesAbsorbed, "November 17 is 3 days old on the 20th");
            Assert.AreEqual(17, (await this.cache.ReadServedCbboAsync(December, new DateTime(2026, 11, 17))).Single().Price);
        }

        [TestMethod]
        public async Task Compact_NoMinDayAge_ArchivesEveryDayFile()
        {
            const string December = "SPXW  261218C06600000";
            this.cache.SaveCbbo(December, new DateTime(2026, 11, 19), 19);
            this.cache.SaveCbbo(December, new DateTime(2026, 11, 20), 20);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0L, report.DayFilesTooRecent);
            Assert.AreEqual(2L, report.DayFilesAbsorbed, "today's day file too, and a later one");
        }

        [TestMethod]
        public async Task Compact_DropEmptyDays_DeletesTheEmptyDayFilesAndArchivesTheRest()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5, 6600.75);
            this.cache.SaveCbbo(Call, October15);
            this.cache.SaveCbbo(Put, October16);

            ArchiveCompactionReport report = await this.CompactAsync(options => options.EmptyDays = EmptyDayArchiving.Drop);

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1L, report.SymbolDaysWritten);
            Assert.AreEqual(2L, report.RowsWritten);
            Assert.AreEqual(2L, report.EmptyDayFilesDropped);
            Assert.AreEqual(3L, report.DayFilesDeleted);
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Call, October16)).Length);
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Call, October15), "a dropped day is asked for again");
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16));
        }

        [TestMethod]
        public async Task Compact_DropEmptyDaysOfAGroupOfOnlyEmptyDays_PublishesNothingAndDeletesThem()
        {
            string empty = this.cache.SaveCbbo(Call, October15);

            ArchiveCompactionReport report = await this.CompactAsync(options => options.EmptyDays = EmptyDayArchiving.Drop);

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(1L, report.EmptyDayFilesDropped);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            Assert.IsFalse(File.Exists(empty));
            Assert.AreEqual(0, this.cache.CbboArchiveFileNames().Length);
            Assert.AreEqual(0, Directory.GetFiles(this.cache.CbboDirectory, "*.tmp").Length);
        }

        [TestMethod]
        public async Task Compact_DropEmptyDays_LeavesThePreviousDeltasEmptyDaysOutOfTheNextDelta()
        {
            this.WriteVersionWithEmptyDays();
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(options => options.EmptyDays = EmptyDayArchiving.Drop);

            Assert.AreEqual(2L, report.SymbolDaysWritten, "the day file and the previous delta's day with records");
            Assert.AreEqual(0L, report.EmptyDayFilesDropped, "a copy is no day file");
            CollectionAssert.AreEqual(new[] { PreviousDelta(2), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16), "the previous delta's empty day is left out");
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October15)).Length);
            Assert.AreEqual(0, (await this.cache.ReadServedCbboAsync(Call, October15)).Length, "a delta leaves the full file's days as they are");
        }

        [TestMethod]
        public async Task Compact_DropEmptyDays_LeavesTheVersionsEmptyDaysOutOfARebuild()
        {
            this.WriteVersionWithEmptyDays();
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(options =>
            {
                options.EmptyDays = EmptyDayArchiving.Drop;
                options.CompactionMode = CompactionMode.Full;
            });

            Assert.AreEqual(3L, report.SymbolDaysWritten, "the day file and the version's two days with records");
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Call, October15));
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16));
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October15)).Length);
            Assert.AreEqual(1, (await this.cache.ReadServedCbboAsync(OtherCall, October16)).Length);
        }

        [TestMethod]
        [DataRow(CompactionMode.Delta, false, DisplayName = "a delta, the copy in the full file")]
        [DataRow(CompactionMode.Delta, true, DisplayName = "a delta, the copy in the previous delta")]
        [DataRow(CompactionMode.Full, false, DisplayName = "a rebuild, the copy in the full file")]
        [DataRow(CompactionMode.Full, true, DisplayName = "a rebuild, the copy in the delta")]
        public async Task Compact_DropEmptyDays_EmptyDayFileOfADayArchivedWithRecords_KeepsServingThatCopy(CompactionMode mode, bool copyInDelta)
        {
            // an empty download of a day the archive holds with records is dropped, and the day is then served from the archive's copy
            CbboRecordDbn[] putRecords = CacheFixture.CreateCbboRecords(2);
            if(copyInDelta)
            {
                this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
                this.cache.WriteCbboFile(PreviousBuiltUtc, 1, new ArchiveSymbolDay<ICbboRecord>(Put, October16, putRecords));
            }
            else
            {
                this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1), new ArchiveSymbolDay<ICbboRecord>(Put, October16, putRecords));
            }

            string emptyDayFile = this.cache.SaveCbbo(Put, October16);
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(options =>
            {
                options.EmptyDays = EmptyDayArchiving.Drop;
                options.CompactionMode = mode;
            });

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1L, report.EmptyDayFilesDropped);
            Assert.IsFalse(File.Exists(emptyDayFile));
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(putRecords, await this.cache.ReadServedCbboAsync(Put, October16), Put);
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_ArchiveEmptyDays_EmptyDayFileOfADayArchivedWithRecords_IsArchivedAsEmpty()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 2));
            this.cache.SaveCbbo(Put, October16);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(0L, report.EmptyDayFilesDropped);
            Assert.AreEqual(0, (await this.cache.ReadServedCbboAsync(Put, October16)).Length, "the download again is final, empty as it is");
        }

        [TestMethod]
        public async Task Compact_DropEmptyDaysDryRun_CountsTheEmptyDayFileAndDeletesNothing()
        {
            this.cache.SaveCbbo(Call, October15);
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync(options =>
            {
                options.EmptyDays = EmptyDayArchiving.Drop;
                options.DryRun = true;
            });

            Assert.AreEqual(1L, report.EmptyDayFilesDropped);
            Assert.AreEqual(0L, report.DayFilesDeleted);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_CountsOnlyVerification_PublishesAFileWhoseValuesNoLongerMatchTheirSource()
        {
            // the same change fails EveryRecord, see DatabentoArchiveCompactorTests
            this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.Verification = ArchiveVerification.CountsOnly);
            compactor.TemporaryFileWritten = _ => this.cache.SaveCbbo(Call, October16, 6601);

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price, "the file holds what was written");
        }

        [TestMethod]
        public async Task Compact_CountsOnlyVerification_RefusesAFileWhoseIndexDiffers()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.Verification = ArchiveVerification.CountsOnly);
            compactor.TemporaryFileWritten = path =>
            {
                // a whole file of the same version, holding a record more than was written
                using(FileStream stream = File.Create(path))
                {
                    ArchiveFileIdentity identity = new ArchiveFileIdentity(CacheFixture.Opra, CacheFixture.Cbbo, "SPXW.OPT", 2026, 10, "UTC", NowUtc);
                    ArchiveFile.WriteAsync(stream, new CbboArchiveSchema(RType.Cbbo1M), identity, new[] { CacheFixture.CbboDay(Call, 2) }).GetAwaiter().GetResult();
                }
            };

            ArchiveCompactionReport report = await compactor.CompactAsync();

            ArchiveGroupFailure failure = report.FailedGroups.Single();
            Assert.IsInstanceOfType(failure.Exception, typeof(InvalidDataException));
            StringAssert.Contains(failure.Exception.Message, "index");
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_NoVerification_PublishesTheFileAsWritten()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.Verification = ArchiveVerification.None);
            compactor.TemporaryFileWritten = path => File.WriteAllBytes(path, new byte[64]);

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(64L, report.BytesAfter);
            CollectionAssert.AreEqual(new byte[64], File.ReadAllBytes(Path.Combine(this.cache.CbboDirectory, NewFullFile)), "published unread");
            Assert.IsFalse(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_Progress_IsToldAsEachGroupStarts()
        {
            this.cache.SaveCbbo(Call, October16, 1);
            this.cache.SaveCbbo(Put, October16, 2);
            this.cache.SaveCbbo("SPY   261016C00600000", October16, 3);
            FakeProgress<ArchiveCompactionProgress> progress = new FakeProgress<ArchiveCompactionProgress>();
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.Progress = progress);

            // each group takes ten minutes of the run's clock
            compactor.TemporaryFileWritten = _ => this.clock.Advance(TimeSpan.FromMinutes(10));
            await compactor.CompactAsync();

            string folder = Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10");
            Assert.AreEqual(2, progress.Reports.Count);
            ArchiveCompactionProgress first = progress.Reports[0];
            Assert.AreEqual(folder, first.RelativeDirectory);
            Assert.AreEqual("SPXW.OPT", first.Group);
            Assert.AreEqual(2, first.DayFiles);
            Assert.AreEqual(1, first.GroupNumber);
            Assert.AreEqual(2, first.PlannedGroups);
            Assert.AreEqual(0L, first.DayFilesDone);
            Assert.AreEqual(3L, first.PlannedDayFiles);
            Assert.AreEqual(TimeSpan.Zero, first.Elapsed);
            Assert.IsNull(first.EstimatedRemaining);

            ArchiveCompactionProgress second = progress.Reports[1];
            Assert.AreEqual(folder, second.RelativeDirectory);
            Assert.AreEqual("SPY.OPT", second.Group);
            Assert.AreEqual(1, second.DayFiles);
            Assert.AreEqual(2, second.GroupNumber);
            Assert.AreEqual(2, second.PlannedGroups);
            Assert.AreEqual(2L, second.DayFilesDone);
            Assert.AreEqual(3L, second.PlannedDayFiles);
            Assert.AreEqual(TimeSpan.FromMinutes(10), second.Elapsed);
            Assert.AreEqual(TimeSpan.FromMinutes(5), second.EstimatedRemaining, "one day file left at five minutes a day file");
        }

        [TestMethod]
        public async Task Compact_ProgressThatThrows_DoesNotStopTheRun()
        {
            this.cache.SaveCbbo(Call, October16, 1);
            this.cache.SaveCbbo("SPY   261016C00600000", October16, 3);
            FakeProgress<ArchiveCompactionProgress> progress = new FakeProgress<ArchiveCompactionProgress>
            {
                Failure = new InvalidOperationException("The progress handler failed."),
            };

            ArchiveCompactionReport report = await this.CompactAsync(options => options.Progress = progress);

            Assert.AreEqual(2, progress.Reports.Count);
            Assert.AreEqual(2, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count, "progress is diagnostic, so its failure fails no group either");
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
        }

        [TestMethod]
        public async Task Compact_DayFileThatCannotBeDeleted_IsRetriedAfterWaitsOfTheClockThatDoubleUpToAMinute()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options =>
            {
                options.RetireAttempts = 5;
                options.RetireRetryDelay = TimeSpan.FromSeconds(10);
            });
            int deletes = 0;
            compactor.DeleteFile = path =>
            {
                ++deletes;
                throw new IOException("The file is in use.");
            };

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(5, deletes);
            CollectionAssert.AreEqual(
                new[] { TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), TimeSpan.FromMinutes(1) },
                this.clock.Waits.ToArray(),
                "a wait after each failed attempt but the last");
            CollectionAssert.AreEqual(new[] { Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz") }, report.FilesLeft.ToArray());
            Assert.AreEqual(0L, report.DayFilesDeleted);
            Assert.IsTrue(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_DayFileThatCannotBeDeleted_FirstWaitAboveAMinute_IsNeverShortened()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options =>
            {
                options.RetireAttempts = 3;
                options.RetireRetryDelay = TimeSpan.FromMinutes(2);
            });
            compactor.DeleteFile = path => throw new UnauthorizedAccessException("Access is denied.");

            await compactor.CompactAsync();

            CollectionAssert.AreEqual(new[] { TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2) }, this.clock.Waits.ToArray());
        }

        private static string PreviousDelta(int generation)
        {
            return Invariant($"OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.delta.{generation}.parquet");
        }

        /// <summary>
        /// Writes a full file holding the other call's day of a record and the call's empty October 15, and its delta holding the put's
        /// empty October 16 and its October 15 of two records
        /// </summary>
        private void WriteVersionWithEmptyDays()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1), new ArchiveSymbolDay<ICbboRecord>(Call, October15, Array.Empty<ICbboRecord>()));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, new ArchiveSymbolDay<ICbboRecord>(Put, October16, Array.Empty<ICbboRecord>()), new ArchiveSymbolDay<ICbboRecord>(Put, October15, CacheFixture.CreateCbboRecords(2)));
        }

        private DatabentoArchiveCompactor CreateCompactor(Action<ArchiveCompactorOptions> configure = null)
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            configure?.Invoke(options);

            return new DatabentoArchiveCompactor(options);
        }

        private Task<ArchiveCompactionReport> CompactAsync(Action<ArchiveCompactorOptions> configure = null)
        {
            return this.CreateCompactor(configure).CompactAsync();
        }
    }
}
