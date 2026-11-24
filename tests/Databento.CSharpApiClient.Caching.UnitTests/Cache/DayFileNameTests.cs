using System;
using System.IO;

using Databento.CSharpApiClient.Caching.Cache;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Cache
{
    [TestClass]
    public class DayFileNameTests
    {
        [TestMethod]
        [DataRow("SPXW  261016C06600000", "SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz", DisplayName = "an OSI symbol")]
        [DataRow("MNQZ6", "MNQZ6_2026-10-16.UTC.json.gz", DisplayName = "a contract month")]
        [DataRow("A_B", "A%5FB_2026-10-16.UTC.json.gz", DisplayName = "a symbol holding the name's separator")]
        public void Format_Symbol_NamesTheEscapedSymbolDayAndCutoff(string symbol, string expectedFileName)
        {
            Assert.AreEqual(expectedFileName, DayFileName.Format(symbol, new DateTime(2026, 10, 16), "UTC"));
        }

        [TestMethod]
        public void Format_DayOfASingleDigitMonth_IsPadded()
        {
            Assert.AreEqual("QQQ_2026-03-05.UTC.json.gz", DayFileName.Format("QQQ", new DateTime(2026, 3, 5, 23, 59, 59), "UTC"));
        }

        [TestMethod]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz", "SPXW  261016C06600000", DisplayName = "an OSI symbol")]
        [DataRow("A%5FB_2026-10-16.UTC.json.gz", "A_B", DisplayName = "a symbol holding the name's separator")]
        public void TryParse_DayFileName_GivesItsSymbolAndDay(string fileName, string expectedSymbol)
        {
            Assert.IsTrue(DayFileName.TryParse(fileName, "UTC", out string symbol, out DateTime date));

            Assert.AreEqual(expectedSymbol, symbol);
            Assert.AreEqual(new DateTime(2026, 10, 16), date);
        }

        [TestMethod]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.ET.json.gz", DisplayName = "another cutoff")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.EST.json.gz", DisplayName = "another cutoff as long as UTC")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.json.gz", DisplayName = "no cutoff")]
        [DataRow("SPXW%20%20261016C06600000_2026-02-30.UTC.json.gz", DisplayName = "a date that is none")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-6.UTC.json.gz", DisplayName = "a day not written as two digits")]
        [DataRow("SPXW%20%20261016C06600000_20261016.UTC.json.gz", DisplayName = "a date of another format")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz.0123456789abcdef0123456789abcdef.tmp", DisplayName = "a day file's temporary file")]
        [DataRow("SPXW%20%20261016C06600000_2026-10-16.UTC.json", DisplayName = "not compressed")]
        [DataRow("_2026-10-16.UTC.json.gz", DisplayName = "no symbol")]
        [DataRow("2026-10-16.UTC.json.gz", DisplayName = "no separator")]
        [DataRow("SPXW  261016C06600000_2026-10-16.UTC.json.gz", DisplayName = "a symbol not escaped")]
        [DataRow("%41_2026-10-16.UTC.json.gz", DisplayName = "a symbol escaped where it needs no escape")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "an archive file")]
        public void TryParse_NameThatIsNoDayFileOfTheCutoff_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(DayFileName.TryParse(fileName, "UTC", out string symbol, out DateTime date));
            Assert.IsNull(symbol);
            Assert.AreEqual(default(DateTime), date);
        }

        [TestMethod]
        public void TryParse_NoNameOrCutoff_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => DayFileName.TryParse(null, "UTC", out _, out _));
            Assert.ThrowsException<ArgumentException>(() => DayFileName.TryParse("QQQ_2026-10-16.UTC.json.gz", string.Empty, out _, out _));
            Assert.ThrowsException<ArgumentException>(() => DayFileName.Format("QQQ", new DateTime(2026, 10, 16), string.Empty));
        }

        [TestMethod]
        public void GetMonthDirectory_EarlyMonth_IsTheDatasetSchemaYearAndPaddedMonthUnderTheRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "cache");

            Assert.AreEqual(Path.Combine(root, "OPRA.PILLAR", "cbbo-1m", "2026", "07"), DayFileName.GetMonthDirectory(root, "OPRA.PILLAR", "cbbo-1m", 2026, 7));
        }
    }
}
