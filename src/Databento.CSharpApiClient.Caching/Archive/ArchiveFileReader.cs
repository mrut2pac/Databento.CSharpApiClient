using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;

using Parquet;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Reads symbol-days from an open archive file (see <see cref="ArchiveFile.OpenAsync{TSource, TRecord}(string, ArchiveRecordSchema{TSource, TRecord})"/>).
    /// A read checks every row it returns belongs to the symbol-day asked for. Reads take turns on the one file handle, so
    /// concurrent callers can share a reader; the last row group decoded is kept, so reading days in index order decodes each once.
    /// </summary>
    /// <typeparam name="TSource">Record interface the file was written from</typeparam>
    /// <typeparam name="TRecord">Record type a read returns</typeparam>
    internal sealed class ArchiveFileReader<TSource, TRecord> : IAsyncDisposable
        where TSource : class, IRecord
        where TRecord : class, TSource
    {
        private readonly ParquetReader reader;

        private readonly ArchiveRecordSchema<TSource, TRecord> schema;

        /// <summary>
        /// File row each row group starts at
        /// </summary>
        private readonly long[] rowGroupStarts;

        /// <summary>
        /// Lets one read at a time use the file handle and <see cref="decoded"/>; disposing takes it too, so it waits for a read in progress
        /// </summary>
        private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The row group the last read decoded, or <c>null</c> before the first read
        /// </summary>
        private DecodedRowGroup decoded;

        private bool disposed;

        internal ArchiveFileReader(ParquetReader reader, ArchiveRecordSchema<TSource, TRecord> schema, ArchiveIndex index, long[] rowGroupStarts)
        {
            this.reader = reader;
            this.schema = schema;
            this.Index = index;
            this.rowGroupStarts = rowGroupStarts;
        }

        /// <summary>
        /// Gets the index of the file.
        /// </summary>
        public ArchiveIndex Index { get; }

        /// <summary>
        /// Gets how many times a read decoded a row group, so a test can tell a row group is decoded once for the days that share it.
        /// </summary>
        internal int RowGroupsDecoded { get; private set; }

        /// <summary>
        /// Reads one symbol-day
        /// </summary>
        /// <param name="symbol">Databento raw symbol</param>
        /// <param name="date">UTC day, in the file's month</param>
        /// <returns>The day's records, each carrying <paramref name="symbol"/>; empty for a day cached as empty, or <c>null</c> when the file does not hold the day</returns>
        /// <exception cref="InvalidDataException">The rows the index points at belong to another symbol-day</exception>
        public async Task<TRecord[]> ReadSymbolDayAsync(string symbol, DateTime date)
        {
            ArchiveFileIdentity identity = this.Index.Identity;
            if(date.Year != identity.Year || date.Month != identity.Month)
            {
                throw new ArgumentException(Invariant($"{date:yyyy-MM-dd} is not in {identity.Year}-{identity.Month:00}, the month of this file."), nameof(date));
            }

            if(!this.Index.TryLocate(symbol, date.Day, out ArchiveDayLocation location))
            {
                return null;
            }

            TRecord[] records = new TRecord[location.RecordCount];
            if(records.Length == 0)
            {
                return records;
            }

            long dayEnd = location.FirstRow + location.RecordCount;
            DateOnly day = DateOnly.FromDateTime(date);
            int filled = 0;

            // the row group the day starts in: the last one starting at or before its first row
            int rowGroup = Array.BinarySearch(this.rowGroupStarts, location.FirstRow);
            if(rowGroup < 0)
            {
                rowGroup = ~rowGroup - 1;
            }

            await this.readLock.WaitAsync().ConfigureAwait(false);
            try
            {
                ObjectDisposedException.ThrowIf(this.disposed, this);

                for(; rowGroup < this.rowGroupStarts.Length && filled < records.Length; ++rowGroup)
                {
                    // the file's row count is the index's, checked when the file was opened
                    long groupStart = this.rowGroupStarts[rowGroup];
                    long groupEnd = rowGroup + 1 < this.rowGroupStarts.Length ? this.rowGroupStarts[rowGroup + 1] : this.Index.RowCount;
                    if(groupEnd <= location.FirstRow)
                    {
                        // an empty row group starting at the same row as the next one
                        continue;
                    }

                    int firstRow = (int)(Math.Max(location.FirstRow, groupStart) - groupStart);
                    int count = (int)(Math.Min(dayEnd, groupEnd) - groupStart) - firstRow;
                    try
                    {
                        DecodedRowGroup decodedRowGroup = await this.DecodeAsync(rowGroup).ConfigureAwait(false);
                        this.FillRows(decodedRowGroup, firstRow, records, filled, count, symbol, day);
                    }
                    catch(Exception ex) when(ex is IndexOutOfRangeException || ex is OverflowException || ex is FormatException)
                    {
                        // what Parquet.Net's decoders throw on a damaged page; a fault of this code is left to reach the caller
                        throw new InvalidDataException(Invariant($"Row group {rowGroup} cannot be read: {ex.Message}"), ex);
                    }

                    filled += count;
                }
            }
            finally
            {
                this.readLock.Release();
            }

            return records;
        }

        public async ValueTask DisposeAsync()
        {
            // the semaphore is left undisposed: a read still waiting on it then gets ObjectDisposedException from the check above
            await this.readLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if(!this.disposed)
                {
                    this.disposed = true;
                    this.decoded = null;
                    await this.reader.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                this.readLock.Release();
            }
        }

        /// <summary>
        /// Decodes every column of a row group, unless it is the row group the last read decoded
        /// </summary>
        /// <param name="rowGroup">Row group's number</param>
        /// <returns>The row group's raw symbols, days and column values</returns>
        private async Task<DecodedRowGroup> DecodeAsync(int rowGroup)
        {
            if(this.decoded != null && this.decoded.RowGroup == rowGroup)
            {
                return this.decoded;
            }

            // dropped first, so a decode that fails leaves no row group to be taken for another
            this.decoded = null;
            ++this.RowGroupsDecoded;
            using(ParquetRowGroupReader rowGroupReader = this.reader.OpenRowGroupReader(rowGroup))
            {
                string[] rawSymbols = new string[rowGroupReader.RowCount];
                await rowGroupReader.ReadAsync(this.schema.RawSymbolField, rawSymbols).ConfigureAwait(false);
                DateOnly[] days = new DateOnly[rowGroupReader.RowCount];
                await rowGroupReader.ReadAsync<DateOnly>(this.schema.DateField, days).ConfigureAwait(false);
                object[] columnValues = new object[this.schema.Columns.Count];
                for(int i = 0; i < columnValues.Length; ++i)
                {
                    columnValues[i] = await this.schema.Columns[i].ReadValuesAsync(rowGroupReader).ConfigureAwait(false);
                }

                this.decoded = new DecodedRowGroup(rowGroup, rawSymbols, days, columnValues);
                return this.decoded;
            }
        }

        /// <summary>
        /// Creates the records of a run of rows of one symbol-day from a decoded row group
        /// </summary>
        /// <param name="rowGroup">Row group decoded</param>
        /// <param name="firstRow">First row of the run, within the row group</param>
        /// <param name="target">Records of the day being read</param>
        /// <param name="targetOffset">Index in <paramref name="target"/> the run's first row goes to</param>
        /// <param name="count">Number of rows in the run</param>
        /// <param name="rawSymbol">Raw symbol the rows must belong to</param>
        /// <param name="day">UTC day the rows must belong to</param>
        /// <exception cref="InvalidDataException">A row belongs to another symbol-day: the index does not match the rows</exception>
        private void FillRows(DecodedRowGroup rowGroup, int firstRow, TRecord[] target, int targetOffset, int count, string rawSymbol, DateOnly day)
        {
            for(int i = firstRow; i < firstRow + count; ++i)
            {
                if(!string.Equals(rowGroup.RawSymbols[i], rawSymbol, StringComparison.Ordinal) || rowGroup.Days[i] != day)
                {
                    throw new InvalidDataException(Invariant($"The index puts {rawSymbol} {day:yyyy-MM-dd} on a row that holds {rowGroup.RawSymbols[i]} {rowGroup.Days[i]:yyyy-MM-dd}."));
                }

                target[targetOffset + i - firstRow] = this.schema.CreateRecord(rawSymbol);
            }

            IReadOnlyList<ArchiveColumn<TSource, TRecord>> columns = this.schema.Columns;
            for(int i = 0; i < columns.Count; ++i)
            {
                columns[i].Fill(rowGroup.ColumnValues[i], firstRow, target, targetOffset, count);
            }
        }

        /// <summary>
        /// A row group's rows as decoded: each row's raw symbol and day, and each record column's values in the schema's column order
        /// </summary>
        private sealed record DecodedRowGroup(int RowGroup, string[] RawSymbols, DateOnly[] Days, object[] ColumnValues);
    }
}
