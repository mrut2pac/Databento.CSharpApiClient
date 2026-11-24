using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Parquet;
using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.Archive.Columns
{
    /// <summary>
    /// A column of UTC times stored as Parquet nanosecond timestamps, null allowed. The default date, how a record holds an undefined
    /// time, is stored as null. A time that is not UTC is refused, since it would come back marked UTC and shift by the machine's offset.
    /// </summary>
    internal sealed class TimestampArchiveColumn<TSource, TRecord> : ArchiveColumn<TSource, TRecord>
    {
        private readonly DateTimeDataField field;

        private readonly Func<TSource, DateTime?> getValue;

        private readonly Action<TRecord, DateTime?> setValue;

        public TimestampArchiveColumn(string name, Func<TSource, DateTime?> getValue, Action<TRecord, DateTime?> setValue)
        {
            this.field = new DateTimeDataField(name, DateTimeFormat.Timestamp, true, DateTimeTimeUnit.Nanos, true);
            this.getValue = getValue;
            this.setValue = setValue;
        }

        internal override DataField Field => this.field;

        internal override async Task WriteAsync(ParquetRowGroupWriter writer, IReadOnlyList<TSource> records)
        {
            DateTime?[] values = new DateTime?[records.Count];
            for(int i = 0; i < values.Length; ++i)
            {
                string problem = this.FindUnstorableValue(records[i]);
                if(problem != null)
                {
                    throw new InvalidDataException(problem);
                }

                values[i] = Normalize(this.getValue(records[i]));
            }

            await writer.WriteAsync<DateTime>(this.field, values).ConfigureAwait(false);
        }

        internal override string FindUnstorableValue(TSource record)
        {
            DateTime? value;
            try
            {
                value = Normalize(this.getValue(record));
            }
            catch(OverflowException ex)
            {
                return this.DescribeUngivableValue(ex);
            }

            return value.HasValue && value.Value.Kind != DateTimeKind.Utc
                ? Invariant($"Column {this.field.Name} holds {value.Value:o}, a {value.Value.Kind} time; only UTC times can be archived.")
                : null;
        }

        internal override async Task<object> ReadValuesAsync(ParquetRowGroupReader reader)
        {
            DateTime?[] values = new DateTime?[reader.RowCount];
            await reader.ReadAsync<DateTime>(this.field, values).ConfigureAwait(false);

            return values;
        }

        internal override void Fill(object values, int firstRow, TRecord[] target, int targetOffset, int count)
        {
            DateTime?[] rowGroupValues = (DateTime?[])values;
            for(int i = 0; i < count; ++i)
            {
                DateTime? value = rowGroupValues[firstRow + i];
                this.setValue(target[targetOffset + i], value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);
            }
        }

        internal override bool HasSameValue(TSource expected, TSource actual)
        {
            return Normalize(this.getValue(expected)) == Normalize(this.getValue(actual));
        }

        /// <summary>
        /// The default date is stored as null, so the two are the same value
        /// </summary>
        private static DateTime? Normalize(DateTime? value)
        {
            return value == DateTime.MinValue ? null : value;
        }
    }
}
