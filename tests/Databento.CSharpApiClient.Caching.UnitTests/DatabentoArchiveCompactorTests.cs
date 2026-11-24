// Ignore Spelling: Cbbo Tcbbo Ohlcv Bbo

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.Caching.Cache;
using Databento.CSharpApiClient.Caching.UnitTests.Archive;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// Day files folded, by the compaction mode, into a verified and published full file or next delta generation that serves what the day
    /// files held, and every failure leaving the cache as it was
    /// </summary>
    [TestClass]
    public class DatabentoArchiveCompactorTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private const string OtherCall = "SPXW  261016C06700000";

        private const string SpyCall = "SPY   261016C00600000";

        private const string NewFullFile = "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021530.full.parquet";

        private const string PreviousFullFile = "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet";

        private static readonly DateTime October15 = new DateTime(2026, 10, 15);

        private static readonly DateTime October16 = new DateTime(2026, 10, 16);

        private static readonly DateTime PreviousBuiltUtc = new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc);

        private static readonly DateTime NowUtc = new DateTime(2026, 11, 19, 2, 15, 30, DateTimeKind.Utc);

        private static readonly string GroupName = Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10") + " SPXW.OPT";

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
        public async Task Compact_DayFiles_ArePublishedAsAFullFileTheArchiveServesAndDeleted()
        {
            string[] dayFiles =
            {
                this.cache.SaveCbbo(Call, October16, 6600.25, 6600.5, 6600.75),
                this.cache.SaveCbbo(Put, October16, 12.5),
                this.cache.SaveCbbo(Call, October15),
            };
            long dayFileBytes = dayFiles.Sum(path => new FileInfo(path).Length);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1, report.FullFilesWritten);
            Assert.AreEqual(0, report.DeltasWritten);
            Assert.AreEqual(3L, report.DayFilesAbsorbed);
            Assert.AreEqual(3L, report.DayFilesDeleted);
            Assert.AreEqual(3L, report.SymbolDaysWritten);
            Assert.AreEqual(4L, report.RowsWritten);
            Assert.AreEqual(dayFileBytes, report.BytesBefore);
            Assert.AreEqual(new FileInfo(Path.Combine(this.cache.CbboDirectory, NewFullFile)).Length, report.BytesAfter);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsFalse(report.DryRun);
            Assert.IsFalse(report.Canceled);
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length, "every absorbed day file is deleted");
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());

            CollectionAssert.AreEqual(new[] { 6600.25, 6600.5, 6600.75 }, (await this.cache.ReadServedCbboAsync(Call, October16)).Select(record => record.Price).ToArray());
            Assert.AreEqual(12.5, (await this.cache.ReadServedCbboAsync(Put, October16)).Single().Price);
            Assert.AreEqual(0, (await this.cache.ReadServedCbboAsync(Call, October15)).Length, "an empty day stays an empty, final day");
        }

        [TestMethod]
        public async Task Compact_DayFiles_KeepEveryValueOfTheirRecords()
        {
            CbboRecordJson[] records = { CacheFixture.CbboRecord(October16, 6600.25, 0), CacheFixture.CbboRecord(October16, 6600.5, 1) };
            records[1].Side = TradeAggressor.Buyer;
            records[1].Levels[0].BidPrice = double.NaN;
            this.cache.SaveDayFile(CacheFixture.Opra, CacheFixture.Cbbo, Call, October16, records);

            await this.CompactAsync();

            ArchiveTestRecords.AssertSameValues<ICbboRecord>(records, await this.cache.ReadServedCbboAsync(Call, October16), Call);
        }

        [TestMethod]
        public async Task Compact_Rebuild_TheDayFileWinsTheOtherDaysAreKeptAndThePreviousVersionIsRetired()
        {
            CbboRecordDbn[] otherCallRecords = CacheFixture.CreateCbboRecords(2);
            string previous = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Call, 3), Day(OtherCall, otherCallRecords));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(2L, report.SymbolDaysWritten);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1, report.FullFilesWritten);
            Assert.AreEqual(0, report.DeltasFolded, "the version had no delta");
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(otherCallRecords, await this.cache.ReadServedCbboAsync(OtherCall, October16), OtherCall);
            Assert.IsFalse(File.Exists(previous), "the previous version is retired");
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_PreviousVersionNamedAfterNow_NewVersionIsNamedASecondAfterIt()
        {
            this.cache.WriteCbboFile(new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            await this.CompactAsync(CompactionMode.Full);

            CollectionAssert.AreEqual(new[] { "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261120T000001.full.parquet" }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_FileReadBackDiffersFromItsSource_FailsTheGroupAndLeavesTheFolderAsItWas()
        {
            string previous = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();
            DatabentoArchiveCompactor compactor = this.CreateCompactor();

            // the day file is changed once its records are written, so the file no longer matches it
            compactor.TemporaryFileWritten = _ => this.cache.SaveCbbo(Call, October16, 6601);
            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            ArchiveGroupFailure failure = report.FailedGroups.Single();
            Assert.AreEqual(GroupName, failure.Group);
            Assert.IsInstanceOfType(failure.Exception, typeof(InvalidDataException));
            StringAssert.Contains(failure.Exception.Message, "price");
            CollectionAssert.AreEqual(before, this.cache.CacheFiles(), "nothing is published, retired or left behind");
            Assert.IsTrue(File.Exists(previous));
        }

        [TestMethod]
        [DataRow(ArchiveVerification.EveryRecord)]
        [DataRow(ArchiveVerification.CountsOnly)]
        public async Task Compact_CorruptTemporaryFile_FailsTheGroupAndLeavesTheFolderAsItWas(ArchiveVerification verification)
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.Verification = verification);
            compactor.TemporaryFileWritten = path => File.WriteAllBytes(path, new byte[64]);

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.IsInstanceOfType(report.FailedGroups.Single().Exception, typeof(InvalidDataException));
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_DayFileChangedAfterTheRunFoundIt_IsKeptForTheNextRun()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            compactor.TemporaryFileWritten = _ => File.SetLastWriteTimeUtc(dayFile, File.GetLastWriteTimeUtc(dayFile).AddHours(1));

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesChangedDuringRun);
            Assert.AreEqual(0L, report.DayFilesDeleted);
            Assert.IsTrue(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_DayFileSavedAgainWhileItsDeleteIsRetried_IsKeptAndCounted()
        {
            // the first delete fails as on a file in use, and the day is downloaded again before the retry: deleting it would lose the new copy
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            int deletes = 0;
            compactor.DeleteFile = path =>
            {
                if(++deletes == 1)
                {
                    this.cache.SaveCbbo(Call, October16, 6601, 6602);
                    throw new IOException("The file is in use.");
                }

                File.Delete(path);
            };

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(1, deletes, "the retry finds the day file changed and does not delete it");
            Assert.AreEqual(1L, report.DayFilesChangedDuringRun);
            Assert.AreEqual(0L, report.DayFilesDeleted);
            Assert.AreEqual(0, report.FilesLeft.Count);
            CollectionAssert.AreEqual(new[] { 6601d, 6602d }, (await DayFile.ReadAsync(dayFile, JsonRecordReader.ReadCbboAsync, CancellationToken.None)).Select(record => record.Price).ToArray());
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price, "the archive holds the copy the run found");
        }

        [TestMethod]
        public async Task Compact_DayFileWhoseFirstDeleteFails_IsDeletedByTheRetry()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            int deletes = 0;
            compactor.DeleteFile = path =>
            {
                if(++deletes == 1)
                {
                    throw new UnauthorizedAccessException("Access is denied.");
                }

                File.Delete(path);
            };

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(2, deletes);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            Assert.AreEqual(0L, report.DayFilesChangedDuringRun);
            Assert.AreEqual(0, report.FilesLeft.Count);
            Assert.IsFalse(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_DayFileInUse_IsRetriedThenLeftAndTheNextRunDeletesIt()
        {
            if(!OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("Only Windows refuses to delete a file another handle holds open.");
            }

            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            ArchiveCompactionReport report;
            using(new FileStream(dayFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                report = await this.CompactAsync();
            }

            CollectionAssert.AreEqual(new[] { Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz") }, report.FilesLeft.ToArray(), "under the cache root");
            Assert.AreEqual(0L, report.DayFilesDeleted);
            Assert.IsTrue(File.Exists(dayFile));

            this.clock.Advance(TimeSpan.FromDays(1));
            ArchiveCompactionReport next = await this.CompactAsync();

            Assert.AreEqual(1L, next.DayFilesMatchingArchive);
            Assert.AreEqual(1L, next.DayFilesDeleted);
            Assert.AreEqual(0, next.GroupsCompacted, "the left day file holds what the full file serves, so no delta is written");
            Assert.IsFalse(File.Exists(dayFile));
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_UnreadableDayFile_IsReportedKeptOnDiskAndLeftOut()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string unreadable = this.cache.WriteUnreadableCbbo(Put, October16);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.SymbolDaysWritten);
            UnreadableDayFile dayFile = report.UnreadableDayFiles.Single();
            Assert.AreEqual(Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "SPXW%20%20261016P06600000_2026-10-16.UTC.json.gz"), dayFile.RelativePath);
            Assert.IsFalse(string.IsNullOrEmpty(dayFile.Error));
            Assert.AreEqual(0, report.Suspects.Count, "an unreadable day file is no suspect");
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsTrue(File.Exists(unreadable));
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16));
        }

        [TestMethod]
        [DataRow("an empty file")]
        [DataRow("an empty gzip payload")]
        [DataRow("whitespace only")]
        [DataRow("JSON lines")]
        public async Task Compact_DayFileHoldingNoJsonArray_IsReportedUnreadableKeptAndLeftOut(string holding)
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string dayFile = this.cache.WriteCbboBytes(Put, October16, CacheFixture.NoArrayDayFile(holding));

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "SPXW%20%20261016P06600000_2026-10-16.UTC.json.gz"), report.UnreadableDayFiles.Single().RelativePath);
            Assert.AreEqual(1L, report.SymbolDaysWritten);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            Assert.IsTrue(File.Exists(dayFile));
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16), "not archived, not even as an empty day");
        }

        [TestMethod]
        [DataRow("a local time", "only UTC times")]
        [DataRow("a record of another type", "Cbbo1S")]
        [DataRow("a record of another symbol", Put)]
        public async Task Compact_DayFileOfRecordsTheArchiveCannotHold_IsReportedUnreadableAndKept(string problem, string expectedError)
        {
            string dayFile = this.cache.SaveChangedCbbo(Call, October16, record =>
            {
                switch(problem)
                {
                    case "a local time":
                        record.TsReceivedUtc = DateTime.SpecifyKind(record.TsReceivedUtc, DateTimeKind.Local);
                        break;
                    case "a record of another type":
                        record.Header.RecordType = RType.Cbbo1S;
                        break;
                    default:
                        record.Symbol = Put;
                        break;
                }
            });

            ArchiveCompactionReport report = await this.CompactAsync();

            StringAssert.Contains(report.UnreadableDayFiles.Single().Error, expectedError);
            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsTrue(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_RebuildWithAnUnreadableDayFileOfAnArchivedDay_KeepsTheArchivedCopy()
        {
            CbboRecordDbn[] putRecords = CacheFixture.CreateCbboRecords(2);
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, Day(Put, putRecords));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string unreadable = this.cache.WriteUnreadableCbbo(Put, October16);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(2L, report.SymbolDaysWritten);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1L, report.DayFilesDeleted, "only the readable day file was absorbed");
            Assert.IsTrue(File.Exists(unreadable));
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(putRecords, await this.cache.ReadServedCbboAsync(Put, October16), Put);
        }

        [TestMethod]
        public async Task Compact_OnlyUnreadableDayFiles_PublishesNothing()
        {
            string previous = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Put, 1));
            this.cache.WriteUnreadableCbbo(Put, October16);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.UnreadableDayFiles.Count);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
            Assert.IsTrue(File.Exists(previous));
        }

        [TestMethod]
        public async Task Compact_DryRun_WritesOnlyIntoTheDryRunFolder()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();
            byte[][] beforeBytes = before.Select(File.ReadAllBytes).ToArray();

            ArchiveCompactionReport report = await this.CompactAsync(options => options.DryRun = true);

            Assert.IsTrue(report.DryRun);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.SymbolDaysWritten, "the day file goes into the next delta");
            Assert.AreEqual(0L, report.DayFilesDeleted);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles(), "the cache is untouched");
            for(int i = 0; i < before.Length; ++i)
            {
                CollectionAssert.AreEqual(beforeBytes[i], File.ReadAllBytes(before[i]));
            }

            string dryRunDirectory = Path.Combine(this.cache.DryRunFolder, "OPRA.PILLAR", "cbbo-1m", "2026", "10");
            CollectionAssert.AreEqual(new[] { PreviousDelta(1) }, this.cache.ArchiveFileNames(dryRunDirectory));
            CollectionAssert.AreEquivalent(new[] { "Cache", "DryRun" }, Directory.GetDirectories(this.cache.Folder).Select(Path.GetFileName).ToArray(), "no other folder is made");
            await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(Path.Combine(dryRunDirectory, PreviousDelta(1)), new CbboArchiveSchema(RType.Cbbo1M)))
            {
                Assert.AreEqual(6600.5, (await reader.ReadSymbolDayAsync(Call, October16)).Single().Price);
            }
        }

        [TestMethod]
        public async Task Compact_FutureAndIndexTradeDayFiles_AreArchivedUnderTheirGroups()
        {
            BboRecordJson bbo = new BboRecordJson
            {
                Header = new RecordHeaderJson
                {
                    RecordType = RType.Bbo1M,
                    TsEventUtc = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc),
                },
                Side = TradeAggressor.None,
                TsReceivedUtc = new DateTime(2026, 10, 16, 13, 30, 1, DateTimeKind.Utc),
                Levels = new[] { new Mbp1LevelJson { BidPrice = 21000.25, AskPrice = 21000.5 } },
            };
            TradeRecordJson trade = new TradeRecordJson
            {
                Header = new RecordHeaderJson
                {
                    RecordType = RType.Mbp0,
                    TsEventUtc = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc),
                },
                Price = 6600.5,
                Side = TradeAggressor.None,
                TsReceivedUtc = new DateTime(2026, 10, 16, 13, 30, 1, DateTimeKind.Utc),
            };
            this.cache.SaveDayFile("GLBX.MDP3", "bbo-1m", "MNQZ6", October16, bbo);
            this.cache.SaveDayFile("DBIX.CORE", "trades", "SPX", October16, trade);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(2, report.GroupsCompacted);
            CollectionAssert.AreEqual(new[] { "GLBX.MDP3_bbo-1m_MNQ.FUT_2026-10.UTC_20261119T021530.full.parquet" }, this.cache.ArchiveFileNames(this.cache.MonthDirectory("GLBX.MDP3", "bbo-1m", 2026, 10)));
            CollectionAssert.AreEqual(new[] { "DBIX.CORE_trades_SPX_2026-10.UTC_20261119T021530.full.parquet" }, this.cache.ArchiveFileNames(this.cache.MonthDirectory("DBIX.CORE", "trades", 2026, 10)));
            Assert.AreEqual(21000.25, (await this.cache.ReadServedAsync(new BboArchiveSchema(RType.Bbo1M), "GLBX.MDP3", "bbo-1m", "MNQZ6", October16)).Single().Level.BidPrice);
            Assert.AreEqual(6600.5, (await this.cache.ReadServedAsync(new TradeArchiveSchema(RType.Mbp0), "DBIX.CORE", "trades", "SPX", October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_EveryOptionAndStockSchema_IsServedAfterwards()
        {
            DateTime received = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc);
            this.cache.SaveDayFile(CacheFixture.Opra, CacheFixture.Cbbo, SpyCall, October16, CacheFixture.CbboRecord(October16, 1.25, 0));
            this.cache.SaveDayFile(CacheFixture.Opra, "tcbbo", Call, October16, new TcbboRecordJson { Header = Header(RType.Tcbbo), Price = 12.5, Side = TradeAggressor.Buyer, TsReceivedUtc = received });
            this.cache.SaveDayFile(CacheFixture.Opra, "statistics", Call, October16, new StatisticsRecordJson { Header = Header(RType.Statistics), Price = double.NaN, Quantity = 321, TsReceivedUtc = received });
            this.cache.SaveDayFile("XNAS.ITCH", "ohlcv-1d", "QQQ", October16, new OhlcvRecordJson { Header = Header(RType.Ohlcv1D), Close = 600.5 });

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(4, report.GroupsCompacted);
            Assert.AreEqual(4L, report.DayFilesDeleted);
            Assert.AreEqual(1.25, (await this.cache.ReadServedCbboAsync(SpyCall, October16)).Single().Price);
            Assert.AreEqual(12.5, (await this.cache.ReadServedAsync(new TcbboArchiveSchema(RType.Tcbbo), CacheFixture.Opra, "tcbbo", Call, October16)).Single().Price);
            Assert.AreEqual(321L, (await this.cache.ReadServedAsync(new StatisticsArchiveSchema(RType.Statistics), CacheFixture.Opra, "statistics", Call, October16)).Single().Quantity);
            Assert.AreEqual(600.5, (await this.cache.ReadServedAsync(new OhlcvArchiveSchema(RType.Ohlcv1D), "XNAS.ITCH", "ohlcv-1d", "QQQ", October16)).Single().Close);
        }

        [TestMethod]
        public async Task Compact_FoldersAndFilesTheArchiveDoesNotTake_AreSkippedAndLeft()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] left =
            {
                this.cache.SaveDayFile("not_a_dataset", CacheFixture.Cbbo, Call, October16, CacheFixture.CbboRecord(October16, 1, 0)),
                this.cache.SaveDayFile(CacheFixture.Opra, "mbo", Call, October16, CacheFixture.CbboRecord(October16, 1, 0)),
                CopyTo(dayFile, this.cache.CbboDirectory, "SPXW%20%20261016C06600000_2026-10-16.ET.json.gz"),
                CopyTo(dayFile, this.cache.CbboDirectory, "SPXW%20%20261016C06600000_2026-11-02.UTC.json.gz"),
                CopyTo(dayFile, this.cache.CbboDirectory, "notes.txt"),
                CopyTo(dayFile, Path.Combine(this.cache.Root, "OPRA.PILLAR", "cbbo-1m", "26", "10"), Path.GetFileName(dayFile)),
                CopyTo(dayFile, Path.Combine(this.cache.Root, "OPRA.PILLAR", "cbbo-1m", "2026", "13"), Path.GetFileName(dayFile)),
            };

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            CollectionAssert.AreEqual(new[] { Path.Combine("OPRA.PILLAR", "mbo"), "not_a_dataset" }, report.FoldersSkipped.ToArray(), "a folder of no dataset, and one of a schema the archive has no columns for");
            Assert.IsFalse(File.Exists(dayFile));
            foreach(string path in left)
            {
                Assert.IsTrue(File.Exists(path), path);
            }
        }

        [TestMethod]
        public async Task Compact_DatasetFolderInLowerCase_IsSkippedAndLeft()
        {
            // Databento names its datasets in capitals; archived from opra.pillar, an option would not even be filed under its parent
            string dayFile = this.cache.SaveDayFile("opra.pillar", CacheFixture.Cbbo, Call, October16, CacheFixture.CbboRecord(October16, 6600.5, 0));

            ArchiveCompactionReport report = await this.CompactAsync();

            CollectionAssert.AreEqual(new[] { "opra.pillar" }, report.FoldersSkipped.ToArray());
            Assert.AreEqual(0, report.GroupsCompacted);
            CollectionAssert.AreEqual(new[] { dayFile }, Directory.GetFiles(Path.GetDirectoryName(dayFile)));
        }

        [TestMethod]
        [DataRow(false, DisplayName = "the group-month's first full file")]
        [DataRow(true, DisplayName = "a delta over a full file serving the day")]
        public async Task Compact_DayFileOfAValueItsRecordInterfaceCannotGive_IsReportedUnreadableAndKept(bool archived)
        {
            // TcbboRecordJson holds ts_in_delta as a long and gives it as the interface's int by a checked cast
            TcbboArchiveSchema schema = new TcbboArchiveSchema(RType.Tcbbo);
            if(archived)
            {
                // every column before ts_in_delta the same as the day file's, so comparing the two reaches it
                ArchiveTestRecords.WriteFile(schema, this.cache.MonthDirectory(CacheFixture.Opra, "tcbbo", 2026, 10), new ArchiveFileIdentity(CacheFixture.Opra, "tcbbo", "SPXW.OPT", 2026, 10, "UTC", PreviousBuiltUtc), new ArchiveSymbolDay<ITcbboRecord>(Call, October16, new ITcbboRecord[] { Tcbbo(12.5, 5150) }));
            }

            string overflowing = this.cache.SaveDayFile(CacheFixture.Opra, "tcbbo", Call, October16, Tcbbo(12.5, (long)int.MaxValue + 1));
            this.cache.SaveDayFile(CacheFixture.Opra, "tcbbo", Put, October16, Tcbbo(1.5, 950));

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            StringAssert.Contains(report.UnreadableDayFiles.Single().Error, "ts_in_delta");
            Assert.IsTrue(File.Exists(overflowing));
            Assert.AreEqual(950, (await this.cache.ReadServedAsync(schema, CacheFixture.Opra, "tcbbo", Put, October16)).Single().TsInDelta);
            TcbboRecordDbn[] call = await this.cache.ReadServedAsync(schema, CacheFixture.Opra, "tcbbo", Call, October16);
            if(archived)
            {
                Assert.AreEqual(5150, call.Single().TsInDelta, "the full file's copy is still served");
            }
            else
            {
                Assert.IsNull(call);
            }
        }

        [TestMethod]
        public async Task Compact_FilesNamedLikeTemporaryFilesOfNoArchiveFile_AreLeftAndTheFolderIsCompacted()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] left =
            {
                CopyTo(dayFile, this.cache.CbboDirectory, ".tmp"),
                CopyTo(dayFile, this.cache.CbboDirectory, "x.parquet..tmp"),
            };

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.IsFalse(File.Exists(dayFile));
            foreach(string path in left)
            {
                Assert.IsTrue(File.Exists(path), path);
            }
        }

        [TestMethod]
        public async Task Compact_NothingNew_DoesNothing()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync();
            string[] after = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            CollectionAssert.AreEqual(after, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_DayDifferingInATimeOrAValueColumn_FailsVerification()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            compactor.TemporaryFileWritten = _ => this.cache.SaveChangedCbbo(Call, October16, record => record.Header.TsEventUtc = record.Header.TsEventUtc.Value.AddSeconds(1));

            ArchiveCompactionReport report = await compactor.CompactAsync();

            StringAssert.Contains(report.FailedGroups.Single().Exception.Message, "ts_event");

            this.cache.SaveCbbo(Call, October16, 6600.5);
            compactor = this.CreateCompactor();
            compactor.TemporaryFileWritten = _ => this.cache.SaveChangedCbbo(Call, October16, record => record.Size += 1);

            report = await compactor.CompactAsync();

            StringAssert.Contains(report.FailedGroups.Single().Exception.Message, "size");
            Assert.AreEqual(0, this.cache.CbboArchiveFileNames().Length);
        }

        [TestMethod]
        public async Task Compact_CurrentVersionUnreadable_FailsTheGroupAndLeavesItWithItsDayFiles()
        {
            Directory.CreateDirectory(this.cache.CbboDirectory);
            File.WriteAllBytes(Path.Combine(this.cache.CbboDirectory, PreviousFullFile), new byte[] { 1, 2, 3 });
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            ArchiveGroupFailure failure = report.FailedGroups.Single();
            Assert.AreEqual(GroupName, failure.Group);
            Assert.IsInstanceOfType(failure.Exception, typeof(InvalidDataException));
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_UnreadableDeltaOfTheCurrentVersion_FailsTheGroupAndLeavesItWithItsDayFiles()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            File.WriteAllBytes(Path.Combine(this.cache.CbboDirectory, PreviousDelta(1)), new byte[] { 1, 2, 3 });
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.FailedGroups.Count);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_LeftTemporaryFile_IsDeleted()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string left = Path.Combine(this.cache.CbboDirectory, PreviousFullFile + ".0123456789abcdef0123456789abcdef.tmp");
            File.WriteAllBytes(left, new byte[] { 1 });

            await this.CompactAsync();

            Assert.IsFalse(File.Exists(left));
        }

        [TestMethod]
        public async Task Compact_DefaultOptions_CompactsAndMakesNoFolderBesideTheCache()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await new DatabentoArchiveCompactor(new ArchiveCompactorOptions { CacheRoot = this.cache.Root }).CompactAsync();

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1, report.FullFilesWritten);
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            CollectionAssert.AreEqual(new[] { "Cache" }, Directory.GetDirectories(this.cache.Folder).Select(Path.GetFileName).ToArray(), "no backup or staging folder is made");
        }

        [TestMethod]
        public async Task Compact_ParallelGroups_CompactsEveryGroupAndTheArchiveServesThemAll()
        {
            string[] symbols = { Call, "NDXP  261016C25000000", SpyCall, "QQQ   261016C00500000" };
            DateTime[] dates = { new DateTime(2026, 8, 14), new DateTime(2026, 9, 16), October16 };
            for(int symbol = 0; symbol < symbols.Length; ++symbol)
            {
                for(int date = 0; date < dates.Length; ++date)
                {
                    this.cache.SaveCbbo(symbols[symbol], dates[date], (symbol * 10) + date);
                }
            }

            ArchiveCompactionReport report = await this.CompactAsync(options => options.MaxParallelGroups = 4);

            Assert.AreEqual(12, report.GroupsCompacted);
            Assert.AreEqual(12L, report.DayFilesAbsorbed);
            Assert.AreEqual(12L, report.DayFilesDeleted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            for(int symbol = 0; symbol < symbols.Length; ++symbol)
            {
                for(int date = 0; date < dates.Length; ++date)
                {
                    Assert.AreEqual((double)((symbol * 10) + date), (await this.cache.ReadServedCbboAsync(symbols[symbol], dates[date])).Single().Price);
                }
            }
        }

        [TestMethod]
        public async Task Compact_MaxParallelGroups_CompactsThatManyGroupsAtOnce()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveCbbo(SpyCall, October16, 1.25);
            DatabentoArchiveCompactor compactor = this.CreateCompactor(options => options.MaxParallelGroups = 2);
            int groupsThatSawTheOther = 0;
            using(CountdownEvent bothWritten = new CountdownEvent(2))
            {
                compactor.TemporaryFileWritten = path =>
                {
                    // run one at a time, the first group would wait here for a second that only starts once it is done
                    bothWritten.Signal();
                    if(bothWritten.Wait(TimeSpan.FromSeconds(10)))
                    {
                        Interlocked.Increment(ref groupsThatSawTheOther);
                    }
                };

                ArchiveCompactionReport report = await compactor.CompactAsync();

                Assert.AreEqual(2, report.GroupsCompacted);
            }

            Assert.AreEqual(2, groupsThatSawTheOther, "both groups were written at the same time");
        }

        [TestMethod]
        public async Task Compact_Canceled_StopsAndLeavesEveryDayToItsDayFile()
        {
            string spxw = this.cache.SaveCbbo(Call, October16, 6600.5);
            string spy = this.cache.SaveCbbo(SpyCall, October16, 1.25);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            ArchiveCompactionReport report;
            using(CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                compactor.TemporaryFileWritten = path => cancellation.Cancel();
                report = await compactor.CompactAsync(cancellation.Token);
            }

            Assert.IsTrue(report.Canceled, "a cancel ends the run rather than failing a group");
            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsTrue(File.Exists(spxw), "the canceled group published nothing and deleted nothing");
            Assert.IsTrue(File.Exists(spy), "the group after the cancel was not started");
            Assert.AreEqual(0, Directory.GetFiles(this.cache.Root, "*.tmp", SearchOption.AllDirectories).Length);
            Assert.AreEqual(0, Directory.GetFiles(this.cache.Root, "*" + ArchiveFileName.Extension, SearchOption.AllDirectories).Length);
        }

        [TestMethod]
        public async Task Compact_CanceledAfterTheFirstGroup_ReturnsWhatTheRunDid()
        {
            string spxw = this.cache.SaveCbbo(Call, October16, 6600.5);
            string spy = this.cache.SaveCbbo(SpyCall, October16, 1.25);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            ArchiveCompactionReport report;
            using(CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                // SPXW.OPT comes before SPY.OPT; the cancel comes as the second group's file is written
                int written = 0;
                compactor.TemporaryFileWritten = path =>
                {
                    if(++written == 2)
                    {
                        cancellation.Cancel();
                    }
                };
                report = await compactor.CompactAsync(cancellation.Token);
            }

            Assert.IsTrue(report.Canceled);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1, report.FullFilesWritten);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsFalse(File.Exists(spxw));
            Assert.IsTrue(File.Exists(spy));
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames(), "the first group's file alone");
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_Group_WorksFromTheFilesItsMonthFolderWasListedWith()
        {
            // the month folder is listed once, before its first group starts; the lock keeps any other writer out, so nothing changes
            // after it but here, where a full file of the second group appears while the first is compacted
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveCbbo(SpyCall, October16, 1.25);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            string appeared = null;
            compactor.TemporaryFileWritten = path =>
            {
                appeared ??= ArchiveTestRecords.WriteFile(new CbboArchiveSchema(RType.Cbbo1M), this.cache.CbboDirectory, new ArchiveFileIdentity(CacheFixture.Opra, CacheFixture.Cbbo, "SPY.OPT", 2026, 10, "UTC", PreviousBuiltUtc), new ArchiveSymbolDay<ICbboRecord>(SpyCall, October16, CacheFixture.CreateCbboRecords(2)));
            };

            ArchiveCompactionReport report = await compactor.CompactAsync();

            Assert.AreEqual(2, report.FullFilesWritten, "the second group found no full file to write a delta of");
            Assert.AreEqual(0, report.DeltasWritten);
            Assert.IsTrue(File.Exists(appeared), "a file the listing did not find is left to the next run");
            Assert.AreEqual(1.25, (await this.cache.ReadServedCbboAsync(SpyCall, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_FullFileExists_WritesOnlyTheDayFilesAsItsNextDelta()
        {
            CbboRecordDbn[] otherCallRecords = CacheFixture.CreateCbboRecords(2);
            string full = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Call, 3), Day(OtherCall, otherCallRecords));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveCbbo(Put, October16, 12.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(2L, report.SymbolDaysWritten, "only the day files, a day the full file holds included");
            Assert.AreEqual(1, report.DeltasWritten);
            Assert.IsTrue(File.Exists(full), "the full file is kept");
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);

            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price, "the delta wins the full file's day");
            Assert.AreEqual(12.5, (await this.cache.ReadServedCbboAsync(Put, October16)).Single().Price);
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(otherCallRecords, await this.cache.ReadServedCbboAsync(OtherCall, October16), OtherCall);
        }

        [TestMethod]
        public async Task Compact_DeltaModeWithADelta_WritesTheNextGenerationOfThePreviousDeltaAndTheDayFilesAndRetiresThePrevious()
        {
            this.cache.SaveCbbo(Call, October16, 1);
            await this.CompactAsync();
            this.clock.SetUtcNow(NowUtc.AddDays(1));
            this.cache.SaveCbbo(Call, October16, 2);
            this.cache.SaveCbbo(Put, October16, 20);
            await this.CompactAsync();
            CollectionAssert.AreEqual(new[] { NewDelta(1), NewFullFile }, this.cache.CbboArchiveFileNames());

            this.clock.SetUtcNow(NowUtc.AddDays(2));
            this.cache.SaveCbbo(OtherCall, October16, 300);
            this.cache.SaveCbbo(Put, October16, 21);
            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.DeltasWritten);
            Assert.AreEqual(0, report.FullFilesWritten);
            Assert.AreEqual(0, report.DeltasFolded, "the next generation carries the previous one forward, it folds it into no full file");
            Assert.AreEqual(2L, report.DayFilesAbsorbed);
            Assert.AreEqual(3L, report.SymbolDaysWritten, "the previous delta's days and the day files, each once");
            CollectionAssert.AreEqual(new[] { NewDelta(2), NewFullFile }, this.cache.CbboArchiveFileNames(), "the full file is kept, the previous generation retired");
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price, "the previous delta's copy is carried over");
            Assert.AreEqual(21, (await this.cache.ReadServedCbboAsync(Put, October16)).Single().Price, "a day file wins over the previous delta's copy");
            Assert.AreEqual(300, (await this.cache.ReadServedCbboAsync(OtherCall, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_DeltaMode_NeverRebuildsHoweverHighTheGeneration()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string delta = this.cache.WriteCbboFile(PreviousBuiltUtc, 9, CacheFixture.CbboDay(Put, 2));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            await this.CompactAsync(CompactionMode.Delta, maxDeltaGeneration: 2);

            CollectionAssert.AreEqual(new[] { PreviousDelta(10), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsFalse(File.Exists(delta));
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October16)).Length);
        }

        [TestMethod]
        public async Task Compact_NoFullFile_WritesAFullFileOfTheDayFilesInEveryMode()
        {
            foreach(CompactionMode mode in Enum.GetValues<CompactionMode>())
            {
                if(Directory.Exists(this.cache.CbboDirectory))
                {
                    Directory.Delete(this.cache.CbboDirectory, recursive: true);
                }

                this.cache.SaveCbbo(Call, October16, 6600.5);

                ArchiveCompactionReport report = await this.CompactAsync(mode);

                Assert.AreEqual(1, report.FullFilesWritten, mode.ToString());
                CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames(), mode.ToString());
            }
        }

        [TestMethod]
        public async Task Compact_FullMode_RebuildsFromTheFullFileTheDeltaAndTheDayFilesAndRetiresBoth()
        {
            CbboRecordDbn[] deltaCallRecords = CacheFixture.CreateCbboRecords(2);
            CbboRecordDbn[] putRecords = CacheFixture.CreateCbboRecords(4);
            string full = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Call, 3), CacheFixture.CbboDay(OtherCall, 1));
            string delta = this.cache.WriteCbboFile(PreviousBuiltUtc, 2, Day(Call, deltaCallRecords), Day(Put, putRecords));
            string dayFile = this.cache.SaveCbbo(OtherCall, October16, 300);
            long bytesBefore = new FileInfo(full).Length + new FileInfo(delta).Length + new FileInfo(dayFile).Length;

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(1, report.FullFilesWritten);
            Assert.AreEqual(0, report.DeltasWritten);
            Assert.AreEqual(1, report.DeltasFolded);
            Assert.AreEqual(3L, report.SymbolDaysWritten, "every day of the version, each once");
            Assert.AreEqual(bytesBefore, report.BytesBefore, "a rebuild replaces the whole version and takes in the day file");
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsFalse(File.Exists(full));
            Assert.IsFalse(File.Exists(delta));
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(deltaCallRecords, await this.cache.ReadServedCbboAsync(Call, October16), Call);
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(putRecords, await this.cache.ReadServedCbboAsync(Put, October16), Put);
            Assert.AreEqual(300, (await this.cache.ReadServedCbboAsync(OtherCall, October16)).Single().Price, "the day file wins");
        }

        [TestMethod]
        public async Task Compact_FullMode_RebuildsOnEveryRun()
        {
            this.cache.SaveCbbo(Call, October16, 1);
            await this.CompactAsync(CompactionMode.Full);
            this.clock.SetUtcNow(NowUtc.AddDays(1));
            this.cache.SaveCbbo(Put, October16, 20);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(2L, report.SymbolDaysWritten);
            Assert.AreEqual(0, report.DeltasFolded, "a rebuild of a full file alone folds no delta");
            CollectionAssert.AreEqual(new[] { "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261120T021530.full.parquet" }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_FullModeGroupWithADeltaAndNoDayFiles_IsRebuilt()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 2));

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(0L, report.DayFilesAbsorbed);
            Assert.AreEqual(1, report.DeltasFolded, "the delta is what the rebuild took in");
            Assert.AreEqual(2L, report.SymbolDaysWritten);
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October16)).Length);
        }

        [TestMethod]
        [DataRow(CompactionMode.Full, DisplayName = "full mode, a full file alone")]
        [DataRow(CompactionMode.Delta, DisplayName = "delta mode, a full file and its delta")]
        [DataRow(CompactionMode.Auto, DisplayName = "auto mode, a full file and its delta")]
        public async Task Compact_GroupWithNothingToFold_IsLeftAsItIs(CompactionMode mode)
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            if(mode != CompactionMode.Full)
            {
                this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 2));
            }

            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync(mode);

            Assert.AreEqual(0, report.GroupsCompacted);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        [DataRow(7, false, DisplayName = "the next generation stays under the maximum")]
        [DataRow(8, true, DisplayName = "the next generation reaches the maximum")]
        public async Task Compact_AutoMode_RebuildsWhenTheNextGenerationReachesTheMaximum(int generation, bool rebuilds)
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, Enumerable.Range(0, 20).Select(strike => CacheFixture.CbboDay(Strike(6600 + strike), 50)).ToArray());
            this.cache.WriteCbboFile(PreviousBuiltUtc, generation, CacheFixture.CbboDay(Put, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto, maxDeltaGeneration: 9, maxDeltaSizePercent: 100);

            Assert.AreEqual(rebuilds ? 1 : 0, report.FullFilesWritten);
            Assert.AreEqual(rebuilds ? 0 : 1, report.DeltasWritten);
            Assert.AreEqual(rebuilds ? 1 : 0, report.DeltasFolded);
            CollectionAssert.AreEqual(
                rebuilds ? new[] { NewFullFile } : new[] { PreviousDelta(generation + 1), PreviousFullFile },
                this.cache.CbboArchiveFileNames(),
                "a rebuild retires the full file and its delta, a delta only the previous generation");
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "a delta smaller than its share of the full file")]
        [DataRow(true, DisplayName = "a delta larger than its share of the full file")]
        public async Task Compact_AutoMode_RebuildsWhenTheDeltaOutgrowsItsShareOfTheFullFile(bool largeDelta)
        {
            ArchiveSymbolDay<ICbboRecord>[] manyDays = Enumerable.Range(0, 20).Select(strike => CacheFixture.CbboDay(Strike(6600 + strike), 50)).ToArray();
            ArchiveSymbolDay<ICbboRecord>[] oneDay = { CacheFixture.CbboDay(Put, 1) };
            string full = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, largeDelta ? oneDay : manyDays);
            string delta = this.cache.WriteCbboFile(PreviousBuiltUtc, 1, largeDelta ? manyDays : oneDay);
            Assert.AreEqual(largeDelta, new FileInfo(delta).Length > new FileInfo(full).Length, "the delta's size against its full file is what the test is about");
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto, maxDeltaGeneration: 9, maxDeltaSizePercent: 100);

            Assert.AreEqual(largeDelta ? 1 : 0, report.FullFilesWritten);
            Assert.AreEqual(largeDelta ? 1 : 0, report.DeltasFolded);
            CollectionAssert.AreEqual(largeDelta ? new[] { NewFullFile } : new[] { PreviousDelta(2), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.IsFalse(File.Exists(delta), "the previous delta is retired either way");
            foreach(string symbol in Enumerable.Range(0, 20).Select(strike => Strike(6600 + strike)).Append(Put))
            {
                Assert.IsNotNull(await this.cache.ReadServedCbboAsync(symbol, October16), Invariant($"{symbol} is still served"));
            }

            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_AutoModeWithoutADelta_WritesTheFirstGeneration()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Put, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto, maxDeltaGeneration: 9, maxDeltaSizePercent: 0);

            Assert.AreEqual(1, report.DeltasWritten);
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_AutoModeAllowingNoDelta_RebuildsEveryTime()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto, maxDeltaGeneration: 1);

            Assert.AreEqual(1, report.FullFilesWritten);
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_OlderGenerationLeftByAKilledRun_IsRetiredByTheNextRunEvenWithoutDayFiles()
        {
            // a run killed between publishing a generation and retiring the previous one leaves both
            string full = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 1));
            string current = this.cache.WriteCbboFile(PreviousBuiltUtc, 2, CacheFixture.CbboDay(Put, 2));
            string temporary = Path.Combine(this.cache.CbboDirectory, PreviousDelta(3) + ".0123456789abcdef0123456789abcdef.tmp");
            File.WriteAllBytes(temporary, new byte[] { 1 });

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.IsFalse(File.Exists(older), "the current generation lists the older one's day");
            Assert.AreEqual(0, report.OrphanFiles.Count);
            Assert.IsFalse(File.Exists(temporary));
            Assert.IsTrue(File.Exists(full));
            Assert.IsTrue(File.Exists(current));
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October16)).Length);
        }

        [TestMethod]
        public async Task Compact_VersionLeftByAKilledRebuild_IsRetiredByTheNextRunEvenWithoutDayFiles()
        {
            // a rebuild killed after publishing leaves the previous full file and its delta beside the new full file
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(-1), 0, CacheFixture.CbboDay(Put, 1));
            string olderDelta = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(-1), 1, CacheFixture.CbboDay(Put, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Put, 2));

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto);

            Assert.IsFalse(File.Exists(older), "the new full file lists the older version's day");
            Assert.IsFalse(File.Exists(olderDelta));
            Assert.AreEqual(0, report.OrphanFiles.Count);
            CollectionAssert.AreEqual(new[] { PreviousFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_AutoModeGroupPastAThresholdWithOnlyALeftover_IsCleanedNotRebuilt()
        {
            // a leftover brings the group into the run, but with no day file there is nothing for a rebuild to add
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc, 7, CacheFixture.CbboDay(Put, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 8, CacheFixture.CbboDay(Put, 2));

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Auto, maxDeltaGeneration: 9);

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.IsFalse(File.Exists(older));
            CollectionAssert.AreEqual(new[] { PreviousDelta(8), PreviousFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_TemporaryFileAloneLeftByAKilledRun_IsDeletedEvenWithoutDayFiles()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string temporary = Path.Combine(this.cache.CbboDirectory, ArchiveFileName.GetTemporaryFileName(PreviousDelta(1)));
            File.WriteAllBytes(temporary, new byte[] { 1 });

            await this.CompactAsync();

            Assert.IsFalse(File.Exists(temporary));
            CollectionAssert.AreEqual(new[] { PreviousFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        [DataRow(CompactionMode.Full, 9, DisplayName = "full mode")]
        [DataRow(CompactionMode.Delta, 9, DisplayName = "delta mode")]
        [DataRow(CompactionMode.Auto, 9, DisplayName = "auto mode")]
        [DataRow(CompactionMode.Auto, 1, DisplayName = "auto mode allowing no delta")]
        public async Task Compact_GroupWithNoDeltaAndNoDayFilesButLeftovers_IsCleanedNotRebuilt(CompactionMode mode, int maxDeltaGeneration)
        {
            // the full file alone already holds every day, so a rebuild would only write it again
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(-1), 0, CacheFixture.CbboDay(OtherCall, 1));
            string full = this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 2));
            string temporary = Path.Combine(this.cache.CbboDirectory, ArchiveFileName.GetTemporaryFileName(PreviousFullFile));
            File.WriteAllBytes(temporary, new byte[] { 1 });
            byte[] fullBytes = File.ReadAllBytes(full);

            ArchiveCompactionReport report = await this.CompactAsync(mode, maxDeltaGeneration);

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FullFilesWritten);
            Assert.AreEqual(0, report.DeltasWritten);
            Assert.IsFalse(File.Exists(older));
            Assert.IsFalse(File.Exists(temporary));
            CollectionAssert.AreEqual(new[] { PreviousFullFile }, this.cache.CbboArchiveFileNames());
            CollectionAssert.AreEqual(fullBytes, File.ReadAllBytes(full));
        }

        [TestMethod]
        public async Task Compact_DeltaModeWithOnlyUnreadableDayFiles_KeepsTheCurrentGeneration()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 2, CacheFixture.CbboDay(Put, 2));
            string unreadable = this.cache.WriteUnreadableCbbo(Put, October16);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.IsTrue(File.Exists(unreadable));
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Put, October16)).Length);
        }

        [TestMethod]
        public async Task Compact_FullModeDryRunOfAGroupWithADeltaAndNoDayFiles_WritesOnlyIntoTheDryRunFolder()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 2));
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync(options =>
            {
                options.DryRun = true;
                options.CompactionMode = CompactionMode.Full;
            });

            Assert.AreEqual(1, report.FullFilesWritten);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.ArchiveFileNames(Path.Combine(this.cache.DryRunFolder, "OPRA.PILLAR", "cbbo-1m", "2026", "10")));
        }

        [TestMethod]
        public async Task Compact_DryRunWithALeftOlderGeneration_DeletesNothing()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Put, 1));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 2, CacheFixture.CbboDay(Put, 2));
            string[] before = this.cache.CacheFiles();

            await this.CompactAsync(options => options.DryRun = true);

            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_DayFileHoldingWhatTheFullFileServes_IsDeletedWithoutWritingADelta()
        {
            // a download again that brought back the same data adds nothing to the version
            this.cache.SaveCbbo(Call, October16, 6600.25, 6600.5);
            await this.CompactAsync();
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.25, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.DayFilesMatchingArchive);
            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(1L, report.DayFilesDeleted);
            Assert.IsFalse(File.Exists(dayFile));
            CollectionAssert.AreEqual(new[] { NewFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_DayFileHoldingWhatTheDeltaServes_IsDeletedWithoutWritingTheNextGeneration()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync();
            string[] archiveFiles = this.cache.CbboArchiveFileNames();
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.DayFilesMatchingArchive);
            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.IsFalse(File.Exists(dayFile));
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile }, archiveFiles);
            CollectionAssert.AreEqual(archiveFiles, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_DayFileHoldingWhatTheDeltaServesBesideANewOne_KeepsTheDeltasDayInTheNextGeneration()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync();
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveCbbo(Put, October16, 12.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.DayFilesMatchingArchive);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(2L, report.SymbolDaysWritten, "the delta's copy of the matching day, and the new day file");
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            CollectionAssert.AreEqual(new[] { PreviousDelta(2), PreviousFullFile }, this.cache.CbboArchiveFileNames());
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
            Assert.AreEqual(12.5, (await this.cache.ReadServedCbboAsync(Put, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_SomeDayFilesHoldingWhatTheVersionServes_OnlyTheOthersGoIntoTheDelta()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync();
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveCbbo(Put, October16, 12.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.SymbolDaysWritten, "only the put");
            Assert.AreEqual(1L, report.DayFilesMatchingArchive);
            Assert.AreEqual(2L, report.DayFilesDeleted);
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
            Assert.AreEqual(12.5, (await this.cache.ReadServedCbboAsync(Put, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_DayFileMatchingOnlyAnOlderCopyOfTheDay_GoesIntoTheNextDelta()
        {
            // the full file's copy is no longer served once a delta overrode it, so going back to it is a change
            this.cache.SaveCbbo(Call, October16, 1);
            await this.CompactAsync();
            this.cache.SaveCbbo(Call, October16, 2);
            await this.CompactAsync();
            this.cache.SaveCbbo(Call, October16, 1);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0L, report.DayFilesMatchingArchive);
            Assert.AreEqual(1L, report.SymbolDaysWritten);
            Assert.AreEqual(1, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_RebuildWithADayFileHoldingWhatTheVersionServes_KeepsTheDay()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync(CompactionMode.Full);
            this.clock.SetUtcNow(NowUtc.AddDays(1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(1L, report.SymbolDaysWritten);
            Assert.AreEqual(0L, report.DayFilesMatchingArchive, "a rebuild takes every day file");
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_DryRunWithADayFileHoldingWhatTheVersionServes_DeletesNothing()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CompactAsync();
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(options => options.DryRun = true);

            Assert.AreEqual(1L, report.DayFilesMatchingArchive);
            Assert.AreEqual(0L, report.DayFilesDeleted);
            Assert.IsTrue(File.Exists(dayFile));
        }

        [TestMethod]
        public async Task Compact_DeltaWithAnUnreadableDayFileOfAnArchivedDay_LeavesTheDayToTheFullFile()
        {
            CbboRecordDbn[] putRecords = CacheFixture.CreateCbboRecords(2);
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, Day(Put, putRecords));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.WriteUnreadableCbbo(Put, October16);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1L, report.SymbolDaysWritten);
            Assert.AreEqual(1, report.UnreadableDayFiles.Count);
            ArchiveTestRecords.AssertSameValues<ICbboRecord>(putRecords, await this.cache.ReadServedCbboAsync(Put, October16), Put);
        }

        [TestMethod]
        public async Task Compact_OlderVersionWhoseDaysTheCurrentOneLists_IsRetired()
        {
            // an older full file and its delta a locked retire left; the current full file and the new delta list every day they hold
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(-1), 0, CacheFixture.CbboDay(Put, 1), CacheFixture.CbboDay(OtherCall, 1));
            string olderDelta = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(-1), 1, CacheFixture.CbboDay(Put, 2));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Put, 2), CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.DeltasWritten);
            Assert.IsFalse(File.Exists(older));
            Assert.IsFalse(File.Exists(olderDelta));
            Assert.AreEqual(0, report.OrphanFiles.Count);
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_DeltaOfANewerBuildListingADayTheVersionDoesNot_IsKeptAndReported()
        {
            // a delta whose full file is gone, as after a restore of an older backup over a newer delta: its put is in no other file
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string orphan = this.cache.WriteCbboFile(PreviousBuiltUtc.AddDays(1), 1, CacheFixture.CbboDay(Put, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(1, report.DeltasWritten);
            Assert.IsTrue(File.Exists(orphan));
            CollectionAssert.AreEqual(new[] { Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021500.delta.1.parquet") }, report.OrphanFiles.ToArray(), "under the cache root");
            Assert.AreEqual(0, report.FilesLeft.Count, "no delete of it was tried");
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousFullFile, Path.GetFileName(orphan) }, this.cache.CbboArchiveFileNames());
        }

        [TestMethod]
        public async Task Compact_UnreadableFileOutsideTheVersion_IsKeptAndReported()
        {
            // what a file that cannot be opened holds cannot be known, so it may be a day's only copy
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string stray = Path.Combine(this.cache.CbboDirectory, "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261117T021500.full.parquet");
            File.WriteAllBytes(stray, new byte[] { 1, 2, 3 });
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            CollectionAssert.AreEqual(new[] { Path.GetRelativePath(this.cache.Root, stray) }, report.OrphanFiles.ToArray());
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_NewFullFileWithALeftDeltaOfTheSameSecond_IsNamedAfterIt()
        {
            // a delta left from a full file built in the same second must never read as the new full file's delta
            string left = this.cache.WriteCbboFile(NowUtc, 1, CacheFixture.CbboDay(Put, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            CollectionAssert.AreEqual(new[] { Path.GetFileName(left), "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021531.full.parquet" }, this.cache.CbboArchiveFileNames());
            CollectionAssert.AreEqual(new[] { Path.GetRelativePath(this.cache.Root, left) }, report.OrphanFiles.ToArray(), "the new full file does not list its put");
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16));
        }

        [TestMethod]
        public async Task Compact_RebuildWithSeveralDeltaGenerations_TakesTheHighestAlone()
        {
            // an older generation is on disk only when a run was killed before retiring it, and the archive does not serve it
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string older = this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(OtherCall, 2), CacheFixture.CbboDay(Put, 5));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 3, CacheFixture.CbboDay(OtherCall, 4));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync(CompactionMode.Full);

            Assert.AreEqual(2L, report.SymbolDaysWritten);
            Assert.AreEqual(4, (await this.cache.ReadServedCbboAsync(OtherCall, October16)).Length);
            Assert.IsNull(await this.cache.ReadServedCbboAsync(Put, October16), "the older generation's day is not copied back");

            // the version the rebuild replaced is retired; the older generation lists a put no current file does, so it is kept
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), NewFullFile }, this.cache.CbboArchiveFileNames());
            CollectionAssert.AreEqual(new[] { Path.GetRelativePath(this.cache.Root, older) }, report.OrphanFiles.ToArray());
        }

        [TestMethod]
        public async Task Compact_UnreadableOlderGeneration_IsNotOpenedAsTheVersionAndIsKeptAndReported()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            string older = Path.Combine(this.cache.CbboDirectory, PreviousDelta(1));
            File.WriteAllBytes(older, new byte[] { 1, 2, 3 });
            this.cache.WriteCbboFile(PreviousBuiltUtc, 2, CacheFixture.CbboDay(OtherCall, 2));
            this.cache.SaveCbbo(Call, October16, 6600.5);

            ArchiveCompactionReport report = await this.CompactAsync();

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(2L, report.SymbolDaysWritten, "the current generation's day and the day file");
            Assert.IsTrue(File.Exists(older));
            CollectionAssert.AreEqual(new[] { Path.GetRelativePath(this.cache.Root, older) }, report.OrphanFiles.ToArray());
            CollectionAssert.AreEqual(new[] { PreviousDelta(1), PreviousDelta(3), PreviousFullFile }, this.cache.CbboArchiveFileNames(), "the generation the new one replaces is retired");
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(OtherCall, October16)).Length);
        }

        [TestMethod]
        public async Task Compact_RestoreInProgress_RefusesAndChangesNothing()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            CacheRestoreMarker.Write(this.cache.Root, "backup-20261118T021530", NowUtc);
            string[] before = this.cache.CacheFiles();

            InvalidOperationException exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => this.CompactAsync());

            StringAssert.Contains(exception.Message, "backup-20261118T021530");
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_RootWithFilesButNoMetadata_IsRefused()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            File.Delete(Path.Combine(this.cache.Root, "metadata.json"));
            string[] before = this.cache.CacheFiles();

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => this.CompactAsync());

            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_MissingRoot_Throws()
        {
            // a mistyped root in a scheduled job must not pass for a cache with nothing to compact
            string missing = Path.Combine(this.cache.Folder, "Missing");

            DirectoryNotFoundException exception = await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(() => this.CompactAsync(options => options.CacheRoot = missing));

            StringAssert.Contains(exception.Message, missing);
            Assert.IsFalse(Directory.Exists(missing), "nothing is created");
        }

        [TestMethod]
        public async Task Compact_EmptyRoot_HasNothingToDo()
        {
            string empty = Path.Combine(this.cache.Folder, "Empty");
            Directory.CreateDirectory(empty);

            ArchiveCompactionReport report = await this.CompactAsync(options => options.CacheRoot = empty);

            Assert.AreEqual(0, report.GroupsCompacted);
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(0, report.FoldersSkipped.Count);
            Assert.AreEqual(0, Directory.GetFileSystemEntries(empty).Length, "nothing is written");
        }

        [TestMethod]
        [DataRow("{\"format_version\":2,\"day_cutoff\":\"UTC\"}", DisplayName = "another format version")]
        [DataRow("{\"format_version\":1,\"day_cutoff\":\"ET\"}", DisplayName = "another cutoff")]
        public async Task Compact_MetadataThisBuildDoesNotAccept_Throws(string metadata)
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            File.WriteAllText(Path.Combine(this.cache.Root, "metadata.json"), metadata);
            string[] before = this.cache.CacheFiles();

            await Assert.ThrowsExceptionAsync<InvalidDataException>(() => this.CompactAsync());

            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        private static string PreviousDelta(int generation)
        {
            return Invariant($"OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.delta.{generation}.parquet");
        }

        private static string NewDelta(int generation)
        {
            return Invariant($"OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021530.delta.{generation}.parquet");
        }

        private static string Strike(int strike)
        {
            return Invariant($"SPXW  261016C{strike * 1000:00000000}");
        }

        private static ArchiveSymbolDay<ICbboRecord> Day(string rawSymbol, CbboRecordDbn[] records)
        {
            return new ArchiveSymbolDay<ICbboRecord>(rawSymbol, October16, records);
        }

        private static RecordHeaderJson Header(RType recordType)
        {
            return new RecordHeaderJson
            {
                RecordType = recordType,
                TsEventUtc = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc),
            };
        }

        private static TcbboRecordJson Tcbbo(double price, long tsInDelta)
        {
            return new TcbboRecordJson
            {
                Header = Header(RType.Tcbbo),
                Price = price,
                Side = TradeAggressor.Buyer,
                TsReceivedUtc = new DateTime(2026, 10, 16, 13, 30, 1, DateTimeKind.Utc),
                TsInDelta = tsInDelta,
            };
        }

        private static string CopyTo(string path, string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            string copy = Path.Combine(directory, fileName);
            File.Copy(path, copy);

            return copy;
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

        private Task<ArchiveCompactionReport> CompactAsync(CompactionMode mode, int maxDeltaGeneration = 9, int maxDeltaSizePercent = 25)
        {
            return this.CompactAsync(options =>
            {
                options.CompactionMode = mode;
                options.MaxDeltaGeneration = maxDeltaGeneration;
                options.MaxDeltaSizePercent = maxDeltaSizePercent;
            });
        }
    }
}
