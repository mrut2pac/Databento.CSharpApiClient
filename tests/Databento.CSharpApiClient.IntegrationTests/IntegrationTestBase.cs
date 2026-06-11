using System;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Base class for all integration tests.
    /// Reads the API key from the <c>DATABENTO_API_KEY</c> environment variable.
    /// Individual tests must call <see cref="SkipIfNoApiKey"/> at their start so
    /// that tests are cleanly skipped in CI environments without credentials.
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly string ApiKey;

        protected IntegrationTestBase()
        {
            this.ApiKey = Environment.GetEnvironmentVariable("DATABENTO_API_KEY") ?? string.Empty;
        }

        /// <summary>
        /// Skips the test when <c>DATABENTO_API_KEY</c> is not set.
        /// Call at the very start of every <c>[SkippableFact]</c> test body.
        /// </summary>
        protected void SkipIfNoApiKey()
        {
            Skip.If(string.IsNullOrWhiteSpace(this.ApiKey), "DATABENTO_API_KEY environment variable not set.");
        }

        protected DatabentoJsonClient CreateJsonClient()
            => new DatabentoJsonClient(new DatabentoOptions { ApiKey = this.ApiKey });

        protected DatabentoClient CreateBinaryClient()
            => new DatabentoClient(new DatabentoOptions { ApiKey = this.ApiKey });

        public void Dispose() { }
    }
}
