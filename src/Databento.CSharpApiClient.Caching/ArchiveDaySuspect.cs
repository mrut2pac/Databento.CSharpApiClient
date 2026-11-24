using System;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// A day-file day an <see cref="IArchiveDayCheck"/> finds suspect, and why
    /// </summary>
    public sealed class ArchiveDaySuspect
    {
        /// <summary>
        /// Creates a suspect day
        /// </summary>
        /// <param name="day">The day, one of <see cref="ArchiveGroupDays.DayFileDays"/></param>
        /// <param name="reason">Why it is suspect, a short code the check defines, e.g. <c>below-peers</c></param>
        /// <param name="expectedRecords">The record count the day was judged against; 0 when the check counts none</param>
        /// <param name="detail">What the check found, for the report; <c>null</c> when the reason says it all</param>
        public ArchiveDaySuspect(ArchiveDay day, string reason, double expectedRecords, string detail)
        {
            ArgumentNullException.ThrowIfNull(day);
            ArgumentException.ThrowIfNullOrEmpty(reason);

            this.Day = day;
            this.Reason = reason;
            this.ExpectedRecords = expectedRecords;
            this.Detail = detail;
        }

        /// <summary>
        /// Gets the day: its raw symbol, date and record count.
        /// </summary>
        public ArchiveDay Day { get; }

        /// <summary>
        /// Gets why it is suspect, a short code the check defines.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the record count the day was judged against, e.g. its peers' median; 0 when the check counts none.
        /// </summary>
        public double ExpectedRecords { get; }

        /// <summary>
        /// Gets what the check found, for the report; <c>null</c> when the reason says it all.
        /// </summary>
        public string Detail { get; }
    }
}
