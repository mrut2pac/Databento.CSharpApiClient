using System;
using System.IO;
using System.Text.Json;

using Databento.CSharpApiClient.Caching.Cache;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Cache
{
    [TestClass]
    public class CacheMetadataTests
    {
        private string directory;

        private string Root => Path.Combine(this.directory, "Cache");

        private string MetadataPath => Path.Combine(this.Root, "metadata.json");

        [TestInitialize]
        public void TestInitialize()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "CacheMetadataTests_" + Guid.NewGuid().ToString("N"));
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
        public void Read_RootWithoutMetadata_ReturnsNull()
        {
            Assert.IsNull(CacheMetadata.Read(this.Root), "a missing root");

            Directory.CreateDirectory(this.Root);
            Assert.IsNull(CacheMetadata.Read(this.Root), "an empty root");
        }

        [TestMethod]
        public void EnsureCreated_MissingRoot_CreatesItWithTheMetadataOfThisBuild()
        {
            CacheMetadata metadata = CacheMetadata.EnsureCreated(this.Root);

            Assert.AreEqual(1, metadata.FormatVersion);
            Assert.AreEqual("UTC", metadata.DayCutoff);
            CollectionAssert.AreEqual(new[] { this.MetadataPath }, Directory.GetFiles(this.Root), "no temporary file is left");
            using(JsonDocument document = JsonDocument.Parse(File.ReadAllText(this.MetadataPath)))
            {
                Assert.AreEqual(1, document.RootElement.GetProperty("format_version").GetInt32());
                Assert.AreEqual("UTC", document.RootElement.GetProperty("day_cutoff").GetString());
            }
        }

        [TestMethod]
        public void Read_MetadataEnsureCreatedWrote_IsRead()
        {
            CacheMetadata.EnsureCreated(this.Root);

            CacheMetadata metadata = CacheMetadata.Read(this.Root);

            Assert.AreEqual(1, metadata.FormatVersion);
            Assert.AreEqual("UTC", metadata.DayCutoff);
        }

        [TestMethod]
        public void EnsureCreated_RootWithMetadata_LeavesItAsItIs()
        {
            Directory.CreateDirectory(this.Root);
            const string Json = "{ \"day_cutoff\": \"UTC\", \"format_version\": 1, \"written_by\": \"another build\" }";
            File.WriteAllText(this.MetadataPath, Json);

            CacheMetadata metadata = CacheMetadata.EnsureCreated(this.Root);

            Assert.AreEqual("UTC", metadata.DayCutoff);
            Assert.AreEqual(Json, File.ReadAllText(this.MetadataPath));
        }

        [TestMethod]
        [DataRow("{\"format_version\":2,\"day_cutoff\":\"UTC\"}", DisplayName = "another format version")]
        [DataRow("{\"format_version\":1,\"day_cutoff\":\"ET\"}", DisplayName = "another cutoff")]
        [DataRow("{\"format_version\":1,\"day_cutoff\":\"utc\"}", DisplayName = "the cutoff in another case")]
        [DataRow("{\"format_version\":1}", DisplayName = "no cutoff")]
        [DataRow("{\"day_cutoff\":\"UTC\"}", DisplayName = "no format version")]
        [DataRow("{\"format_version\":\"1\",\"day_cutoff\":\"UTC\"}", DisplayName = "a format version that is no number")]
        [DataRow("{\"format_version\":1.5,\"day_cutoff\":\"UTC\"}", DisplayName = "a format version that is no integer")]
        [DataRow("[]", DisplayName = "no object")]
        [DataRow("not json", DisplayName = "not JSON")]
        [DataRow("", DisplayName = "empty")]
        public void ReadAndEnsureCreated_MetadataThisBuildDoesNotAccept_Throw(string json)
        {
            Directory.CreateDirectory(this.Root);
            File.WriteAllText(this.MetadataPath, json);

            Assert.ThrowsException<InvalidDataException>(() => CacheMetadata.Read(this.Root));
            Assert.ThrowsException<InvalidDataException>(() => CacheMetadata.EnsureCreated(this.Root));
            Assert.AreEqual(json, File.ReadAllText(this.MetadataPath), "a file this build does not accept is never replaced");
        }

        [TestMethod]
        public void Read_NoRoot_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => CacheMetadata.Read(string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => CacheMetadata.Read(null));
        }
    }
}
