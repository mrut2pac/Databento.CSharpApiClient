using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    [TestClass]
    public class ArchiveCompactionReportTests
    {
        private const int Threads = 8;

        private const int CallsPerThread = 20_000;

        [TestMethod]
        public void Report_FilledByGroupsCompactedAtOnce_LosesNothing()
        {
            ArchiveCompactionReport report = new ArchiveCompactionReport(false);
            InvalidOperationException failure = new InvalidOperationException("reason");
            ArchiveDaySuspect suspect = new ArchiveDaySuspect(new ArchiveDay("SPXW  261016C06600000", new DateOnly(2026, 10, 16), 2), "short", 60, null);

            Parallel.For(0, Threads, new ParallelOptions { MaxDegreeOfParallelism = Threads }, thread =>
            {
                for(int call = 0; call < CallsPerThread; ++call)
                {
                    report.GroupWritten(thread % 2 == 0 ? ArchiveFileKind.Full : ArchiveFileKind.Delta, 1, thread % 2 == 0, 2, 3, 4, 5);
                    report.DayFileDeleted();
                    report.DayFileChangedDuringRun();
                    report.DayFileMatchingArchive();
                    report.EmptyDayFileDropped();
                    report.DayFileTooRecent();
                    report.GroupFailed("group", failure);
                    report.DayFileUnreadable("path", "reason");
                    report.FileLeft("path");
                    report.FileOrphaned("orphan");
                    report.FolderSkipped("folder");
                    report.DayFileSuspect(new SuspectDayFile("path", suspect, DateTime.UnixEpoch));
                }
            });

            const long Expected = Threads * CallsPerThread;
            Assert.AreEqual(Expected, report.GroupsCompacted);
            Assert.AreEqual(Expected / 2, report.FullFilesWritten);
            Assert.AreEqual(Expected / 2, report.DeltasWritten);
            Assert.AreEqual(Expected / 2, report.DeltasFolded);
            Assert.AreEqual(Expected, report.DayFilesAbsorbed);
            Assert.AreEqual(Expected * 2, report.SymbolDaysWritten);
            Assert.AreEqual(Expected * 3, report.RowsWritten);
            Assert.AreEqual(Expected * 4, report.BytesBefore);
            Assert.AreEqual(Expected * 5, report.BytesAfter);
            Assert.AreEqual(Expected, report.DayFilesDeleted);
            Assert.AreEqual(Expected, report.DayFilesChangedDuringRun);
            Assert.AreEqual(Expected, report.DayFilesMatchingArchive);
            Assert.AreEqual(Expected, report.EmptyDayFilesDropped);
            Assert.AreEqual(Expected, report.DayFilesTooRecent);
            Assert.AreEqual(Expected, report.FailedGroups.Count(group => group.Group == "group" && group.Exception == failure));
            Assert.AreEqual(Expected, report.UnreadableDayFiles.Count(dayFile => dayFile.RelativePath == "path" && dayFile.Error == "reason"));
            Assert.AreEqual(Expected, report.FilesLeft.Count(file => file == "path"));
            Assert.AreEqual(Expected, report.OrphanFiles.Count(file => file == "orphan"));
            Assert.AreEqual(Expected, report.FoldersSkipped.Count(folder => folder == "folder"));
            Assert.AreEqual(Expected, report.Suspects.Count(dayFile => dayFile.RelativePath == "path"));
        }

        [TestMethod]
        public void Suspects_SeveralDayFiles_AreOrderedByPathAndCarryTheirFinding()
        {
            ArchiveCompactionReport report = new ArchiveCompactionReport(true);
            ArchiveDay day = new ArchiveDay("SPXW  261016C06600000", new DateOnly(2026, 10, 16), 2);
            DateTime written = new DateTime(2026, 10, 17, 1, 2, 3, DateTimeKind.Utc);
            report.DayFileSuspect(new SuspectDayFile("b", new ArchiveDaySuspect(day, "short", 60, "2 of 60"), written));
            report.DayFileSuspect(new SuspectDayFile("B", new ArchiveDaySuspect(day, "late", 0, null), written));
            report.DayFileSuspect(new SuspectDayFile("a", new ArchiveDaySuspect(day, "early", 0, null), written));

            CollectionAssert.AreEqual(new[] { "B", "a", "b" }, report.Suspects.Select(suspect => suspect.RelativePath).ToArray(), "ordinal order");
            SuspectDayFile last = report.Suspects[2];
            Assert.AreSame(day, last.Day);
            Assert.AreEqual("short", last.Reason);
            Assert.AreEqual(60d, last.ExpectedRecords);
            Assert.AreEqual("2 of 60", last.Detail);
            Assert.AreEqual(written, last.LastWriteUtc);
            Assert.IsTrue(report.DryRun);
        }

        [TestMethod]
        public void FilesLeft_ReadBeforeAnotherFileIsLeft_IsNotChanged()
        {
            ArchiveCompactionReport report = new ArchiveCompactionReport(false);
            report.FileLeft("first");

            IReadOnlyList<string> before = report.FilesLeft;
            report.FileLeft("second");

            Assert.AreEqual(1, before.Count, "a list read before is not changed by what comes after");
            Assert.AreEqual(2, report.FilesLeft.Count);
        }
    }
}
