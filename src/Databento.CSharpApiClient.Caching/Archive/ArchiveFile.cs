using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;

using Parquet;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Writes and opens archive files. A file holds the symbol-days of one dataset, schema, group and month as Parquet rows in symbol
    /// then day order, with the <see cref="ArchiveIndex"/> in the file's metadata.
    /// </summary>
    internal static class ArchiveFile
    {
        /// <summary>
        /// Rows per row group. A read of one symbol-day decodes the row groups its rows sit in, unless the read before it decoded them,
        /// so this bounds that work.
        /// </summary>
        public const int DefaultRowGroupSize = 4096;

        /// <summary>
        /// Zstd at its default level.
        /// </summary>
        private static readonly ParquetOptions WriterOptions = new ParquetOptions
        {
            CompressionMethod = CompressionMethod.Zstd,
            CompressionLevel = CompressionLevel.Optimal,
        };

        private static readonly ParquetOptions ReaderOptions = new ParquetOptions
        {
            UseDateOnlyTypeForDates = true,
        };

        /// <summary>
        /// Writes the symbol-days as an archive file
        /// </summary>
        /// <typeparam name="TSource">Record interface, which the JSON and DBN records both implement</typeparam>
        /// <typeparam name="TRecord">Record type a read returns</typeparam>
        /// <param name="stream">Writable, seekable stream; left open</param>
        /// <param name="schema">Archive schema of the Databento schema</param>
        /// <param name="identity">Version being written</param>
        /// <param name="symbolDays">Symbol-days, any order; each of the identity's group and month, none twice</param>
        /// <param name="rowGroupSize">Rows per row group</param>
        /// <returns>The index written into the file</returns>
        public static async Task<ArchiveIndex> WriteAsync<TSource, TRecord>(Stream stream, ArchiveRecordSchema<TSource, TRecord> schema, ArchiveFileIdentity identity, IEnumerable<ArchiveSymbolDay<TSource>> symbolDays, int rowGroupSize = DefaultRowGroupSize)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentNullException.ThrowIfNull(symbolDays);
            ThrowIfBadRowGroupSize(rowGroupSize);

            // every symbol-day is checked before a byte is written
            Dictionary<(string Symbol, int Day), ArchiveSymbolDay<TSource>> symbolDaysByDay = new Dictionary<(string Symbol, int Day), ArchiveSymbolDay<TSource>>();
            foreach(ArchiveSymbolDay<TSource> symbolDay in symbolDays)
            {
                Validate(schema, identity, symbolDay, nameof(symbolDays));
                if(!symbolDaysByDay.TryAdd((symbolDay.Symbol, symbolDay.Date.Day), symbolDay))
                {
                    throw new ArgumentException(Invariant($"{symbolDay.Symbol} {symbolDay.Date:yyyy-MM-dd} is given twice."), nameof(symbolDays));
                }
            }

            IEnumerable<ArchiveSymbolDay<TSource>> inIndexOrder = symbolDaysByDay.Values
                .OrderBy(symbolDay => symbolDay.Symbol, StringComparer.Ordinal)
                .ThenBy(symbolDay => symbolDay.Date);

            return await WriteInOrderAsync(stream, schema, identity, ToAsyncEnumerable(inIndexOrder), rowGroupSize).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes symbol-days that arrive in index order, raw symbol (ordinal) then day, holding one symbol-day and one row group in
        /// memory at a time, so a month too large to load at once can be written. A symbol-day out of order fails the write.
        /// </summary>
        /// <typeparam name="TSource">Record interface, which the JSON and DBN records both implement</typeparam>
        /// <typeparam name="TRecord">Record type a read returns</typeparam>
        /// <param name="stream">Writable, seekable stream; left open, holding a partial file if the write fails</param>
        /// <param name="schema">Archive schema of the Databento schema</param>
        /// <param name="identity">Version being written</param>
        /// <param name="symbolDays">Symbol-days in index order; each of the identity's group and month</param>
        /// <param name="rowGroupSize">Rows per row group</param>
        /// <returns>The index written into the file</returns>
        public static async Task<ArchiveIndex> WriteInOrderAsync<TSource, TRecord>(Stream stream, ArchiveRecordSchema<TSource, TRecord> schema, ArchiveFileIdentity identity, IAsyncEnumerable<ArchiveSymbolDay<TSource>> symbolDays, int rowGroupSize = DefaultRowGroupSize)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentNullException.ThrowIfNull(symbolDays);
            ThrowIfBadRowGroupSize(rowGroupSize);

            List<ArchiveIndexEntry> entries = new List<ArchiveIndexEntry>();
            ArchiveIndex index;
            ParquetWriter writer = await ParquetWriter.CreateAsync(schema.ParquetSchema, stream, WriterOptions, false).ConfigureAwait(false);
            await using(writer.ConfigureAwait(false))
            {
                // rows go in the index's order, which is what makes its first rows true
                RowGroupBuffer<TSource> buffer = new RowGroupBuffer<TSource>(rowGroupSize);
                ArchiveSymbolDay<TSource> previous = null;
                await foreach(ArchiveSymbolDay<TSource> symbolDay in symbolDays.ConfigureAwait(false))
                {
                    Validate(schema, identity, symbolDay, nameof(symbolDays));
                    if(previous != null && !IsAfter(symbolDay, previous))
                    {
                        throw new ArgumentException(Invariant($"{symbolDay.Symbol} {symbolDay.Date:yyyy-MM-dd} comes after {previous.Symbol} {previous.Date:yyyy-MM-dd}, out of index order."), nameof(symbolDays));
                    }

                    DateOnly day = DateOnly.FromDateTime(symbolDay.Date);
                    foreach(TSource record in symbolDay.Records)
                    {
                        buffer.Add(symbolDay.Symbol, day, record);
                        if(buffer.IsFull)
                        {
                            await buffer.FlushAsync(writer, schema).ConfigureAwait(false);
                        }
                    }

                    entries.Add(new ArchiveIndexEntry(symbolDay.Symbol, symbolDay.Date.Day, symbolDay.Records.Length));
                    previous = symbolDay;
                }

                await buffer.FlushAsync(writer, schema).ConfigureAwait(false);

                // the metadata goes into the footer, which the writer puts down when it is disposed
                index = new ArchiveIndex(identity, entries);
                writer.CustomMetadata = new Dictionary<string, string>
                {
                    [ArchiveIndex.MetadataKey] = index.ToJson(),
                };
            }

            return index;
        }

        /// <summary>
        /// Opens an archive file on disk. The file can be deleted while it is open, so retiring an old version never waits for
        /// its readers. A file whose name is not the version its index describes is refused.
        /// </summary>
        /// <typeparam name="TSource">Record interface</typeparam>
        /// <typeparam name="TRecord">Record type a read returns</typeparam>
        /// <param name="path">File path</param>
        /// <param name="schema">Archive schema of the Databento schema</param>
        /// <returns>The reader, which owns the file handle</returns>
        /// <exception cref="InvalidDataException">The file is not an archive of this schema, its index does not match its rows, or its name is not its index's version</exception>
        public static async Task<ArchiveFileReader<TSource, TRecord>> OpenAsync<TSource, TRecord>(string path, ArchiveRecordSchema<TSource, TRecord> schema)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            try
            {
                ArchiveFileReader<TSource, TRecord> reader = await OpenCoreAsync(stream, schema, leaveStreamOpen: false).ConfigureAwait(false);

                // the name says which dataset, schema, group and month the file is, so a file renamed or copied elsewhere is refused
                string fileName = Path.GetFileName(path);
                if(!string.Equals(fileName, reader.Index.Identity.FileName, StringComparison.Ordinal))
                {
                    await reader.DisposeAsync().ConfigureAwait(false);
                    throw new InvalidDataException(Invariant($"The file {fileName} holds the version {reader.Index.Identity.FileName}."));
                }

                return reader;
            }
            catch
            {
                await stream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Opens an archive file from a stream
        /// </summary>
        /// <typeparam name="TSource">Record interface</typeparam>
        /// <typeparam name="TRecord">Record type a read returns</typeparam>
        /// <param name="stream">Readable, seekable stream; left open</param>
        /// <param name="schema">Archive schema of the Databento schema</param>
        /// <returns>The reader</returns>
        /// <exception cref="InvalidDataException">The stream is not an archive of this schema, or its index does not match its rows</exception>
        public static Task<ArchiveFileReader<TSource, TRecord>> OpenAsync<TSource, TRecord>(Stream stream, ArchiveRecordSchema<TSource, TRecord> schema)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            ArgumentNullException.ThrowIfNull(stream);

            return OpenCoreAsync(stream, schema, leaveStreamOpen: true);
        }

        /// <summary>
        /// Writes one row group: each row's raw symbol, UTC day and record columns
        /// </summary>
        /// <param name="writer">File being written</param>
        /// <param name="schema">Archive schema of the Databento schema</param>
        /// <param name="rawSymbols">Each row's raw symbol</param>
        /// <param name="days">Each row's UTC day</param>
        /// <param name="records">Each row's record</param>
        internal static async Task WriteRowGroupAsync<TSource, TRecord>(ParquetWriter writer, ArchiveRecordSchema<TSource, TRecord> schema, string[] rawSymbols, DateOnly[] days, IReadOnlyList<TSource> records)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            using(ParquetRowGroupWriter rowGroup = writer.CreateRowGroup())
            {
                await rowGroup.WriteAsync(schema.RawSymbolField, rawSymbols).ConfigureAwait(false);
                await rowGroup.WriteAsync<DateOnly>(schema.DateField, days).ConfigureAwait(false);

                foreach(ArchiveColumn<TSource, TRecord> column in schema.Columns)
                {
                    await column.WriteAsync(rowGroup, records).ConfigureAwait(false);
                }
            }
        }

        private static void ThrowIfBadRowGroupSize(int rowGroupSize)
        {
            if(rowGroupSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowGroupSize), rowGroupSize, "A row group holds at least one row.");
            }
        }

        private static void Validate<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema, ArchiveFileIdentity identity, ArchiveSymbolDay<TSource> symbolDay, string paramName)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            if(symbolDay == null)
            {
                throw new ArgumentException("A symbol-day is missing.", paramName);
            }

            if(!string.Equals(ArchiveGroup.Of(identity.Dataset, symbolDay.Symbol), identity.Group, StringComparison.Ordinal))
            {
                throw new ArgumentException(Invariant($"{symbolDay.Symbol} is not of group {identity.Group}."), paramName);
            }

            if(symbolDay.Date.Year != identity.Year || symbolDay.Date.Month != identity.Month)
            {
                throw new ArgumentException(Invariant($"{symbolDay.Symbol} {symbolDay.Date:yyyy-MM-dd} is not in {identity.Year}-{identity.Month:00}."), paramName);
            }

            foreach(TSource record in symbolDay.Records)
            {
                if(record.RecordType != schema.RecordType)
                {
                    throw new InvalidDataException(Invariant($"{symbolDay.Symbol} {symbolDay.Date:yyyy-MM-dd} has a record of type {record.RecordType}, where {identity.Schema} holds {schema.RecordType}."));
                }
            }
        }

        private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach(T item in items)
            {
                yield return item;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Whether a symbol-day comes after another in index order: raw symbol (ordinal), then day
        /// </summary>
        private static bool IsAfter<TSource>(ArchiveSymbolDay<TSource> symbolDay, ArchiveSymbolDay<TSource> previous)
            where TSource : class, IRecord
        {
            int bySymbol = string.CompareOrdinal(symbolDay.Symbol, previous.Symbol);
            return bySymbol > 0 || (bySymbol == 0 && symbolDay.Date > previous.Date);
        }

        private static async Task<ArchiveFileReader<TSource, TRecord>> OpenCoreAsync<TSource, TRecord>(Stream stream, ArchiveRecordSchema<TSource, TRecord> schema, bool leaveStreamOpen)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            ArgumentNullException.ThrowIfNull(schema);

            ParquetReader reader;
            try
            {
                reader = await ParquetReader.CreateAsync(stream, ReaderOptions, leaveStreamOpen).ConfigureAwait(false);
            }
            catch(Exception ex) when(ex is not OperationCanceledException)
            {
                // Parquet.Net reports bytes that are not Parquet as a plain IOException, so the type cannot tell them from a disk
                // failure; the stream is already open, so a failure here is the file's contents either way
                throw new InvalidDataException(Invariant($"Cannot be read as a Parquet file: {ex.Message}"), ex);
            }

            try
            {
                if(reader.CustomMetadata == null || !reader.CustomMetadata.TryGetValue(ArchiveIndex.MetadataKey, out string indexJson))
                {
                    throw new InvalidDataException("The Parquet file carries no archive index.");
                }

                ArchiveIndex index = ArchiveIndex.FromJson(indexJson);

                if(!reader.Schema.Equals(schema.ParquetSchema))
                {
                    throw new InvalidDataException(Invariant($"The file's columns are not those of {index.Identity.Schema}: {reader.Schema.GetNotEqualsMessage(schema.ParquetSchema, "file", "schema")}"));
                }

                long[] rowGroupStarts = new long[reader.RowGroupCount];
                long rowCount = 0;
                for(int rowGroup = 0; rowGroup < reader.RowGroupCount; ++rowGroup)
                {
                    rowGroupStarts[rowGroup] = rowCount;
                    using(ParquetRowGroupReader rowGroupReader = reader.OpenRowGroupReader(rowGroup))
                    {
                        rowCount += rowGroupReader.RowCount;
                    }
                }

                if(rowCount != index.RowCount)
                {
                    throw new InvalidDataException(Invariant($"The file holds {rowCount} rows, but its index lists {index.RowCount}."));
                }

                return new ArchiveFileReader<TSource, TRecord>(reader, schema, index, rowGroupStarts);
            }
            catch
            {
                await reader.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// The rows of the row group being filled
        /// </summary>
        private sealed class RowGroupBuffer<TSource>
            where TSource : class, IRecord
        {
            private readonly int capacity;

            private readonly List<string> rawSymbols;

            private readonly List<DateOnly> days;

            private readonly List<TSource> records;

            public RowGroupBuffer(int capacity)
            {
                this.capacity = capacity;
                this.rawSymbols = new List<string>(capacity);
                this.days = new List<DateOnly>(capacity);
                this.records = new List<TSource>(capacity);
            }

            public bool IsFull => this.records.Count >= this.capacity;

            public void Add(string rawSymbol, DateOnly day, TSource record)
            {
                this.rawSymbols.Add(rawSymbol);
                this.days.Add(day);
                this.records.Add(record);
            }

            public async Task FlushAsync<TRecord>(ParquetWriter writer, ArchiveRecordSchema<TSource, TRecord> schema)
                where TRecord : class, TSource
            {
                if(this.records.Count == 0)
                {
                    return;
                }

                await WriteRowGroupAsync(writer, schema, this.rawSymbols.ToArray(), this.days.ToArray(), this.records).ConfigureAwait(false);

                this.rawSymbols.Clear();
                this.days.Clear();
                this.records.Clear();
            }
        }
    }
}
