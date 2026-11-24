using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Parquet;
using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.Archive.Columns
{
    /// <summary>
    /// A column of a value every record has
    /// </summary>
    internal sealed class ValueArchiveColumn<TSource, TRecord, TValue> : ArchiveColumn<TSource, TRecord> where TValue : struct
    {
        private readonly DataField<TValue> field;

        private readonly Func<TSource, TValue> getValue;

        private readonly Action<TRecord, TValue> setValue;

        public ValueArchiveColumn(string name, Func<TSource, TValue> getValue, Action<TRecord, TValue> setValue)
        {
            this.field = new DataField<TValue>(name, false);
            this.getValue = getValue;
            this.setValue = setValue;
        }

        internal override DataField Field => this.field;

        internal override async Task WriteAsync(ParquetRowGroupWriter writer, IReadOnlyList<TSource> records)
        {
            TValue[] values = new TValue[records.Count];
            for(int i = 0; i < values.Length; ++i)
            {
                values[i] = this.getValue(records[i]);
            }

            await writer.WriteAsync<TValue>(this.field, values).ConfigureAwait(false);
        }

        internal override async Task<object> ReadValuesAsync(ParquetRowGroupReader reader)
        {
            TValue[] values = new TValue[reader.RowCount];
            await reader.ReadAsync<TValue>(this.field, values).ConfigureAwait(false);

            return values;
        }

        internal override void Fill(object values, int firstRow, TRecord[] target, int targetOffset, int count)
        {
            TValue[] rowGroupValues = (TValue[])values;
            for(int i = 0; i < count; ++i)
            {
                this.setValue(target[targetOffset + i], rowGroupValues[firstRow + i]);
            }
        }

        internal override bool HasSameValue(TSource expected, TSource actual)
        {
            return EqualityComparer<TValue>.Default.Equals(this.getValue(expected), this.getValue(actual));
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
    }
}
