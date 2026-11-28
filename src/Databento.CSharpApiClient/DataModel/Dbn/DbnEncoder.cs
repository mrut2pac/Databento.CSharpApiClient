using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Encodes a DBN stream, the reverse of <see cref="DbnDecoder"/>: the metadata first, then each record as it is enumerated,
    /// gathered in blocks so the stream is written in large chunks however many records there are. The stream is left open.
    /// </summary>
    internal static class DbnEncoder
    {
        private const int BlockBytes = 64 * 1024;

        public static async Task EncodeAsync<TRecord>(
            Stream stream,
            DbnMetadata metadata,
            IAsyncEnumerable<TRecord> records,
            DbnRecordLayout<TRecord> layout,
            CancellationToken cancellationToken)
        {
            using(Block<TRecord> block = new Block<TRecord>(metadata, layout))
            {
                await foreach(TRecord record in records.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    block.Add(record);
                    if(block.IsFull)
                    {
                        await block.WriteToAsync(stream, cancellationToken).ConfigureAwait(false);
                    }
                }

                await block.WriteToAsync(stream, cancellationToken).ConfigureAwait(false);
            }

            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public static void Encode<TRecord>(Stream stream, DbnMetadata metadata, IEnumerable<TRecord> records, DbnRecordLayout<TRecord> layout)
        {
            using(Block<TRecord> block = new Block<TRecord>(metadata, layout))
            {
                foreach(TRecord record in records)
                {
                    block.Add(record);
                    if(block.IsFull)
                    {
                        block.WriteTo(stream);
                    }
                }

                block.WriteTo(stream);
            }

            stream.Flush();
        }

        /// <summary>The encoded bytes not yet written to the stream, starting with the metadata.</summary>
        private sealed class Block<TRecord> : IDisposable
        {
            private readonly MemoryStream buffer = new MemoryStream(BlockBytes + byte.MaxValue * 4);

            private readonly BinaryWriter writer;

            private readonly DbnRecordLayout<TRecord> layout;

            private readonly byte version;

            public Block(DbnMetadata metadata, DbnRecordLayout<TRecord> layout)
            {
                // a record class keeps no ts_out, so there would be none to append to each record
                if(metadata.TsOut)
                {
                    throw new NotSupportedException("DBN with ts_out is not written: the record classes do not keep the ts_out of each record.");
                }

                this.writer = new BinaryWriter(this.buffer);
                this.layout = layout;
                this.version = metadata.Version;
                metadata.WriteToBinaryWriter(this.writer);
            }

            public bool IsFull => this.buffer.Length >= BlockBytes;

            public void Add(TRecord record)
            {
                if(record == null)
                {
                    throw new ArgumentException("The records hold a null record.");
                }

                long start = this.buffer.Length;
                this.layout.Write(record, this.writer, this.version);

                // the record type is the record's second byte; the reader would refuse a stream holding another schema's records
                byte recordType = this.buffer.GetBuffer()[start + 1];
                if(!this.layout.Accepts((RType)recordType))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "A record of rtype 0x{0:X2} is not a {1} record.", recordType, this.layout.Name));
                }
            }

            public void WriteTo(Stream stream)
            {
                stream.Write(this.buffer.GetBuffer(), 0, (int)this.buffer.Length);
                this.buffer.SetLength(0);
            }

            public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
            {
                await stream.WriteAsync(this.buffer.GetBuffer().AsMemory(0, (int)this.buffer.Length), cancellationToken).ConfigureAwait(false);
                this.buffer.SetLength(0);
            }

            public void Dispose()
            {
                this.writer.Dispose();
            }
        }
    }
}
