using System;
using System.IO;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// How one DBN schema's records are decoded: which record types it holds, how a record's body is read and, for the
    /// records that carry one, how its symbol is found from the stream's symbol mappings.
    /// </summary>
    /// <typeparam name="TRecord">Record type</typeparam>
    internal sealed class DbnRecordLayout<TRecord>
    {
        public DbnRecordLayout(string name, Func<RType, bool> accepts, Func<DbnRecordHeader, BinaryReader, TRecord> read, Action<TRecord, DbnSymbolMap> assignSymbol = null)
        {
            this.Name = name;
            this.Accepts = accepts;
            this.Read = read;
            this.AssignSymbol = assignSymbol;
        }

        /// <summary>
        /// Gets the schema's name, as error messages show it.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether a record type belongs to the schema.
        /// </summary>
        public Func<RType, bool> Accepts { get; }

        /// <summary>
        /// Gets the reader of a record's body, given its header and a reader positioned after it.
        /// </summary>
        public Func<DbnRecordHeader, BinaryReader, TRecord> Read { get; }

        /// <summary>
        /// Gets what sets a record's symbol from the stream's symbol mappings; <see langword="null"/> for records without one.
        /// </summary>
        public Action<TRecord, DbnSymbolMap> AssignSymbol { get; }
    }
}
