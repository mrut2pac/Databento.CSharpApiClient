using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Cache;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// The cache root's lock file: one compaction at a time per root, and the lock released however a run ends
    /// </summary>
    [TestClass]
    public class DatabentoArchiveCompactorLockTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private static readonly DateTime October16 = new DateTime(2026, 10, 16);

        private static readonly DateTime NowUtc = new DateTime(2026, 11, 19, 2, 15, 30, DateTimeKind.Utc);

        private CacheFixture cache;

        private FakeTimeProvider clock;

        private string LockPath => Path.Combine(this.cache.Root, "cache.lock");

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
        public async Task Compact_SecondRunWhileTheFirstIsUnderWay_IsRefused()
        {
            string dayFile = this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor first = this.CreateCompactor();
            DatabentoArchiveCompactor second = this.CreateCompactor();
            Exception refused = null;
            bool lockedDuringTheRun = false;
            first.TemporaryFileWritten = _ =>
            {
                lockedDuringTheRun = File.Exists(this.LockPath);
                try
                {
                    second.CompactAsync().GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    refused = ex;
                }
            };

            ArchiveCompactionReport report = await first.CompactAsync();

            Assert.IsTrue(lockedDuringTheRun);
            Assert.IsInstanceOfType(refused, typeof(InvalidOperationException));
            StringAssert.Contains(refused.Message, this.LockPath);
            Assert.AreEqual(1, report.GroupsCompacted, "the first run goes on as if the second never started");
            Assert.AreEqual(0, report.FailedGroups.Count);
            Assert.IsFalse(File.Exists(dayFile));
            Assert.AreEqual(6600.5, (await this.cache.ReadServedCbboAsync(Call, October16)).Single().Price);
        }

        [TestMethod]
        public async Task Compact_LockHeldByAnother_IsRefusedAndChangesNothing()
        {
            // the backup and restore take the same lock
            this.cache.SaveCbbo(Call, October16, 6600.5);
            using(this.TakeLock())
            {
                string[] before = this.cache.CacheFiles();

                InvalidOperationException exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => this.CreateCompactor().CompactAsync());

                StringAssert.Contains(exception.Message, this.LockPath);
                CollectionAssert.AreEqual(before, this.cache.CacheFiles());

                // a refused open must not delete the holder's file, or a third run could take a lock of its own
                Assert.IsTrue(File.Exists(this.LockPath), "the holder's lock file is still there");
                Assert.ThrowsException<InvalidOperationException>(() => CacheRootLock.Take(this.cache.Root, "a third run"));
            }
        }

        [TestMethod]
        public async Task Compact_RunDone_ReleasesTheLock()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            await this.CreateCompactor().CompactAsync();

            this.AssertLockReleased();
            this.cache.SaveCbbo(Put, October16, 12.5);
            Assert.AreEqual(1, (await this.CreateCompactor().CompactAsync()).GroupsCompacted, "the next run takes the lock");
        }

        [TestMethod]
        public async Task Compact_RunThatFails_ReleasesTheLock()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            this.clock.TimestampFailure = new InvalidOperationException("The clock failed.");

            InvalidOperationException exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => this.CreateCompactor().CompactAsync());

            Assert.AreEqual("The clock failed.", exception.Message, "the run failed once it held the lock");
            this.AssertLockReleased();
        }

        [TestMethod]
        public async Task Compact_RunCanceled_ReleasesTheLock()
        {
            this.cache.SaveCbbo(Call, October16, 6600.5);
            DatabentoArchiveCompactor compactor = this.CreateCompactor();
            ArchiveCompactionReport report;
            using(CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                compactor.TemporaryFileWritten = _ => cancellation.Cancel();
                report = await compactor.CompactAsync(cancellation.Token);
            }

            Assert.IsTrue(report.Canceled);
            this.AssertLockReleased();
        }

        [TestMethod]
        public async Task Compact_RootHoldingOnlyALeftLockFile_HasNothingToDo()
        {
            // a run killed on Linux leaves its lock file, unlocked; a root of no cache holding it is still an empty one
            string empty = Path.Combine(this.cache.Folder, "Empty");
            Directory.CreateDirectory(empty);
            File.WriteAllBytes(Path.Combine(empty, "cache.lock"), Array.Empty<byte>());

            ArchiveCompactionReport report = await this.CreateCompactor(options => options.CacheRoot = empty).CompactAsync();

            Assert.AreEqual(0, report.GroupsCompacted);
            CollectionAssert.AreEqual(new[] { Path.Combine(empty, "cache.lock") }, Directory.GetFileSystemEntries(empty), "nothing is written");
        }

        private void AssertLockReleased()
        {
            Assert.IsFalse(File.Exists(this.LockPath), "the lock file is deleted as it is released");
            using(this.TakeLock())
            {
            }
        }

        /// <summary>
        /// Takes the cache root's lock as a run takes it
        /// </summary>
        private FileStream TakeLock()
        {
            return new FileStream(this.LockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
        }

        private DatabentoArchiveCompactor CreateCompactor(Action<ArchiveCompactorOptions> configure = null)
        {
            ArchiveCompactorOptions options = this.cache.CreateOptions(this.clock);
            configure?.Invoke(options);

            return new DatabentoArchiveCompactor(options);
        }
    }
}
