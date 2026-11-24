using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Parquet;
using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.Archive.Columns
{
    /// <summary>
    /// A column of a value a record may not have, stored as a Parquet null
    /// </summary>
    internal sealed class NullableValueArchiveColumn<TSource, TRecord, TValue> : ArchiveColumn<TSource, TRecord> where TValue : struct
    {
        private readonly DataField<TValue?> field;

        private readonly Func<TSource, TValue?> getValue;

        private readonly Action<TRecord, TValue?> setValue;

        public NullableValueArchiveColumn(string name, Func<TSource, TValue?> getValue, Action<TRecord, TValue?> setValue)
        {
            this.field = new DataField<TValue?>(name, true);
            this.getValue = getValue;
            this.setValue = setValue;
        }

        internal override DataField Field => this.field;

        internal override async Task WriteAsync(ParquetRowGroupWriter writer, IReadOnlyList<TSource> records)
        {
            TValue?[] values = new TValue?[records.Count];
            for(int i = 0; i < values.Length; ++i)
            {
                values[i] = this.getValue(records[i]);
            }

            await writer.WriteAsync<TValue>(this.field, values).ConfigureAwait(false);
        }

        internal override async Task<object> ReadValuesAsync(ParquetRowGroupReader reader)
        {
            TValue?[] values = new TValue?[reader.RowCount];
            await reader.ReadAsync<TValue>(this.field, values).ConfigureAwait(false);

            return values;
        }

        internal override void Fill(object values, int firstRow, TRecord[] target, int targetOffset, int count)
        {
            TValue?[] rowGroupValues = (TValue?[])values;
            for(int i = 0; i < count; ++i)
            {
                this.setValue(target[targetOffset + i], rowGroupValues[firstRow + i]);
            }
        }

        internal override bool HasSameValue(TSource expected, TSource actual)
        {
            TValue? expectedValue = this.getValue(expected);
            TValue? actualValue = this.getValue(actual);

            // a record type may hold a missing price as null or as NaN, and a read can give back either
            return (IsUndefined(expectedValue) && IsUndefined(actualValue)) || EqualityComparer<TValue?>.Default.Equals(expectedValue, actualValue);
        }

        internal override string FindUnstorableValue(TSource record)
        {
            try
            {
                this.getValue(record);
            }
            catch(OverflowException ex)
            {
                return this.DescribeUngivableValue(ex);
            }

            return null;
        }

        private static bool IsUndefined(TValue? value)
        {
            return !value.HasValue || (value.Value is double number && double.IsNaN(number));
        }
    }
}
