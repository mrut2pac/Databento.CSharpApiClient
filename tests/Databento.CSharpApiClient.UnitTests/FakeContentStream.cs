using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// A response body for streaming tests: it serves the given bytes, can fail with an <see cref="IOException"/> once a set
    /// number of bytes has been read (a connection dropping mid-body), and records whether it was disposed.
    /// </summary>
    internal sealed class FakeContentStream : MemoryStream
    {
        private readonly long failAfterBytes;

        public FakeContentStream(byte[] bytes, long failAfterBytes = long.MaxValue)
            : base(bytes, writable: false)
        {
            this.failAfterBytes = failAfterBytes;
        }

        public bool IsDisposed { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(this.Position >= this.failAfterBytes)
            {
                throw new IOException("The connection dropped.");
            }

            int allowed = (int)Math.Min(count, this.failAfterBytes - this.Position);
            return base.Read(buffer, offset, allowed);
        }

        public override int Read(Span<byte> buffer)
        {
            byte[] chunk = new byte[buffer.Length];
            int read = this.Read(chunk, 0, chunk.Length);
            chunk.AsSpan(0, read).CopyTo(buffer);
            return read;
        }

        public override int ReadByte()
        {
            byte[] one = new byte[1];
            return this.Read(one, 0, 1) == 0 ? -1 : one[0];
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Read(buffer, offset, count));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(this.Read(buffer.Span));
        }

        protected override void Dispose(bool disposing)
        {
            this.IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
