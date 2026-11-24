using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;

using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// The Parquet columns of one Databento schema's archive files: each column's name and type, the value it takes from a record's
    /// interface when written, and the property it sets on the record a read returns. Every row also carries the raw symbol and UTC
    /// day it belongs to, so a query sees them and a read checks it got the rows it asked for.
    /// </summary>
    /// <typeparam name="TSource">Record interface the archive is written from, which the JSON and DBN records both implement</typeparam>
    /// <typeparam name="TRecord">Record type a read returns</typeparam>
    internal abstract class ArchiveRecordSchema<TSource, TRecord>
        where TSource : class, IRecord
        where TRecord : class, TSource
    {
        protected ArchiveRecordSchema(RType recordType, IEnumerable<ArchiveColumn<TSource, TRecord>> columns)
        {
            ArgumentNullException.ThrowIfNull(columns);

            this.RecordType = recordType;
            this.Columns = columns.ToArray();
            this.ParquetSchema = new ParquetSchema(new Field[] { this.RawSymbolField, this.DateField }.Concat(this.Columns.Select(column => column.Field)));
        }

        /// <summary>
        /// Gets the Parquet schema of the archive files.
        /// </summary>
        public ParquetSchema ParquetSchema { get; }

        /// <summary>
        /// Gets the record type of the schema; the writer refuses a record of any other, such as a 1-second quote in a 1-minute schema,
        /// or a JSON record without a header, which reads as type 0.
        /// </summary>
        public RType RecordType { get; }

        /// <summary>
        /// Gets the column of each row's raw symbol.
        /// </summary>
        public DataField<string> RawSymbolField { get; } = new DataField<string>("raw_symbol", false);

        /// <summary>
        /// Gets the column of each row's UTC day.
        /// </summary>
        public DataField<DateOnly> DateField { get; } = new DataField<DateOnly>("date", false);

        /// <summary>
        /// Gets the record columns, in file order after the raw symbol and day.
        /// </summary>
        public IReadOnlyList<ArchiveColumn<TSource, TRecord>> Columns { get; }

        /// <summary>
        /// Creates an empty record of a raw symbol for a read to fill, with every nested object a column sets already in place
        /// </summary>
        internal abstract TRecord CreateRecord(string rawSymbol);

        /// <summary>
        /// Finds the first column in which a record read back differs from the record written
        /// </summary>
        /// <param name="expected">Record written</param>
        /// <param name="actual">Record read back</param>
        /// <returns>The column's name, or <c>null</c> when every column holds the same value</returns>
        internal string FindDifferentColumn(TSource expected, TSource actual)
        {
            foreach(ArchiveColumn<TSource, TRecord> column in this.Columns)
            {
                if(!column.HasSameValue(expected, actual))
                {
                    return column.Field.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks a record can be written: the schema's record type, and every column's value one the record gives and the column can store
        /// </summary>
        /// <param name="record">Record to be written</param>
        /// <returns>What is wrong with the record, or <c>null</c> when it can be written</returns>
        internal string FindUnwritableValue(TSource record)
        {
            if(record.RecordType != this.RecordType)
            {
                return Invariant($"A record of type {record.RecordType}, where the schema holds {this.RecordType}.");
            }

            foreach(ArchiveColumn<TSource, TRecord> column in this.Columns)
            {
                string problem = column.FindUnstorableValue(record);
                if(problem != null)
                {
                    return problem;
                }
            }

            return null;
        }

        /// <summary>
        /// A column of a value every record has
        /// </summary>
        protected static ArchiveColumn<TSource, TRecord> Value<TValue>(string name, Func<TSource, TValue> getValue, Action<TRecord, TValue> setValue) where TValue : struct
        {
            return new ValueArchiveColumn<TSource, TRecord, TValue>(name, getValue, setValue);
        }

        /// <summary>
        /// A column of a value a record may not have, such as a price nobody quoted
        /// </summary>
        protected static ArchiveColumn<TSource, TRecord> NullableValue<TValue>(string name, Func<TSource, TValue?> getValue, Action<TRecord, TValue?> setValue) where TValue : struct
        {
            return new NullableValueArchiveColumn<TSource, TRecord, TValue>(name, getValue, setValue);
        }

        /// <summary>
        /// A column of a UTC time, null when the record has none
        /// </summary>
        protected static ArchiveColumn<TSource, TRecord> Timestamp(string name, Func<TSource, DateTime?> getValue, Action<TRecord, DateTime?> setValue)
        {
            return new TimestampArchiveColumn<TSource, TRecord>(name, getValue, setValue);
        }

        /// <summary>
        /// The columns of the record header every Databento record starts with, read from <see cref="IRecord"/>
        /// </summary>
        protected static IEnumerable<ArchiveColumn<TSource, TRecord>> HeaderColumns(Action<TRecord, DateTime?> setTsEvent, Action<TRecord, RType> setRecordType, Action<TRecord, ushort> setPublisherId, Action<TRecord, uint> setInstrumentId)
        {
            yield return Timestamp("ts_event", record => record.TsEventUtc, setTsEvent);
            yield return Value<byte>("rtype", record => (byte)record.RecordType, (record, value) => setRecordType(record, (RType)value));
            yield return Value<ushort>("publisher_id", record => record.PublisherId, setPublisherId);
            yield return Value<uint>("instrument_id", record => record.InstrumentId, setInstrumentId);
        }
    }
}
