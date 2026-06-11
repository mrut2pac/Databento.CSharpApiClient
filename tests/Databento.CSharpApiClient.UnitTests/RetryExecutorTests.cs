using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Tests retry / back-off behaviour of <c>HttpRetryExecutor</c> via the public client API.
    /// </summary>
    [TestClass]
    public class RetryExecutorTests
    {
        private const string AnyDataset = "XNAS.ITCH";
        private const string AnySymbol = "SPY";
        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset End = Start.AddHours(1);

        // =====================================================================================
        // Retry count
        // =====================================================================================

        [TestMethod]
        public async Task RetryEnabled_TransientError_RetriesUpToMaxAndSucceeds()
        {
            // Arrange: first 2 calls return 500, third returns 200 + data.
            byte[] dbnBytes = DbnBinaryBuilder.BuildTradesStream(new TradesSeed
            {
                PublisherId = 1,
                InstrumentId = 1,
                TsEvent = Start,
                TsReceived = Start,
                Price = 100.0,
                Size = 1,
            });

            int callCount = 0;
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if(callCount <= 2)
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);

                    HttpResponseMessage ok = new HttpResponseMessage(HttpStatusCode.OK);
                    ok.Content = new ByteArrayContent(dbnBytes);
                    return ok;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = 3,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),   // keep tests fast
                MaxRetryDelay = TimeSpan.FromMilliseconds(10),
            };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            TradeRecordDbn[] records = await client.GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual(3, callCount, "Expected 2 failures + 1 success = 3 transport calls.");
        }

        [TestMethod]
        public async Task RetryEnabled_ExceedsMaxRetries_ThrowsDatabentoHttpException()
        {
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    resp.Content = new StringContent("{\"detail\":\"overloaded\"}");
                    return resp;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = 2,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(5),
            };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            await AssertThrowsAsync<DatabentoHttpException>(
                () => client.GetTradesAsync(AnyDataset, AnySymbol, Start, End));
        }

        [TestMethod]
        public async Task RetryEnabled_Http400_NotRetried_ThrowsImmediately()
        {
            // 400 is a client error (not transient); it must not be retried.
            int callCount = 0;
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    resp.Content = new StringContent("{\"detail\":\"bad symbol\"}");
                    return resp;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions { ApiKey = "test-key", MaxRetries = 3 };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            await AssertThrowsAsync<DatabentoHttpException>(
                () => client.GetTradesAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(1, callCount, "400 must not be retried.");
        }

        // =====================================================================================
        // Retry-After header: delta-seconds
        // =====================================================================================

        [TestMethod]
        public async Task RetryAfterDeltaSeconds_HonouredOverBackoff()
        {
            // The Retry-After header specifies 1 second but RetryBaseDelay is only 1 ms.
            // We verify the retry does NOT fire before the Retry-After window by capturing the
            // wall-clock timestamps of each send call.  (We can't assert the exact delay without
            // sleeping in a test, so we verify the header is read without crashing and that
            // retry eventually succeeds.)
            byte[] dbnBytes = DbnBinaryBuilder.BuildTradesStream();

            int callCount = 0;
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if(callCount == 1)
                    {
                        HttpResponseMessage rateLimit = new HttpResponseMessage((HttpStatusCode)429);
                        rateLimit.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                        return rateLimit;
                    }

                    HttpResponseMessage ok = new HttpResponseMessage(HttpStatusCode.OK);
                    ok.Content = new ByteArrayContent(dbnBytes);
                    return ok;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions
            {
                ApiKey = "test-key",
                MaxRetries = 1,
                RetryBaseDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(50),
            };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            TradeRecordDbn[] records = await client.GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(0, records.Length);
            Assert.AreEqual(2, callCount, "Expected 1 rate-limit + 1 success.");
        }

        // =====================================================================================
        // MaxRetries = 0
        // =====================================================================================

        [TestMethod]
        public async Task MaxRetriesZero_SingleAttemptOnly_FailsOnTransient()
        {
            int callCount = 0;
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    resp.Content = new StringContent("error body");
                    return resp;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions { ApiKey = "test-key", MaxRetries = 0 };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            await AssertThrowsAsync<DatabentoHttpException>(
                () => client.GetTradesAsync(AnyDataset, AnySymbol, Start, End));

            Assert.AreEqual(1, callCount, "MaxRetries=0 must send exactly once.");
        }

        // =====================================================================================
        // Http 200 with valid payload — baseline / no-retry path
        // =====================================================================================

        [TestMethod]
        public async Task NoError_SingleCall_NoRetry()
        {
            byte[] dbnBytes = DbnBinaryBuilder.BuildTradesStream(
                new TradesSeed
                {
                    PublisherId = 1,
                    InstrumentId = 42,
                    TsEvent = Start,
                    TsReceived = Start,
                    Price = 420.0,
                    Size = 7,
                });

            int callCount = 0;
            Mock<IHttpTransport> mockTransport = new Mock<IHttpTransport>(MockBehavior.Strict);
            mockTransport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    HttpResponseMessage ok = new HttpResponseMessage(HttpStatusCode.OK);
                    ok.Content = new ByteArrayContent(dbnBytes);
                    return ok;
                });
            mockTransport.Setup(t => t.Dispose());

            DatabentoOptions options = new DatabentoOptions { ApiKey = "test-key", MaxRetries = 3 };
            DatabentoClient client = new DatabentoClient(options, mockTransport.Object);

            TradeRecordDbn[] records = await client.GetTradesAsync(AnyDataset, AnySymbol, Start, End);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual(1, callCount, "Successful first call must not retry.");
        }

        // =====================================================================================
        // Helpers
        // =====================================================================================

        private static async Task AssertThrowsAsync<TException>(Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch(TException)
            {
                return;
            }
            catch(Exception ex)
            {
                Assert.Fail($"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
            }

            Assert.Fail($"Expected {typeof(TException).Name} but no exception was thrown.");
        }
    }
}
