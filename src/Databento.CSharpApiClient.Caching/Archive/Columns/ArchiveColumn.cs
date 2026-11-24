using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Parquet;
using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.Archive.Columns
{
    /// <summary>
    /// One Parquet column of an archive file: the value it takes from a record being written and where it puts the value on a
    /// record being read. Built through the factory methods of <see cref="Schemas.ArchiveRecordSchema{TSource, TRecord}"/>.
    /// </summary>
    /// <typeparam name="TSource">Record interface the value is written from</typeparam>
    /// <typeparam name="TRecord">Record type a read fills</typeparam>
    internal abstract class ArchiveColumn<TSource, TRecord>
    {
        internal ArchiveColumn()
        {
        }

        /// <summary>
        /// Gets the Parquet field of the column.
        /// </summary>
        internal abstract DataField Field { get; }

        /// <summary>
        /// Writes the column of a row group
        /// </summary>
        /// <param name="writer">Row group being written</param>
        /// <param name="records">The row group's records, in row order</param>
        internal abstract Task WriteAsync(ParquetRowGroupWriter writer, IReadOnlyList<TSource> records);

        /// <summary>
        /// Decodes the column of a whole row group, so the days that share the row group are read from one decode
        /// </summary>
        /// <param name="reader">Row group being read</param>
        /// <returns>The row group's values in row order, for <see cref="Fill"/></returns>
        internal abstract Task<object> ReadValuesAsync(ParquetRowGroupReader reader);

        /// <summary>
        /// Sets a run of a row group's decoded values on the records of a day being read
        /// </summary>
        /// <param name="values">The row group's values, from <see cref="ReadValuesAsync"/></param>
        /// <param name="firstRow">First row of the run, within the row group</param>
        /// <param name="target">Records to set the value on, already created</param>
        /// <param name="targetOffset">Index in <paramref name="target"/> the run's first row goes to</param>
        /// <param name="count">Number of rows in the run</param>
        internal abstract void Fill(object values, int firstRow, TRecord[] target, int targetOffset, int count);

        /// <summary>
        /// Whether two records hold the same value as the column stores it, so a record read back can be checked against the one written
        /// </summary>
        /// <param name="expected">Record written</param>
        /// <param name="actual">Record read back</param>
        /// <returns><c>true</c> when the column would store the same value for both</returns>
        internal abstract bool HasSameValue(TSource expected, TSource actual);

        /// <summary>
        /// Checks the record gives its value, and one the column can store, which a write would otherwise fail on halfway through a file.
        /// Some JSON models' interface getters do checked casts, so a value out of the interface's range throws <see cref="OverflowException"/>.
        /// </summary>
        /// <param name="record">Record to be written</param>
        /// <returns>What is wrong with the value, or <c>null</c> when it can be stored</returns>
        internal abstract string FindUnstorableValue(TSource record);

        /// <summary>
        /// Says that a record cannot give the column's value
        /// </summary>
        protected string DescribeUngivableValue(OverflowException exception)
        {
            return Invariant($"Column {this.Field.Name} holds a value the record interface cannot give: {exception.Message}");
        }
    }
}
