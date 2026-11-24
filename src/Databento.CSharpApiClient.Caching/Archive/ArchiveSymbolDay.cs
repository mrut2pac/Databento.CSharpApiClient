using System;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// One symbol-day handed to <see cref="ArchiveFile"/>: what a day file holds. A record that names its symbol must name this one;
    /// one that names none, as a record without <c>map_symbols</c> does, belongs to it too.
    /// </summary>
    /// <typeparam name="TSource">Record interface</typeparam>
    internal sealed class ArchiveSymbolDay<TSource>
        where TSource : class, IRecord
    {
        public ArchiveSymbolDay(string symbol, DateTime date, TSource[] records)
        {
            ArgumentException.ThrowIfNullOrEmpty(symbol);
            ArgumentNullException.ThrowIfNull(records);
            foreach(TSource record in records)
            {
                if(record == null)
                {
                    throw new ArgumentException(Invariant($"{symbol} {date:yyyy-MM-dd} has a missing record."), nameof(records));
                }

                if(record.Symbol != null && !string.Equals(record.Symbol, symbol, StringComparison.Ordinal))
                {
                    throw new ArgumentException(Invariant($"{symbol} {date:yyyy-MM-dd} has a record of {record.Symbol}."), nameof(records));
                }
            }

            this.Symbol = symbol;
            this.Date = date.Date;
            this.Records = records;
        }

        /// <summary>
        /// Gets the Databento raw symbol, in the dataset's own symbology, e.g. <c>SPXW  261016C06600000</c>.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the UTC day.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets the day's records in the order Databento sent them; empty for a day cached as empty and final.
        /// </summary>
        public TSource[] Records { get; }
    }
}
