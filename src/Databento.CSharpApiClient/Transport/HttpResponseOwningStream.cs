using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Wraps the content stream of an <see cref="HttpResponseMessage"/> and disposes the response
    /// (releasing the underlying connection) when the stream is disposed. This lets a streaming
    /// download be handed to a caller with a single <see cref="IDisposable"/> to manage, without
    /// buffering the (potentially multi-gigabyte) payload into memory.
    /// </summary>
    internal sealed class HttpResponseOwningStream : Stream
    {
        private readonly Stream inner;
        private readonly HttpResponseMessage response;

        public HttpResponseOwningStream(Stream inner, HttpResponseMessage response)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public override bool CanRead => this.inner.CanRead;

        public override bool CanSeek => this.inner.CanSeek;

        public override bool CanWrite => false;

        public override long Length => this.inner.Length;

        public override long Position
        {
            get => this.inner.Position;
            set => this.inner.Position = value;
        }

        public override void Flush() => this.inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => this.inner.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => this.inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => this.inner.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => this.inner.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.inner.Dispose();
                this.response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
