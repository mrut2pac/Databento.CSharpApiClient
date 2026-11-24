using System;
using System.IO;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive
{
    [TestClass]
    public class ArchiveFileNameTests
    {
        private const string Prefix = "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_";

        private static readonly DateTime BuiltUtc = new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc);

        private string directory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "ArchiveFileNameTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.directory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(Directory.Exists(this.directory))
            {
                Directory.Delete(this.directory, recursive: true);
            }
        }

        [TestMethod]
        public void GetSearchPrefix_GroupMonth_NamesEverythingUpToTheBuildTime()
        {
            Assert.AreEqual(Prefix, ArchiveFileName.GetSearchPrefix("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC"));
            Assert.AreEqual("XNAS.ITCH_ohlcv-1d_BRK%20B_2026-03.UTC_", ArchiveFileName.GetSearchPrefix("XNAS.ITCH", "ohlcv-1d", "BRK B", 2026, 3, "UTC"));
        }

        [TestMethod]
        public void GetTemporaryFileName_FinalName_NeverLooksLikeAnArchiveFile()
        {
            string fileName = Identity(BuiltUtc).FileName;

            string temporaryFileName = ArchiveFileName.GetTemporaryFileName(fileName);

            StringAssert.StartsWith(temporaryFileName, fileName + ".");
            StringAssert.EndsWith(temporaryFileName, ".tmp");
            Assert.AreNotEqual(temporaryFileName, ArchiveFileName.GetTemporaryFileName(fileName), "each write gets a name of its own");
        }

        [TestMethod]
        public void FindCurrentVersion_NoFile_ReturnsNull()
        {
            Assert.IsNull(ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix));
        }

        [TestMethod]
        public void FindCurrentVersion_SeveralBuilds_ReturnsTheNewestFullFile()
        {
            // the build time in the name decides, not the file system's dates, which a copy from a backup resets
            string older = this.Touch(Identity(BuiltUtc).FileName);
            string newest = this.Touch(Identity(BuiltUtc.AddDays(1)).FileName);
            File.SetCreationTimeUtc(older, DateTime.UtcNow.AddDays(1));
            File.SetLastWriteTimeUtc(older, DateTime.UtcNow.AddDays(1));

            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix);

            Assert.AreEqual(newest, version.FullPath);
            Assert.AreEqual(new DateTime(2026, 11, 19, 2, 15, 0, DateTimeKind.Utc), version.BuiltUtc);
            Assert.AreEqual(DateTimeKind.Utc, version.BuiltUtc.Kind);
            Assert.IsNull(version.DeltaPath);
            Assert.AreEqual(0, version.DeltaGeneration);
            CollectionAssert.AreEqual(new[] { newest }, version.Paths.ToArray());
        }

        [TestMethod]
        public void FindCurrentVersion_SeveralGenerations_TheHighestIsCurrent()
        {
            string full = this.Touch(Identity(BuiltUtc).FileName);
            string[] deltas = new[] { 1, 2, 9, 10 }.Select(generation => this.Touch(Identity(BuiltUtc, generation).FileName)).ToArray();
            this.Touch(Identity(BuiltUtc.AddDays(-1), 11).FileName);

            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix);

            Assert.AreEqual(full, version.FullPath);
            Assert.AreEqual(deltas[^1], version.DeltaPath, "10 comes after 9, and the older build's delta is not its");
            Assert.AreEqual(10, version.DeltaGeneration);
            CollectionAssert.AreEqual(new[] { full, deltas[^1] }, version.Paths.ToArray(), "an older generation is not part of the version");
        }

        [TestMethod]
        public void FindCurrentVersion_DeltaOfAnOlderBuild_IsNotTheCurrentDelta()
        {
            // left by a rebuild killed after publishing the new full file and before retiring the old one's delta
            this.Touch(Identity(BuiltUtc).FileName);
            this.Touch(Identity(BuiltUtc, 3).FileName);
            string newest = this.Touch(Identity(BuiltUtc.AddDays(1)).FileName);

            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix);

            Assert.AreEqual(newest, version.FullPath);
            Assert.IsNull(version.DeltaPath);
            Assert.AreEqual(0, version.DeltaGeneration);
        }

        [TestMethod]
        public void FindCurrentVersion_DeltasWithoutTheirFullFile_AreNotAVersion()
        {
            // left by a full file removed by hand, or by a restore of an older backup over a newer delta
            string full = this.Touch(Identity(BuiltUtc).FileName);
            this.Touch(Identity(BuiltUtc.AddDays(1), 1).FileName);

            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix);

            Assert.AreEqual(full, version.FullPath);
            Assert.IsNull(version.DeltaPath);
        }

        [TestMethod]
        public void FindCurrentVersion_OnlyADelta_IsNoVersion()
        {
            this.Touch(Identity(BuiltUtc, 1).FileName);

            Assert.IsNull(ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix));
        }

        [TestMethod]
        public void FindCurrentVersion_IgnoresFilesThatAreNotThisArchive()
        {
            string current = this.Touch(Identity(BuiltUtc).FileName);
            DateTime later = BuiltUtc.AddDays(1);
            this.Touch(ArchiveFileName.GetTemporaryFileName(Identity(later).FileName));
            this.Touch(ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc, 1).FileName));
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPX.OPT", 2026, 10, "UTC", later).FileName);
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1s", "SPXW.OPT", 2026, 10, "UTC", later).FileName);
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 9, "UTC", later).FileName);
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "ET", later).FileName);
            this.Touch(new ArchiveFileIdentity("OPRA.TEST", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC", later).FileName);
            this.Touch(Prefix + "notatimestamp.full.parquet");
            this.Touch(Prefix + "20261119T021500.parquet");
            this.Touch(Prefix + "20261119T021500.FULL.parquet");
            this.Touch(Prefix + "20261119T021500.full.1.parquet");
            this.Touch(Prefix + "20261119T0215000.full.parquet");
            this.Touch(Prefix + "20261319T021500.full.parquet");
            this.Touch("SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz");

            // a delta generation is 1 and up, written as the writer writes it
            foreach(string generation in new[] { "0", "01", "-1", "+1", "x", string.Empty, " 1" })
            {
                this.Touch(Invariant($"{Prefix}20261118T021500.delta.{generation}.parquet"));
            }

            this.Touch(Prefix + "20261118T021500.delta.parquet");
            this.Touch(Prefix + "20261118T021500.Delta.1.parquet");

            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(this.ListFiles(), Prefix);

            Assert.AreEqual(current, version.FullPath);
            Assert.IsNull(version.DeltaPath);
        }

        [TestMethod]
        public void FindFiles_FilesOfSeveralBuilds_ListsEveryFullFileAndDeltaOfTheGroupMonth()
        {
            string[] expected =
            {
                this.Touch(Identity(BuiltUtc).FileName),
                this.Touch(Identity(BuiltUtc, 1).FileName),
                this.Touch(Identity(BuiltUtc.AddDays(1)).FileName),
                this.Touch(Identity(BuiltUtc.AddDays(2), 1).FileName),
            };
            this.Touch(ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc.AddDays(3)).FileName));
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPX.OPT", 2026, 10, "UTC", BuiltUtc).FileName);

            string[] files = ArchiveFileName.FindFiles(this.ListFiles(), Prefix).ToArray();

            CollectionAssert.AreEquivalent(expected, files);
            Assert.AreEqual(0, ArchiveFileName.FindFiles(Array.Empty<string>(), Prefix).Count);
        }

        [TestMethod]
        public void FindLatestBuildTime_SeveralBuilds_IsTheNewestOfAnyFullFileOrDelta()
        {
            this.Touch(Identity(BuiltUtc).FileName);
            this.Touch(Identity(BuiltUtc.AddDays(2), 1).FileName);
            this.Touch(ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc.AddDays(3)).FileName));
            this.Touch(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPX.OPT", 2026, 10, "UTC", BuiltUtc.AddDays(4)).FileName);

            Assert.AreEqual(new DateTime(2026, 11, 20, 2, 15, 0, DateTimeKind.Utc), ArchiveFileName.FindLatestBuildTime(this.ListFiles(), Prefix));
            Assert.IsNull(ArchiveFileName.FindLatestBuildTime(this.ListFiles(), "OPRA.PILLAR_cbbo-1m_NDXP.OPT_2026-10.UTC_"));
        }

        [TestMethod]
        public void IsTemporaryFileOf_TemporaryFilesOfSeveralGroups_AreThoseOfTheGroupMonthOnly()
        {
            string[] ofTheGroupMonth =
            {
                ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc).FileName),
                ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc, 2).FileName),
            };
            string[] others =
            {
                ArchiveFileName.GetTemporaryFileName(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPX.OPT", 2026, 10, "UTC", BuiltUtc).FileName),
                ArchiveFileName.GetTemporaryFileName(new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 9, "UTC", BuiltUtc).FileName),
                Identity(BuiltUtc).FileName,
                "SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz.0123456789abcdef0123456789abcdef.tmp",
                Prefix + "notatimestamp.full.parquet.0123456789abcdef0123456789abcdef.tmp",
                Identity(BuiltUtc).FileName + ".tmp",
                ".tmp",
            };

            foreach(string fileName in ofTheGroupMonth)
            {
                Assert.IsTrue(ArchiveFileName.IsTemporaryFileOf(fileName, Prefix), fileName);
            }

            foreach(string fileName in others)
            {
                Assert.IsFalse(ArchiveFileName.IsTemporaryFileOf(fileName, Prefix), fileName);
            }
        }

        [TestMethod]
        public void TryParseGroup_FullFileDeltaAndTheirTemporaryFiles_GiveTheGroup()
        {
            string[] fileNames =
            {
                Identity(BuiltUtc).FileName,
                Identity(BuiltUtc, 3).FileName,
                ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc).FileName),
                ArchiveFileName.GetTemporaryFileName(Identity(BuiltUtc, 4).FileName),
            };

            foreach(string fileName in fileNames)
            {
                Assert.IsTrue(ArchiveFileName.TryParseGroup(fileName, "OPRA.PILLAR", "cbbo-1m", 2026, 10, "UTC", out string group), fileName);
                Assert.AreEqual("SPXW.OPT", group, fileName);
            }
        }

        [TestMethod]
        public void TryParseGroup_EscapedGroup_IsUnescaped()
        {
            Assert.IsTrue(ArchiveFileName.TryParseGroup("XNAS.ITCH_ohlcv-1d_BRK%20B_2026-10.UTC_20261118T021500.full.parquet", "XNAS.ITCH", "ohlcv-1d", 2026, 10, "UTC", out string group));

            Assert.AreEqual("BRK B", group);
        }

        [TestMethod]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz", DisplayName = "a day file")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz.0123456789abcdef0123456789abcdef.tmp", DisplayName = "a day file's temporary file")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-11.UTC_20261118T021500.full.parquet", DisplayName = "another month")]
        [DataRow("OPRA.PILLAR_cbbo-1s_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "another schema")]
        [DataRow("XNAS.ITCH_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "another dataset")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.ET_20261118T021500.full.parquet", DisplayName = "another cutoff")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.0.parquet", DisplayName = "an old numbered name")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.delta.01.parquet", DisplayName = "a generation not written as the writer writes it")]
        [DataRow("OPRA.PILLAR_cbbo-1m__2026-10.UTC_20261118T021500.full.parquet", DisplayName = "no group")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW%2EOPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "a group escaped where it needs no escape")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_extra_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "a part too many")]
        [DataRow("being-written.tmp", DisplayName = "a temporary file of no archive file")]
        [DataRow(".tmp", DisplayName = "the temporary extension alone")]
        [DataRow("x.parquet..tmp", DisplayName = "a temporary file of no archive file and no guid")]
        public void TryParseGroup_NoArchiveFileOfTheFolder_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(ArchiveFileName.TryParseGroup(fileName, "OPRA.PILLAR", "cbbo-1m", 2026, 10, "UTC", out string group));
            Assert.IsNull(group);
        }

        private static ArchiveFileIdentity Identity(DateTime builtUtc, int generation = 0)
        {
            return new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC", builtUtc, generation);
        }

        private string Touch(string fileName)
        {
            string path = Path.Combine(this.directory, fileName);
            File.WriteAllBytes(path, Array.Empty<byte>());

            return path;
        }

        /// <summary>
        /// Lists the folder as the compactor lists a month folder
        /// </summary>
        private string[] ListFiles()
        {
            return Directory.GetFiles(this.directory);
        }
    }
}
