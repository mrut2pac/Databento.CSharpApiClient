// Ignore Spelling: Cbbo Tcbbo Bbo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.UnitTests.Archive;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// The compactor's day checks: shown the days of the schemas they judge, summarizing each day the new file holds, and their suspect
    /// day-file days reported and archived all the same
    /// </summary>
    [TestClass]
    public class DatabentoArchiveCompactorDayCheckTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private const string OtherCall = "SPXW  261016C06700000";

        private const int SessionMinutes = 60;

        private static readonly DateTime October16 = new DateTime(2026, 10, 16);

        private static readonly DateTime PreviousBuiltUtc = new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc);

        private static readonly DateTime NowUtc = new DateTime(2026, 11, 19, 2, 15, 30, DateTimeKind.Utc);

        /// <summary>
        /// The strikes <see cref="SaveVersionAndDayFiles"/> writes, by the letter the tests name them by
        /// </summary>
        private static readonly Dictionary<string, string> Letters = new Dictionary<string, string>
        {
            [Strike(6601)] = "A",
            [Strike(6602)] = "B",
            [Strike(6603)] = "C",
            [Strike(6604)] = "D",
            [Strike(6605)] = "E",
        };

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
        public async Task Compact_SuspectDay_IsReportedWithItsDayFileAndArchivedAllTheSame()
        {
            this.SavePeers(5);
            string shortDay = this.cache.SaveCbbo(Strike(6800), October16, 6600.5, 6600.75);
            DateTime written = File.GetLastWriteTimeUtc(shortDay);
            FakeArchiveDayCheck check = ShortDayCheck();

            ArchiveCompactionReport report = await this.CompactAsync(check);

            SuspectDayFile suspect = report.Suspects.Single();
            Assert.AreEqual(Path.Combine("OPRA.PILLAR", "cbbo-1m", "2026", "10", "SPXW%20%20261016C06800000_2026-10-16.UTC.json.gz"), suspect.RelativePath);
            Assert.AreEqual(Strike(6800), suspect.Day.Symbol);
            Assert.AreEqual(new DateOnly(2026, 10, 16), suspect.Day.Date);
            Assert.AreEqual(2, suspect.Day.RecordCount);
            Assert.AreEqual("short", suspect.Reason);
            Assert.AreEqual(60d, suspect.ExpectedRecords);
            Assert.AreEqual("2 of 60", suspect.Detail);
            Assert.AreEqual(written, suspect.LastWriteUtc);

            Assert.AreEqual(6L, report.DayFilesAbsorbed, "a suspect day is archived all the same");
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            Assert.AreEqual(2, (await this.cache.ReadServedCbboAsync(Strike(6800), October16)).Length);

            ArchiveGroupDays days = check.JudgedGroups.Single();
            Assert.AreEqual("OPRA.PILLAR", days.Dataset);
            Assert.AreEqual("cbbo-1m", days.Schema);
            Assert.AreEqual("SPXW.OPT", days.Group);
            Assert.AreEqual(2026, days.Year);
            Assert.AreEqual(10, days.Month);
            Assert.AreEqual(6, days.DayFileDays.Count);
            Assert.AreEqual(6, days.ServedDays.Count);
        }

        [TestMethod]
        public async Task Compact_NextDelta_SummarizesTheDaysOfTheNewFileAndJudgesOnlyItsDayFileDays()
        {
            FakeArchiveDayCheck check = this.SaveVersionAndDayFiles();

            ArchiveCompactionReport report = await this.CompactAsync(check);

            Assert.AreEqual(1, report.DeltasWritten);
            Assert.AreEqual(4L, report.SymbolDaysWritten);

            // the new delta holds the previous delta's days and the day files; the full file's are not read
            CollectionAssert.AreEqual(new[] { "B:4", "C:1", "D:2", "E:0" }, check.SummarizedDays.Select(day => Describe(day.Day)).ToArray());
            CollectionAssert.AreEqual(new double?[] { 7 }, check.SummarizedDays[1].Records.Select(record => ((ICbboRecord)record).Price).ToArray(), "the day file's records");
            CollectionAssert.AreEqual(new double?[] { 8, 9 }, check.SummarizedDays[2].Records.Select(record => ((ICbboRecord)record).Price).ToArray());

            ArchiveGroupDays days = check.JudgedGroups.Single();
            CollectionAssert.AreEquivalent(new[] { "A:5", "B:4", "C:1", "D:2", "E:0" }, days.ServedDays.Select(Describe).ToArray(), "the full file's days by their counts, the rest as the new delta holds them");
            CollectionAssert.AreEqual(new[] { "C:1", "D:2", "E:0" }, days.DayFileDays.Select(Describe).ToArray(), "the previous delta's day is carried over, not judged");
            Assert.IsNull(days.SummaryOf(ServedDay(days, "A")), "the full file's day is not in the new delta");
            Assert.AreEqual("B x4", days.SummaryOf(ServedDay(days, "B")));
            Assert.IsNull(days.SummaryOf(ServedDay(days, "C")), "the check kept nothing of a day of one record");
            Assert.AreEqual("D x2", days.SummaryOf(ServedDay(days, "D")));
        }

        [TestMethod]
        public async Task Compact_Rebuild_SummarizesEveryDayOfTheVersionAndJudgesTheDayFilesAgainstThemAll()
        {
            FakeArchiveDayCheck check = this.SaveVersionAndDayFiles();

            ArchiveCompactionReport report = await this.CompactAsync(check, CompactionMode.Full);

            Assert.AreEqual(1, report.FullFilesWritten);
            CollectionAssert.AreEqual(new[] { "A:5", "B:4", "C:1", "D:2", "E:0" }, check.SummarizedDays.Select(day => Describe(day.Day)).ToArray());

            ArchiveGroupDays days = check.JudgedGroups.Single();
            CollectionAssert.AreEquivalent(new[] { "A:5", "B:4", "C:1", "D:2", "E:0" }, days.ServedDays.Select(Describe).ToArray());
            CollectionAssert.AreEqual(new[] { "C:1", "D:2", "E:0" }, days.DayFileDays.Select(Describe).ToArray());
            Assert.AreEqual("A x5", days.SummaryOf(ServedDay(days, "A")), "a rebuild holds the full file's days too");
            Assert.AreEqual("B x4", days.SummaryOf(ServedDay(days, "B")));
        }

        [TestMethod]
        public async Task Compact_Check_IsGivenEachDaysRecordsReadOnly()
        {
            // the records go on to be written, so a check must not replace one
            this.cache.SaveCbbo(Call, October16, 6600.5, 6600.75);
            FakeArchiveDayCheck check = new FakeArchiveDayCheck();

            await this.CompactAsync(check);

            IReadOnlyList<IRecord> records = check.SummarizedDays.Single().Records;
            Assert.AreEqual(2, records.Count);
            Assert.IsTrue(((IList)records).IsReadOnly);
        }

        [TestMethod]
        public async Task Compact_Check_IsShownOnlyTheSchemasItAppliesTo()
        {
            DateTime received = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc);
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.cache.SaveDayFile(CacheFixture.Opra, "tcbbo", Call, October16, new TcbboRecordJson { Header = Header(RType.Tcbbo), Price = 12.5, Side = TradeAggressor.Buyer, TsReceivedUtc = received });
            this.cache.SaveDayFile("GLBX.MDP3", "bbo-1m", "MNQZ6", October16, new BboRecordJson { Header = Header(RType.Bbo1M), Side = TradeAggressor.None, TsReceivedUtc = received, Levels = new[] { new Mbp1LevelJson { BidPrice = 21000.25 } } });
            FakeArchiveDayCheck check = new FakeArchiveDayCheck
            {
                Judges = (dataset, schema) => schema == "tcbbo" || dataset == "GLBX.MDP3",
                Suspects = days => days.DayFileDays.Select(day => new ArchiveDaySuspect(day, "any", 0, null)),
            };

            ArchiveCompactionReport report = await this.CompactAsync(check);

            Assert.AreEqual(3, report.GroupsCompacted);
            CollectionAssert.AreEquivalent(new[] { "GLBX.MDP3 bbo-1m MNQ.FUT", "OPRA.PILLAR tcbbo SPXW.OPT" }, check.JudgedGroups.Select(days => Invariant($"{days.Dataset} {days.Schema} {days.Group}")).ToArray());
            CollectionAssert.AreEquivalent(new[] { "MNQZ6", Call }, check.SummarizedDays.Select(day => day.Day.Symbol).ToArray(), "the cbbo-1m day is never shown to it");
            Assert.AreEqual(12.5, check.SummarizedDays.Select(day => day.Records.Single()).OfType<ITcbboRecord>().Single().Price, "a day is summarized as its schema's records");
            CollectionAssert.AreEqual(
                new[]
                {
                    Path.Combine("GLBX.MDP3", "bbo-1m", "2026", "10", "MNQZ6_2026-10-16.UTC.json.gz"),
                    Path.Combine("OPRA.PILLAR", "tcbbo", "2026", "10", "SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz"),
                },
                report.Suspects.Select(suspect => suspect.RelativePath).ToArray());
        }

        [TestMethod]
        public async Task Compact_SuspectOfADayNotFromADayFile_IsIgnored()
        {
            // a check may name any day; only those the run takes from day files are its to report
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 5));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            FakeArchiveDayCheck check = new FakeArchiveDayCheck
            {
                Suspects = days => days.ServedDays.Select(day => new ArchiveDaySuspect(day, "served", 0, null))
                    .Append(new ArchiveDaySuspect(new ArchiveDay(Call, new DateOnly(2026, 11, 2), 1), "another month", 0, null))
                    .Append(new ArchiveDaySuspect(new ArchiveDay(Put, new DateOnly(2026, 10, 16), 3), "not served", 0, null))
                    .Append(null),
            };

            ArchiveCompactionReport report = await this.CompactAsync(check);

            Assert.AreEqual(2, check.JudgedGroups.Single().ServedDays.Count, "the archived day is served beside the day file's");
            SuspectDayFile suspect = report.Suspects.Single();
            Assert.AreEqual(Call, suspect.Day.Symbol);
            Assert.AreEqual("served", suspect.Reason);
            Assert.AreEqual(1, report.GroupsCompacted);
        }

        [TestMethod]
        public async Task Compact_SuspectOfADayARebuildCopies_IsIgnored()
        {
            // a rebuild copies the archived day into its new file, but the day is not the run's to report
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 5));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            FakeArchiveDayCheck check = new FakeArchiveDayCheck
            {
                Suspects = days => days.ServedDays.Select(day => new ArchiveDaySuspect(day, "served", 0, null)),
            };

            ArchiveCompactionReport report = await this.CompactAsync(check, CompactionMode.Full);

            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(1, report.FullFilesWritten, "the run rebuilt the full file, copying the archived day");
            Assert.AreEqual(Call, report.Suspects.Single().Day.Symbol);
        }

        [TestMethod]
        public async Task Compact_CheckFindingNothing_MayAnswerNull()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            FakeArchiveDayCheck check = new FakeArchiveDayCheck
            {
                Suspects = days => null,
            };

            ArchiveCompactionReport report = await this.CompactAsync(check);

            Assert.AreEqual(1, report.GroupsCompacted);
            Assert.AreEqual(0, report.Suspects.Count);
        }

        [TestMethod]
        public async Task Compact_TwoChecksFindingTheSameDay_ListItOnce()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            FakeArchiveDayCheck first = new FakeArchiveDayCheck { Suspects = days => days.DayFileDays.Select(day => new ArchiveDaySuspect(day, "first", 0, null)) };
            FakeArchiveDayCheck second = new FakeArchiveDayCheck { Suspects = days => days.DayFileDays.Select(day => new ArchiveDaySuspect(day, "second", 0, null)) };

            ArchiveCompactionReport report = await this.CompactAsync(new IArchiveDayCheck[] { first, second });

            Assert.AreEqual("first", report.Suspects.Single().Reason);
            Assert.AreEqual(1, second.JudgedGroups.Count, "every check judges the days");
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
        }

        [TestMethod]
        [DataRow("Summarize")]
        [DataRow("FindSuspects")]
        public async Task Compact_CheckThatThrows_FailsTheGroupWithNothingListedOrChanged(string failingCall)
        {
            // the first check finds a suspect, the second throws: nothing is listed for a group that publishes nothing
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(OtherCall, 1));
            this.cache.SaveCbbo(Call, October16, 6600.5);
            string[] before = this.cache.CacheFiles();
            FakeArchiveDayCheck failing = new FakeArchiveDayCheck();
            if(failingCall == "Summarize")
            {
                failing.Summary = (day, records) => throw new InvalidOperationException("The check failed.");
            }
            else
            {
                failing.Suspects = days => throw new InvalidOperationException("The check failed.");
            }

            ArchiveCompactionReport report = await this.CompactAsync(new IArchiveDayCheck[] { ShortDayCheck(), failing });

            ArchiveGroupFailure failure = report.FailedGroups.Single();
            Assert.IsInstanceOfType(failure.Exception, typeof(InvalidOperationException));
            Assert.AreEqual("The check failed.", failure.Exception.Message);
            Assert.AreEqual(0, report.Suspects.Count);
            Assert.AreEqual(0, report.GroupsCompacted);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles(), "no file is published and no temporary file is left");
        }

        [TestMethod]
        public async Task Compact_DryRun_ReportsTheSuspectsAndChangesNothing()
        {
            this.SavePeers(5);
            this.cache.SaveCbbo(Strike(6800), October16, 6600.5);
            string[] before = this.cache.CacheFiles();

            ArchiveCompactionReport report = await this.CreateCompactor(new IArchiveDayCheck[] { ShortDayCheck() }, options => options.DryRun = true).CompactAsync();

            Assert.AreEqual(1, report.Suspects.Count);
            Assert.AreEqual(6L, report.SymbolDaysWritten);
            CollectionAssert.AreEqual(before, this.cache.CacheFiles());
        }

        [TestMethod]
        public async Task Compact_DayDownloadedAgain_ReplacesTheSuspectArchivedCopy()
        {
            // the fix for a suspect day: its new day file is read ahead of the archive, and the next run's delta wins over the old copy
            this.SavePeers(5);
            this.cache.SaveCbbo(Strike(6800), October16, 6600.5);
            Assert.AreEqual(1, (await this.CompactAsync(ShortDayCheck())).Suspects.Count);
            this.clock.Advance(TimeSpan.FromDays(1));
            this.cache.SaveCbbo(Strike(6800), October16, Prices(SessionMinutes));

            ArchiveCompactionReport report = await this.CompactAsync(ShortDayCheck());

            Assert.AreEqual(0, report.Suspects.Count);
            Assert.AreEqual(1L, report.DayFilesAbsorbed);
            Assert.AreEqual(1, report.DeltasWritten);
            Assert.AreEqual(0, this.cache.DayFiles(this.cache.CbboDirectory).Length);
            Assert.AreEqual(SessionMinutes, (await this.cache.ReadServedCbboAsync(Strike(6800), October16)).Length, "the delta wins over the old copy");
        }

        private static string Strike(int strike)
        {
            return Invariant($"SPXW  261016C{strike * 1000:00000000}");
        }

        private static double[] Prices(int count)
        {
            return Enumerable.Range(0, count).Select(minute => 6600.25 + minute).ToArray();
        }

        private static RecordHeaderJson Header(RType recordType)
        {
            return new RecordHeaderJson
            {
                RecordType = recordType,
                TsEventUtc = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc),
            };
        }

        /// <summary>
        /// A check finding each day-file day of fewer than 30 records suspect, judged against a session of 60
        /// </summary>
        private static FakeArchiveDayCheck ShortDayCheck()
        {
            return new FakeArchiveDayCheck
            {
                Suspects = days => days.DayFileDays.Where(day => day.RecordCount < 30).Select(day => new ArchiveDaySuspect(day, "short", 60, Invariant($"{day.RecordCount} of 60"))),
            };
        }

        /// <summary>
        /// The letter <see cref="SaveVersionAndDayFiles"/> names a day's strike by, and the day's record count
        /// </summary>
        private static string Describe(ArchiveDay day)
        {
            return Invariant($"{Letters[day.Symbol]}:{day.RecordCount}");
        }

        private static ArchiveDay ServedDay(ArchiveGroupDays days, string letter)
        {
            return days.ServedDays.Single(day => Describe(day).StartsWith(letter + ":", StringComparison.Ordinal));
        }

        /// <summary>
        /// Writes a version and day files of strikes 6601 (A) to 6605 (E): the full file holds A of 5 records and B of 5, its delta B of 4
        /// and C of 3; the day files hold C of 1, D of 2 and E of none. Returns a check that keeps a symbol-and-count summary of each day of
        /// more than one record.
        /// </summary>
        private FakeArchiveDayCheck SaveVersionAndDayFiles()
        {
            this.cache.WriteCbboFile(PreviousBuiltUtc, 0, CacheFixture.CbboDay(Strike(6601), 5), CacheFixture.CbboDay(Strike(6602), 5));
            this.cache.WriteCbboFile(PreviousBuiltUtc, 1, CacheFixture.CbboDay(Strike(6602), 4), CacheFixture.CbboDay(Strike(6603), 3));
            this.cache.SaveCbbo(Strike(6603), October16, 7);
            this.cache.SaveCbbo(Strike(6604), October16, 8, 9);
            this.cache.SaveCbbo(Strike(6605), October16);

            return new FakeArchiveDayCheck
            {
                Summary = (day, records) => records.Count > 1 ? Invariant($"{Describe(day)[0]} x{records.Count}") : null,
            };
        }

        private void SavePeers(int count)
        {
            for(int strike = 0; strike < count; ++strike)
            {
                this.cache.SaveCbbo(Strike(6600 + strike), October16, Prices(SessionMinutes));
            }
        }

        private DatabentoArchiveCompactor CreateCompactor(IReadOnlyList<IArchiveDayCheck> checks, Action<ArchiveCompactorOptions> configure = null)
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            options.DayChecks = checks;
            configure?.Invoke(options);

            return new DatabentoArchiveCompactor(options);
        }

        private Task<ArchiveCompactionReport> CompactAsync(FakeArchiveDayCheck check, CompactionMode mode = CompactionMode.Delta)
        {
            return this.CreateCompactor(new IArchiveDayCheck[] { check }, options => options.CompactionMode = mode).CompactAsync();
        }

        private Task<ArchiveCompactionReport> CompactAsync(IReadOnlyList<IArchiveDayCheck> checks)
        {
            return this.CreateCompactor(checks).CompactAsync();
        }
    }
}
