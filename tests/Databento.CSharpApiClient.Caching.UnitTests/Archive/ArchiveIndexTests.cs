using System;
using System.IO;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive
{
    [TestClass]
    public class ArchiveIndexTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private const string Header = "{\"format_version\":1,\"dataset\":\"OPRA.PILLAR\",\"schema\":\"cbbo-1m\",\"group\":\"SPXW.OPT\",\"month\":\"2026-10\",\"cutoff\":\"UTC\",\"built_utc\":\"2026-11-18T02:15:00Z\"";

        private static readonly ArchiveFileIdentity Identity = new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC", new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc));

        [TestMethod]
        public void TryLocate_RowsFollowSymbolThenDayOrder_WhateverTheInputOrder()
        {
            // the rows sit in the file in this order, so a day's first row is the sum of the counts before it
            ArchiveIndex index = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Put, 16, 5),
                new ArchiveIndexEntry(Call, 16, 7),
                new ArchiveIndexEntry(Call, 15, 3),
            });

            AssertLocation(index, Call, 15, 0, 3);
            AssertLocation(index, Call, 16, 3, 7);
            AssertLocation(index, Put, 16, 10, 5);
            Assert.AreEqual(15L, index.RowCount);
            CollectionAssert.AreEqual(new[] { Call, Call, Put }, index.Entries.Select(entry => entry.Symbol).ToArray());
            CollectionAssert.AreEqual(new[] { 15, 16, 16 }, index.Entries.Select(entry => entry.Day).ToArray());
        }

        [TestMethod]
        public void TryLocate_EmptyDay_IsFoundWithNoRecords()
        {
            // an empty day is final: the vendor has nothing for it, and it must not be asked again
            ArchiveIndex index = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Call, 15, 0),
                new ArchiveIndexEntry(Call, 16, 4),
            });

            AssertLocation(index, Call, 15, 0, 0);
            AssertLocation(index, Call, 16, 0, 4);
        }

        [TestMethod]
        public void TryLocate_DayOrSymbolNotListed_IsNotCached()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, 15, 3) });

            Assert.IsFalse(index.TryLocate(Call, 16, out _));
            Assert.IsFalse(index.TryLocate(Put, 15, out _));
            Assert.IsFalse(index.TryLocate(null, 15, out _));
        }

        [TestMethod]
        public void GetDays_DaysListedOutOfOrder_AreInOrder()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Call, 16, 0),
                new ArchiveIndexEntry(Call, 3, 9),
            });

            CollectionAssert.AreEqual(new[] { 3, 16 }, index.GetDays(Call).ToArray());
            Assert.AreEqual(0, index.GetDays(Put).Count);
            Assert.AreEqual(0, index.GetDays(null).Count);
        }

        [TestMethod]
        public void Constructor_SameDayTwice_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Call, 15, 3),
                new ArchiveIndexEntry(Call, 15, 3),
            }));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(32)]
        public void Constructor_DayOutsideTheMonth_Throws(int day)
        {
            Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, day, 3) }));
        }

        [TestMethod]
        public void Constructor_DayPastTheEndOfAShortMonth_Throws()
        {
            ArchiveFileIdentity february = new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 2, "UTC", Identity.BuiltUtc);

            Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(february, new[] { new ArchiveIndexEntry(Call, 29, 3) }));
        }

        [TestMethod]
        public void Constructor_NegativeRecordCount_Throws()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, 15, -1) }));

            Assert.AreEqual("entries", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_EntryWithoutASymbol_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(string.Empty, 15, 3) }));
            Assert.ThrowsException<ArgumentException>(() => new ArchiveIndex(Identity, new ArchiveIndexEntry[] { null }));
        }

        [TestMethod]
        public void Entries_OfAnIndex_AreNoArrayToCastAndChange()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, 15, 3) });

            Assert.IsNotInstanceOfType(index.Entries, typeof(ArchiveIndexEntry[]));
        }

        [TestMethod]
        public void FromJson_RoundTrip_KeepsIdentityAndEveryEntry()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Put, 16, 5),
                new ArchiveIndexEntry(Call, 15, 0),
                new ArchiveIndexEntry(Call, 16, 7),
            });

            ArchiveIndex copy = ArchiveIndex.FromJson(index.ToJson());

            Assert.AreEqual(Identity.FileName, copy.Identity.FileName);
            Assert.AreEqual("OPRA.PILLAR", copy.Identity.Dataset);
            Assert.AreEqual("cbbo-1m", copy.Identity.Schema);
            Assert.AreEqual("SPXW.OPT", copy.Identity.Group);
            Assert.AreEqual("UTC", copy.Identity.Cutoff);
            Assert.AreEqual(new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc), copy.Identity.BuiltUtc);
            Assert.AreEqual(DateTimeKind.Utc, copy.Identity.BuiltUtc.Kind);
            CollectionAssert.AreEqual(index.Entries.ToArray(), copy.Entries.ToArray());
            AssertLocation(copy, Put, 16, 7, 5);
        }

        [TestMethod]
        public void FromJson_DeltaRoundTrip_KeepsItsKindAndGeneration()
        {
            ArchiveFileIdentity delta = new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC", Identity.BuiltUtc, 3);
            ArchiveIndex index = new ArchiveIndex(delta, new[] { new ArchiveIndexEntry(Call, 15, 3) });

            string json = index.ToJson();
            ArchiveIndex copy = ArchiveIndex.FromJson(json);

            StringAssert.Contains(json, "\"kind\":\"delta\",\"generation\":3");
            Assert.AreEqual(ArchiveFileKind.Delta, copy.Identity.Kind);
            Assert.AreEqual(3, copy.Identity.Generation);
            Assert.AreEqual("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.delta.3.parquet", copy.Identity.FileName);
        }

        [TestMethod]
        [DataRow("\"kind\":\"full\",\"generation\":2")]
        [DataRow("\"kind\":\"delta\",\"generation\":0")]
        [DataRow("\"kind\":\"main\",\"generation\":0")]
        [DataRow("\"kind\":\"Full\",\"generation\":0")]
        [DataRow("\"generation\":0")]
        public void FromJson_KindThatDoesNotMatchTheGeneration_IsRefused(string kindAndGeneration)
        {
            // a full file is generation 0 and a delta 1 and up, so an index saying otherwise does not describe a file of ours
            string json = new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, 15, 3) }).ToJson().Replace("\"kind\":\"full\",\"generation\":0", kindAndGeneration);

            Assert.ThrowsException<InvalidDataException>(() => ArchiveIndex.FromJson(json));
        }

        [TestMethod]
        public void ToJson_ListsTheIdentityThenEachSymbolsDaysWithFirstRowAndCount()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Put, 16, 5),
                new ArchiveIndexEntry(Call, 16, 7),
                new ArchiveIndexEntry(Call, 15, 0),
            });

            Assert.AreEqual(
                Header + ",\"kind\":\"full\",\"generation\":0,\"symbols\":{\"SPXW  261016C06600000\":[[15,0,0],[16,0,7]],\"SPXW  261016P06600000\":[[16,7,5]]}}",
                index.ToJson());
        }

        [TestMethod]
        public void ToJson_NoDays_ListsNoSymbols()
        {
            ArchiveIndex index = new ArchiveIndex(Identity, Array.Empty<ArchiveIndexEntry>());

            Assert.AreEqual(Header + ",\"kind\":\"full\",\"generation\":0,\"symbols\":{}}", index.ToJson());
            Assert.AreEqual(0, ArchiveIndex.FromJson(index.ToJson()).Entries.Count);
        }

        [TestMethod]
        public void FromJson_FirstRowThatDoesNotFollowTheLayout_IsRefused()
        {
            // the first rows are stored for a direct lookup, and checked on load: a damaged index must not serve another day's rows
            string json = new ArchiveIndex(Identity, new[]
            {
                new ArchiveIndexEntry(Call, 15, 3),
                new ArchiveIndexEntry(Call, 16, 4),
            }).ToJson().Replace("[16,3,4]", "[16,2,4]");

            Assert.ThrowsException<InvalidDataException>(() => ArchiveIndex.FromJson(json));
        }

        [TestMethod]
        public void FromJson_UnknownFormatVersion_IsRefused()
        {
            string json = new ArchiveIndex(Identity, new[] { new ArchiveIndexEntry(Call, 15, 3) }).ToJson().Replace("\"format_version\":1", "\"format_version\":2");

            Assert.ThrowsException<InvalidDataException>(() => ArchiveIndex.FromJson(json));
        }

        [TestMethod]
        [DataRow("", DisplayName = "empty")]
        [DataRow("not json", DisplayName = "not JSON")]
        [DataRow("{\"format_version\":1}", DisplayName = "the format version alone")]
        [DataRow(Header + ",\"symbols\":{\"SPXW  261016C06600000\":[[15,0,3]]}}", DisplayName = "no kind or generation")]
        [DataRow(Header + ",\"kind\":\"full\",\"generation\":0,\"symbols\":{\"SPXW  261016C06600000\":[[15,0]]}}", DisplayName = "a day of two numbers")]
        [DataRow(Header + ",\"kind\":\"delta\",\"generation\":-1,\"symbols\":{\"SPXW  261016C06600000\":[[15,0,3]]}}", DisplayName = "a negative generation")]
        [DataRow(Header + ",\"kind\":\"full\",\"generation\":0,\"symbols\":{\"SPXW  261016C06600000\":[[15,0,3],[15,3,3]]}}", DisplayName = "a day twice")]
        [DataRow(Header + ",\"kind\":\"full\",\"generation\":0,\"symbols\":{\"SPXW  261016C06600000\":[[32,0,3]]}}", DisplayName = "a day past the month")]
        [DataRow("{\"format_version\":1,\"schema\":\"cbbo-1m\",\"group\":\"SPXW.OPT\",\"month\":\"2026-10\",\"cutoff\":\"UTC\",\"built_utc\":\"2026-11-18T02:15:00Z\",\"kind\":\"full\",\"generation\":0,\"symbols\":{}}", DisplayName = "no dataset")]
        [DataRow("{\"format_version\":1,\"dataset\":\"OPRA.PILLAR\",\"schema\":\"cbbo-1m\",\"group\":\"SPXW.OPT\",\"month\":\"2026-13\",\"cutoff\":\"UTC\",\"built_utc\":\"2026-11-18T02:15:00Z\",\"kind\":\"full\",\"generation\":0,\"symbols\":{}}", DisplayName = "a month that is none")]
        [DataRow("{\"format_version\":1,\"dataset\":\"OPRA.PILLAR\",\"schema\":\"cbbo-1m\",\"group\":\"SPXW.OPT\",\"month\":\"2026-10\",\"cutoff\":\"U_TC\",\"built_utc\":\"2026-11-18T02:15:00Z\",\"kind\":\"full\",\"generation\":0,\"symbols\":{}}", DisplayName = "a cutoff that cannot sit in a name")]
        public void FromJson_MalformedIndex_IsRefused(string json)
        {
            Assert.ThrowsException<InvalidDataException>(() => ArchiveIndex.FromJson(json));
        }

        private static void AssertLocation(ArchiveIndex index, string symbol, int day, long expectedFirstRow, int expectedRecordCount)
        {
            Assert.IsTrue(index.TryLocate(symbol, day, out ArchiveDayLocation location), Invariant($"{symbol} day {day} should be listed"));
            Assert.AreEqual(expectedFirstRow, location.FirstRow, Invariant($"{symbol} day {day} first row"));
            Assert.AreEqual(expectedRecordCount, location.RecordCount, Invariant($"{symbol} day {day} record count"));
        }
    }
}
