using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Transport
{
    /// <summary>
    /// Single source of truth for transient-error retry behaviour shared by both clients:
    /// exponential backoff with equal jitter, an honoured <c>Retry-After</c> header, and a delay cap.
    /// Retries only idempotent requests — callers performing non-idempotent writes (e.g. batch.submit_job)
    /// must pass <paramref name="retryEnabled"/> = false.
    /// </summary>
    internal static class HttpRetryExecutor
    {
        /// <summary>
        /// Sends a request (rebuilt per attempt via <paramref name="requestFactory"/>, since an
        /// <see cref="HttpRequestMessage"/> and its content cannot be reused) and retries transient
        /// failures (HTTP 429 and 5xx). Returns the first non-transient or final response, undisposed —
        /// the caller owns disposal.
        /// </summary>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(
            IHttpTransport transport,
            Func<HttpRequestMessage> requestFactory,
            DatabentoOptions options,
            bool retryEnabled,
            CancellationToken cancellationToken)
        {
            int maxRetries = retryEnabled ? Math.Max(0, options.MaxRetries) : 0;

            for(int attempt = 0; ; attempt++)
            {
                HttpResponseMessage response;
                using(HttpRequestMessage request = requestFactory())
                {
                    response = await transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }

                bool isTransient = (int)response.StatusCode == 429 || (int)response.StatusCode >= 500;
                if(!isTransient || attempt >= maxRetries)
                {
                    return response;
                }

                TimeSpan delay = ComputeDelay(attempt, options, response);
                response.Dispose();
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        private static TimeSpan ComputeDelay(int attempt, DatabentoOptions options, HttpResponseMessage response)
        {
            TimeSpan cap = options.MaxRetryDelay > TimeSpan.Zero ? options.MaxRetryDelay : TimeSpan.FromSeconds(30);

            // A server-provided Retry-After (delta-seconds or HTTP-date) wins over computed backoff.
            RetryConditionHeaderValue retryAfter = response.Headers.RetryAfter;
            if(retryAfter != null)
            {
                if(retryAfter.Delta.HasValue && retryAfter.Delta.Value > TimeSpan.Zero)
                {
                    return Min(retryAfter.Delta.Value, cap);
                }

                if(retryAfter.Date.HasValue)
                {
                    TimeSpan untilDate = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                    if(untilDate > TimeSpan.Zero)
                    {
                        return Min(untilDate, cap);
                    }
                }
            }

            // Exponential backoff with "equal jitter": half fixed + half random, decorrelating concurrent
            // clients so they do not retry in lockstep (thundering herd) while still backing off.
            double baseMs = options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
            double cappedMs = Math.Min(baseMs, cap.TotalMilliseconds);
            double half = cappedMs / 2.0;
            double jittered = half + (Random.Shared.NextDouble() * half);
            return TimeSpan.FromMilliseconds(jittered);
        }

        private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
    }
}
