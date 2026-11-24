using System;
using System.IO;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Compaction;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Compaction
{
    [TestClass]
    public class CompactionProgressTests
    {
        private const string Group = "SPXW.OPT";

        private const int DayFiles = 42_470;

        private static readonly string Folder = Path.Combine("OPRA.PILLAR", "cbbo-1m", "2022", "02");

        [TestMethod]
        public void StartGroup_FirstGroup_IsEstimating()
        {
            CompactionProgress progress = new CompactionProgress(3, 1_000);

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);

            Assert.AreEqual(Folder, started.RelativeDirectory);
            Assert.AreEqual(Group, started.Group);
            Assert.AreEqual(DayFiles, started.DayFiles);
            Assert.AreEqual(1, started.GroupNumber);
            Assert.AreEqual(3, started.PlannedGroups);
            Assert.AreEqual(0L, started.DayFilesDone);
            Assert.AreEqual(1_000L, started.PlannedDayFiles);
            Assert.AreEqual(TimeSpan.Zero, started.Elapsed);
            Assert.IsNull(started.EstimatedRemaining, "nothing is done to estimate by");
        }

        [TestMethod]
        public void StartGroup_HalfTheDayFilesDone_ProjectsTheRestAtThePaceSoFar()
        {
            CompactionProgress progress = new CompactionProgress(3, 1_000);
            progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);
            progress.EndGroup(500);

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.FromMinutes(10));

            Assert.AreEqual(2, started.GroupNumber);
            Assert.AreEqual(500L, started.DayFilesDone);
            Assert.AreEqual(TimeSpan.FromMinutes(10), started.Elapsed);
            Assert.AreEqual(TimeSpan.FromMinutes(10), started.EstimatedRemaining);
        }

        [TestMethod]
        public void StartGroup_ATenthDone_ProjectsNineTimesTheTimeSoFar()
        {
            CompactionProgress progress = new CompactionProgress(2, 1_000);
            progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);
            progress.EndGroup(100);

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.FromHours(1));

            Assert.AreEqual(TimeSpan.FromHours(9), started.EstimatedRemaining);
        }

        [TestMethod]
        public void StartGroup_FinishedGroupWithoutDayFiles_IsStillEstimating()
        {
            // a group with no day files, such as one only cleaned, says nothing of the pace
            CompactionProgress progress = new CompactionProgress(2, 1_000);
            progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);
            progress.EndGroup(0);

            Assert.IsNull(progress.StartGroup(Folder, Group, DayFiles, TimeSpan.FromMinutes(1)).EstimatedRemaining);
        }

        [TestMethod]
        public void StartGroup_MoreWorkThanPlanned_NeverShowsANegativeWait()
        {
            // day files saved while the run goes on make the month folders hold more than the planning pass counted
            CompactionProgress progress = new CompactionProgress(1, 100);
            progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);
            progress.EndGroup(150);

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.FromMinutes(1));

            Assert.AreEqual(2, started.GroupNumber);
            Assert.AreEqual(2, started.PlannedGroups, "the groups started outgrow the plan");
            Assert.AreEqual(150L, started.DayFilesDone);
            Assert.AreEqual(TimeSpan.Zero, started.EstimatedRemaining);
        }

        [TestMethod]
        public void StartGroup_NothingPlanned_StillCounts()
        {
            CompactionProgress progress = new CompactionProgress(0, 0);

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);

            Assert.AreEqual(1, started.GroupNumber);
            Assert.AreEqual(1, started.PlannedGroups);
            Assert.IsNull(started.EstimatedRemaining);
        }

        [TestMethod]
        public void StartGroup_GroupsCompactedAtOnce_CountsEveryGroupAndDayFile()
        {
            CompactionProgress progress = new CompactionProgress(10_001, 1_600_000);

            Parallel.For(0, 10_000, new ParallelOptions { MaxDegreeOfParallelism = 8 }, group =>
            {
                progress.StartGroup(Folder, Group, DayFiles, TimeSpan.Zero);
            });
            Parallel.For(0, 8, new ParallelOptions { MaxDegreeOfParallelism = 8 }, thread =>
            {
                for(int group = 0; group < 100_000; ++group)
                {
                    progress.EndGroup(1);
                }
            });

            ArchiveCompactionProgress started = progress.StartGroup(Folder, Group, DayFiles, TimeSpan.FromMinutes(10));

            Assert.AreEqual(10_001, started.GroupNumber);
            Assert.AreEqual(10_001, started.PlannedGroups);
            Assert.AreEqual(800_000L, started.DayFilesDone);
            Assert.AreEqual(TimeSpan.FromMinutes(10), started.EstimatedRemaining);
        }
    }
}
