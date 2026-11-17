using System;
using System.Net.Http;
using System.Threading;

using Moq;

using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// A transport that answers each call with the response the given function builds for its 1-based call number.
    /// </summary>
    internal sealed class FakeTransport
    {
        private readonly Mock<IHttpTransport> mock = new Mock<IHttpTransport>(MockBehavior.Strict);

        public FakeTransport(Func<int, HttpResponseMessage> respond)
        {
            this.mock
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => respond(++this.CallCount));
            this.mock.Setup(t => t.Dispose());
        }

        public int CallCount { get; private set; }

        public IHttpTransport Object => this.mock.Object;
    }
}
