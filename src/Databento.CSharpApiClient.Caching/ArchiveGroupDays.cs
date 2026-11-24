using System;
using System.Collections.Generic;
using System.Linq;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// The days of one group-month a compaction run is about to publish, as an <see cref="IArchiveDayCheck"/> sees them: each with its
    /// record count, which come from day files, and the check's own summaries of the days the new file holds
    /// </summary>
    public sealed class ArchiveGroupDays
    {
        private readonly Dictionary<(string Symbol, DateOnly Date), object> summaries;

        /// <summary>
        /// Creates the days of a group-month as the compactor shows them to a check, so a check's own tests can build them. The days and
        /// summaries are copied.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c></param>
        /// <param name="schema">Databento schema, e.g. <c>cbbo-1m</c></param>
        /// <param name="group">Group, e.g. <c>SPXW.OPT</c></param>
        /// <param name="year">Year of the month, 1 to 9999</param>
        /// <param name="month">Month, 1 to 12</param>
        /// <param name="servedDays">Every day the group-month serves once the file is published</param>
        /// <param name="dayFileDays">The days taken from day files, the ones a check judges</param>
        /// <param name="summaries">The check's summaries of the days the new file holds, by raw symbol and date</param>
        /// <exception cref="ArgumentNullException">A name, a list of days or the summaries is <c>null</c></exception>
        /// <exception cref="ArgumentException">A name is empty, or a list holds a <c>null</c> day</exception>
        /// <exception cref="ArgumentOutOfRangeException">The year or the month is out of range</exception>
        public ArchiveGroupDays(string dataset, string schema, string group, int year, int month, IReadOnlyList<ArchiveDay> servedDays, IReadOnlyList<ArchiveDay> dayFileDays, IReadOnlyDictionary<(string Symbol, DateOnly Date), object> summaries)
        {
            ArgumentException.ThrowIfNullOrEmpty(dataset);
            ArgumentException.ThrowIfNullOrEmpty(schema);
            ArgumentException.ThrowIfNullOrEmpty(group);
            ArgumentOutOfRangeException.ThrowIfLessThan(year, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
            ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
            ArgumentNullException.ThrowIfNull(summaries);

            this.Dataset = dataset;
            this.Schema = schema;
            this.Group = group;
            this.Year = year;
            this.Month = month;
            this.ServedDays = CopyDays(servedDays, nameof(servedDays));
            this.DayFileDays = CopyDays(dayFileDays, nameof(dayFileDays));
            this.summaries = new Dictionary<(string Symbol, DateOnly Date), object>(summaries);
        }

        /// <summary>
        /// Gets the Databento dataset, e.g. <c>OPRA.PILLAR</c>.
        /// </summary>
        public string Dataset { get; }

        /// <summary>
        /// Gets the Databento schema, e.g. <c>cbbo-1m</c>.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the group, e.g. <c>SPXW.OPT</c>: an OPRA option's or a CME future's parent symbol, else the symbol itself.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the year of the month.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the month, 1 to 12.
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// Gets every day the group-month's archive serves once the run publishes: the days already archived and the day-file days.
        /// </summary>
        public IReadOnlyList<ArchiveDay> ServedDays { get; }

        /// <summary>
        /// Gets the days the run takes from day files, the ones a check judges; each is in <see cref="ServedDays"/> too.
        /// </summary>
        public IReadOnlyList<ArchiveDay> DayFileDays { get; }

        /// <summary>
        /// Gets what the check's <see cref="IArchiveDayCheck.Summarize"/> kept of a day
        /// </summary>
        /// <param name="day">The day</param>
        /// <returns>The summary, or <c>null</c> when the check kept none or the new file does not hold the day: a delta holds the day
        /// files and the previous delta's days, not its full file's</returns>
        public object SummaryOf(ArchiveDay day)
        {
            ArgumentNullException.ThrowIfNull(day);

            return this.summaries.TryGetValue((day.Symbol, day.Date), out object summary) ? summary : null;
        }

        private static IReadOnlyList<ArchiveDay> CopyDays(IReadOnlyList<ArchiveDay> days, string paramName)
        {
            ArgumentNullException.ThrowIfNull(days, paramName);
            if(days.Any(day => day == null))
            {
                throw new ArgumentException("A day is missing.", paramName);
            }

            return Array.AsReadOnly(days.ToArray());
        }
    }
}
