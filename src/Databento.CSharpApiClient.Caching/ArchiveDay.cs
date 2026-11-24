using System;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// One symbol's UTC day in the archive, and how many records it holds
    /// </summary>
    public sealed class ArchiveDay
    {
        /// <summary>
        /// Creates a day
        /// </summary>
        /// <param name="symbol">Databento raw symbol</param>
        /// <param name="date">UTC day</param>
        /// <param name="recordCount">Number of records; 0 for a day cached as empty and final</param>
        public ArchiveDay(string symbol, DateOnly date, int recordCount)
        {
            ArgumentException.ThrowIfNullOrEmpty(symbol);
            ArgumentOutOfRangeException.ThrowIfNegative(recordCount);

            this.Symbol = symbol;
            this.Date = date;
            this.RecordCount = recordCount;
        }

        /// <summary>
        /// Gets the Databento raw symbol, in the dataset's own symbology, e.g. <c>SPXW  261016C06600000</c>.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the UTC day.
        /// </summary>
        public DateOnly Date { get; }

        /// <summary>
        /// Gets the number of records of the day; 0 means the day is cached as empty and final.
        /// </summary>
        public int RecordCount { get; }
    }
}
