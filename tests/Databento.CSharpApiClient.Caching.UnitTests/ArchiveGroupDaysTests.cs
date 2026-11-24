using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// The days a check is shown, built as a check's own unit test builds them
    /// </summary>
    [TestClass]
    public class ArchiveGroupDaysTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private static readonly ArchiveDay CallDay = new ArchiveDay(Call, new DateOnly(2026, 10, 16), 60);

        private static readonly ArchiveDay PutDay = new ArchiveDay(Put, new DateOnly(2026, 10, 16), 2);

        [TestMethod]
        public void Constructor_Days_AreWhatACheckIsShown()
        {
            Dictionary<(string Symbol, DateOnly Date), object> summaries = new Dictionary<(string Symbol, DateOnly Date), object>
            {
                [(Put, new DateOnly(2026, 10, 16))] = "2 quotes",
            };

            ArchiveGroupDays days = new ArchiveGroupDays("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, new[] { CallDay, PutDay }, new[] { PutDay }, summaries);

            Assert.AreEqual("OPRA.PILLAR", days.Dataset);
            Assert.AreEqual("cbbo-1m", days.Schema);
            Assert.AreEqual("SPXW.OPT", days.Group);
            Assert.AreEqual(2026, days.Year);
            Assert.AreEqual(10, days.Month);
            CollectionAssert.AreEqual(new[] { CallDay, PutDay }, days.ServedDays.ToArray());
            CollectionAssert.AreEqual(new[] { PutDay }, days.DayFileDays.ToArray());
            Assert.AreEqual("2 quotes", days.SummaryOf(PutDay));
            Assert.IsNull(days.SummaryOf(CallDay));
        }

        [TestMethod]
        public void Constructor_CollectionsChangedAfterwards_DoNotChangeTheDays()
        {
            ArchiveDay[] servedDays = { CallDay, PutDay };
            List<ArchiveDay> dayFileDays = new List<ArchiveDay> { PutDay };
            Dictionary<(string Symbol, DateOnly Date), object> summaries = new Dictionary<(string Symbol, DateOnly Date), object>();
            ArchiveGroupDays days = new ArchiveGroupDays("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, servedDays, dayFileDays, summaries);

            servedDays[0] = PutDay;
            dayFileDays.Add(CallDay);
            summaries[(Put, new DateOnly(2026, 10, 16))] = "added";

            CollectionAssert.AreEqual(new[] { CallDay, PutDay }, days.ServedDays.ToArray());
            CollectionAssert.AreEqual(new[] { PutDay }, days.DayFileDays.ToArray());
            Assert.IsNull(days.SummaryOf(PutDay));
            Assert.ThrowsException<NotSupportedException>(() => ((IList<ArchiveDay>)days.ServedDays)[0] = PutDay, "the lists cannot be changed through the days either");
        }

        [TestMethod]
        [DataRow("no dataset")]
        [DataRow("an empty schema")]
        [DataRow("no group")]
        [DataRow("year 0")]
        [DataRow("month 0")]
        [DataRow("month 13")]
        [DataRow("no served days")]
        [DataRow("a missing served day")]
        [DataRow("no day-file days")]
        [DataRow("a missing day-file day")]
        [DataRow("no summaries")]
        public void Constructor_ArgumentMissingOrOutOfRange_Throws(string argument)
        {
            string dataset = argument == "no dataset" ? null : "OPRA.PILLAR";
            string schema = argument == "an empty schema" ? string.Empty : "cbbo-1m";
            string group = argument == "no group" ? null : "SPXW.OPT";
            int year = argument == "year 0" ? 0 : 2026;
            int month = argument switch
            {
                "month 0" => 0,
                "month 13" => 13,
                _ => 10,
            };
            ArchiveDay[] servedDays = argument switch
            {
                "no served days" => null,
                "a missing served day" => new[] { CallDay, null },
                _ => new[] { CallDay, PutDay },
            };
            ArchiveDay[] dayFileDays = argument switch
            {
                "no day-file days" => null,
                "a missing day-file day" => new ArchiveDay[] { null },
                _ => new[] { PutDay },
            };
            Dictionary<(string Symbol, DateOnly Date), object> summaries = argument == "no summaries" ? null : new Dictionary<(string Symbol, DateOnly Date), object>();

            ArgumentException exception = null;
            try
            {
                _ = new ArchiveGroupDays(dataset, schema, group, year, month, servedDays, dayFileDays, summaries);
            }
            catch(ArgumentException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception, "an ArgumentException or one derived from it");
        }
    }
}
