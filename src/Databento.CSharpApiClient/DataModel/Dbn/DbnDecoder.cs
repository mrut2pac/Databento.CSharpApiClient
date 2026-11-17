using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Decodes a DBN stream one record at a time: the metadata first, then each record as soon as its bytes have arrived, so
    /// a caller holds one record at a time however long the stream is. The stream is read to its end and left open.
    /// </summary>
    internal static class DbnDecoder
    {
        private const int PreludeBytes = 8;

        public static async IAsyncEnumerable<TRecord> DecodeAsync<TRecord>(
            Stream stream,
            DbnRecordLayout<TRecord> layout,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // records are read a few bytes at a time, so read the source in large chunks; not disposed, which would close the
            // caller's stream
            stream = new BufferedStream(stream, 64 * 1024);

            DbnMetadata metadata = await ReadMetadataAsync(stream, cancellationToken).ConfigureAwait(false);
            DbnSymbolMap symbolMap = layout.AssignSymbol == null ? null : new DbnSymbolMap(metadata);

            // a record's first byte is its length in 4-byte words, so a record never exceeds 1020 bytes
            byte[] buffer = new byte[byte.MaxValue * 4];
            using(MemoryStream recordStream = new MemoryStream(buffer, writable: false))
            using(BinaryReader reader = new BinaryReader(recordStream))
            {
                while(true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if(await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false) == 0)
                    {
                        yield break;
                    }

                    int length = buffer[0] * 4;
                    if(length < DbnRecordHeader.SizeBytes)
                    {
                        throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid DBN record length {0}.", length));
                    }

                    await ReadExactlyAsync(stream, buffer.AsMemory(1, length - 1), "Truncated DBN record.", cancellationToken).ConfigureAwait(false);

                    recordStream.Position = 0;
                    DbnRecordHeader header = DbnRecordHeader.ReadFromBytes(reader);
                    if(!layout.Accepts(header.RecordType))
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture, "Unexpected rtype 0x{0:X2} in {1} stream.", (byte)header.RecordType, layout.Name));
                    }

                    TRecord record = layout.Read(header, reader);
                    layout.AssignSymbol?.Invoke(record, symbolMap);
                    yield return record;
                }
            }
        }

        private static async Task<DbnMetadata> ReadMetadataAsync(Stream stream, CancellationToken cancellationToken)
        {
            byte[] prelude = new byte[PreludeBytes];
            await ReadExactlyAsync(stream, prelude, "Truncated DBN metadata.", cancellationToken).ConfigureAwait(false);

            // checked before the length is trusted: anything else, such as zstd-compressed DBN or an HTML error page, would
            // otherwise have its bytes read as a metadata length of up to 2 GB
            if(prelude[0] != (byte)'D' || prelude[1] != (byte)'B' || prelude[2] != (byte)'N')
            {
                throw new InvalidDataException("Not DBN data: it does not start with 'DBN'. Zstandard-compressed data must be decompressed first.");
            }

            uint metadataLength = BinaryPrimitives.ReadUInt32LittleEndian(prelude.AsSpan(4));
            if(metadataLength > int.MaxValue - PreludeBytes)
            {
                throw new InvalidDataException("Metadata length overflow.");
            }

            byte[] block = new byte[PreludeBytes + (int)metadataLength];
            prelude.CopyTo(block, 0);
            await ReadExactlyAsync(stream, block.AsMemory(PreludeBytes), "Truncated DBN metadata.", cancellationToken).ConfigureAwait(false);

            using(MemoryStream metadataStream = new MemoryStream(block, writable: false))
            using(BinaryReader reader = new BinaryReader(metadataStream))
            {
                try
                {
                    return DbnMetadata.FromBinaryReader(reader);
                }
                catch(EndOfStreamException ex)
                {
                    throw new InvalidDataException("Malformed DBN metadata: its contents overrun its length.", ex);
                }
            }
        }

        private static async Task ReadExactlyAsync(Stream stream, Memory<byte> buffer, string truncatedMessage, CancellationToken cancellationToken)
        {
            try
            {
                await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch(EndOfStreamException ex)
            {
                throw new InvalidDataException(truncatedMessage, ex);
            }
        }
    }
}
