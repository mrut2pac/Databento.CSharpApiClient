using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads bytes already read from a stream, then the rest of that stream, so a reader can look at the start of data it
    /// cannot seek back in. Disposing it leaves the stream open.
    /// </summary>
    internal sealed class PrefixedStream : Stream
    {
        private readonly byte[] prefix;

        private readonly int prefixLength;

        private readonly Stream rest;

        private int prefixPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixedStream"/> class.
        /// </summary>
        /// <param name="prefix">The bytes already read</param>
        /// <param name="prefixLength">How many of <paramref name="prefix"/> were read</param>
        /// <param name="rest">The stream they were read from, positioned after them; left open</param>
        public PrefixedStream(byte[] prefix, int prefixLength, Stream rest)
        {
            this.prefix = prefix;
            this.prefixLength = prefixLength;
            this.rest = rest;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            return this.Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            int fromPrefix = this.ReadPrefix(buffer);
            return fromPrefix > 0 ? fromPrefix : this.rest.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);
            return this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int fromPrefix = this.ReadPrefix(buffer.Span);
            return fromPrefix > 0 ? new ValueTask<int>(fromPrefix) : this.rest.ReadAsync(buffer, cancellationToken);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private int ReadPrefix(Span<byte> buffer)
        {
            int count = Math.Min(buffer.Length, this.prefixLength - this.prefixPosition);
            if(count <= 0)
            {
                return 0;
            }

            this.prefix.AsSpan(this.prefixPosition, count).CopyTo(buffer);
            this.prefixPosition += count;
            return count;
        }
    }
}
