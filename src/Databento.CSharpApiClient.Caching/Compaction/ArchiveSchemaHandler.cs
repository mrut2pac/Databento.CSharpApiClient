using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// A Databento schema as a compaction run sees it: its archive columns and the reader of its day files' JSON records
    /// </summary>
    /// <typeparam name="TSource">Record interface</typeparam>
    /// <typeparam name="TRecord">Record type an archive read returns</typeparam>
    /// <typeparam name="TJson">Record type a day file holds</typeparam>
    internal sealed class ArchiveSchemaHandler<TSource, TRecord, TJson> : IArchiveSchemaHandler
        where TSource : class, IRecord
        where TRecord : class, TSource
        where TJson : class, TSource
    {
        public ArchiveSchemaHandler(string schema, ArchiveRecordSchema<TSource, TRecord> recordSchema, Func<Stream, CancellationToken, IAsyncEnumerable<TJson>> readDayFile)
        {
            this.Schema = schema;
            this.RecordSchema = recordSchema;
            this.ReadDayFile = readDayFile;
        }

        public string Schema { get; }

        /// <summary>
        /// Gets the archive columns of the schema.
        /// </summary>
        public ArchiveRecordSchema<TSource, TRecord> RecordSchema { get; }

        /// <summary>
        /// Gets the reader of a day file's JSON records, decompressed.
        /// </summary>
        public Func<Stream, CancellationToken, IAsyncEnumerable<TJson>> ReadDayFile { get; }

        public Task CompactAsync(ArchiveGroupRequest request, CompactionRun run, CancellationToken cancellationToken)
        {
            return new ArchiveGroupCompaction<TSource, TRecord, TJson>(this, request, run).RunAsync(cancellationToken);
        }
    }
}
