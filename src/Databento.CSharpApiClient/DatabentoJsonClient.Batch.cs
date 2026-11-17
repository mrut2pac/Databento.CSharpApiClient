// Ignore Spelling: Api Databento Json

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Batch;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoJsonClient
    {
        // ================================================================
        // Batch
        // ================================================================

        /// <summary>
        /// Submits a batch data job and returns the created <see cref="BatchJob"/> with its assigned ID.
        /// Batch jobs are processed asynchronously; poll <see cref="GetBatchJobDetailsAsync"/> for status.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param>
        /// <param name="symbols">One or more symbols; pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full universe.</param>
        /// <param name="schema">Schema name (e.g. <c>"trades"</c>).</param>
        /// <param name="startUtc">Inclusive start of the requested time range (UTC).</param>
        /// <param name="endUtc">Exclusive end of the requested time range (UTC).</param>
        /// <param name="encoding">Output encoding; defaults to <c>"dbn"</c>.</param>
        /// <param name="compression">Output compression; defaults to <c>"zstd"</c>.</param>
        /// <param name="prettyPx">When <see langword="true"/>, prices are formatted as display decimals.</param>
        /// <param name="prettyTs">When <see langword="true"/>, timestamps are formatted as ISO-8601 strings.</param>
        /// <param name="mapSymbols">When <see langword="true"/>, symbol mappings are appended inline.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<BatchJob> SubmitBatchJobAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string encoding = "dbn",
            string compression = "zstd",
            bool prettyPx = false,
            bool prettyTs = false,
            bool mapSymbols = false,
            CancellationToken ct = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0)
            {
                throw new ArgumentException("At least one symbol is required. Pass \"ALL_SYMBOLS\" explicitly to request the whole dataset.", nameof(symbols));
            }

            string symbolsJoined = string.Join(",", symbols);
            string start = startUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string end = endUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("dataset", dataset),
                new KeyValuePair<string, string>("symbols", symbolsJoined),
                new KeyValuePair<string, string>("schema", schema),
                new KeyValuePair<string, string>("start", start),
                new KeyValuePair<string, string>("end", end),
                new KeyValuePair<string, string>("encoding", encoding),
                new KeyValuePair<string, string>("compression", compression),
                new KeyValuePair<string, string>("pretty_px", prettyPx ? "true" : "false"),
                new KeyValuePair<string, string>("pretty_ts", prettyTs ? "true" : "false"),
                new KeyValuePair<string, string>("map_symbols", mapSymbols ? "true" : "false"),
            };

            string json = await this.PostFormAsync("batch.submit_job", form, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<BatchJob>(json, RecordDeserializeOptions);
        }

        /// <summary>
        /// Submits a batch data job and returns the created <see cref="BatchJob"/> with its assigned ID.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param>
        /// <param name="symbols">One or more symbols; pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full universe.</param>
        /// <param name="schema">Schema name (e.g. <c>"trades"</c>).</param>
        /// <param name="startUtc">Inclusive start of the requested time range (UTC).</param>
        /// <param name="endUtc">Exclusive end of the requested time range (UTC).</param>
        /// <param name="encoding">Output encoding; defaults to <c>"dbn"</c>.</param>
        /// <param name="compression">Output compression; defaults to <c>"zstd"</c>.</param>
        public BatchJob SubmitBatchJob(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string encoding = "dbn",
            string compression = "zstd")
            => this.SubmitBatchJobAsync(dataset, symbols, schema, startUtc, endUtc, encoding, compression).GetAwaiter().GetResult();

        /// <summary>
        /// Returns a summary of each batch job (its ID, state and received time), optionally limited to some states and to jobs
        /// received since a time. <see cref="GetBatchJobDetailsAsync"/> returns a job's full details.
        /// </summary>
        /// <param name="states">
        /// States to include, e.g. <c>"queued"</c>, <c>"processing"</c>, <c>"done"</c>, <c>"expired"</c>; <see langword="null"/> or empty
        /// leaves the filter to the API, which then lists jobs in every state.
        /// </param>
        /// <param name="since">Only jobs received at or after this time; <see langword="null"/> for no limit.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<BatchJob[]> ListBatchJobsAsync(IReadOnlyList<string> states = null, DateTimeOffset? since = null, CancellationToken ct = default)
        {
            StringBuilder path = new StringBuilder("batch.list_jobs");
            string sep = "?";
            if(states != null && states.Count > 0)
            {
                path.Append(sep).Append("states=").Append(Uri.EscapeDataString(string.Join(",", states)));
                sep = "&";
            }

            if(since.HasValue)
            {
                path.Append(sep).Append("since=").Append(Uri.EscapeDataString(since.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
            }

            return await this.GetJsonArrayAsync<BatchJob>(path.ToString(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a summary of each batch job, optionally limited to some states and to jobs received since a time.
        /// </summary>
        /// <param name="states">States to include; <see langword="null"/> or empty for every state.</param>
        /// <param name="since">Only jobs received at or after this time; <see langword="null"/> for no limit.</param>
        public BatchJob[] ListBatchJobs(IReadOnlyList<string> states = null, DateTimeOffset? since = null)
            => this.ListBatchJobsAsync(states, since).GetAwaiter().GetResult();

        /// <summary>Returns a summary of each batch job, optionally filtered by state. The API has no dataset filter.</summary>
        /// <param name="dataset">Ignored: <c>batch.list_jobs</c> has no dataset filter.</param>
        /// <param name="state">Optional state filter (e.g. <c>"queued"</c>, <c>"done"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        [Obsolete("batch.list_jobs has no dataset filter, so dataset is ignored. Use ListBatchJobsAsync(states, since).")]
        public Task<BatchJob[]> ListBatchJobsAsync(string dataset, string state = null, CancellationToken ct = default)
            => this.ListBatchJobsAsync(string.IsNullOrEmpty(state) ? null : new[] { state }, null, ct);

        /// <summary>Returns a summary of each batch job, optionally filtered by state. The API has no dataset filter.</summary>
        /// <param name="dataset">Ignored: <c>batch.list_jobs</c> has no dataset filter.</param>
        /// <param name="state">Optional state filter (e.g. <c>"queued"</c>, <c>"done"</c>).</param>
        [Obsolete("batch.list_jobs has no dataset filter, so dataset is ignored. Use ListBatchJobs(states, since).")]
        public BatchJob[] ListBatchJobs(string dataset, string state = null)
            => this.ListBatchJobsAsync(dataset, state).GetAwaiter().GetResult();

        /// <summary>Returns the current details and status of the batch job identified by <paramref name="jobId"/>.</summary>
        /// <param name="jobId">The job identifier returned by <see cref="SubmitBatchJobAsync"/>.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<BatchJob> GetBatchJobDetailsAsync(string jobId, CancellationToken ct = default)
        {
            string path = "batch.get_job_details?job_id=" + Uri.EscapeDataString(jobId);
            string json = await this.GetRawJsonAsync(path, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<BatchJob>(json, RecordDeserializeOptions);
        }

        /// <summary>Returns the current details and status of the batch job identified by <paramref name="jobId"/>.</summary>
        /// <param name="jobId">The job identifier returned by <see cref="SubmitBatchJob"/>.</param>
        public BatchJob GetBatchJobDetails(string jobId) => this.GetBatchJobDetailsAsync(jobId).GetAwaiter().GetResult();

        /// <summary>Returns the list of output files available for download for a completed batch job.</summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<BatchFile[]> ListBatchFilesAsync(string jobId, CancellationToken ct = default)
        {
            string path = "batch.list_files?job_id=" + Uri.EscapeDataString(jobId);
            return await this.GetJsonArrayAsync<BatchFile>(path, ct).ConfigureAwait(false);
        }

        /// <summary>Returns the list of output files available for download for a completed batch job.</summary>
        /// <param name="jobId">The job identifier.</param>
        public BatchFile[] ListBatchFiles(string jobId) => this.ListBatchFilesAsync(jobId).GetAwaiter().GetResult();

        /// <summary>
        /// Downloads a batch file and returns its raw content as a <see cref="Stream"/>.
        /// The caller is responsible for disposing the stream.
        /// </summary>
        public async Task<Stream> DownloadBatchFileAsync(string httpsUrl, CancellationToken ct = default)
        {
            HttpResponseMessage response = await HttpRetryExecutor.SendWithRetryAsync(
                this.downloadTransport,
                () => new HttpRequestMessage(HttpMethod.Get, httpsUrl),
                this.options,
                retryEnabled: true,
                ct).ConfigureAwait(false);

            try
            {
                if(!response.IsSuccessStatusCode)
                {
                    string body = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                    throw BuildHttpException((int)response.StatusCode, body);
                }

                Stream content = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);

                // Ownership transfers to the returned stream — disposing it disposes the response.
                Stream owning = new HttpResponseOwningStream(content, response);
                response = null;
                return owning;
            }
            finally
            {
                response?.Dispose();
            }
        }

        /// <summary>
        /// Downloads a batch file and returns its raw content as a <see cref="Stream"/>.
        /// The caller is responsible for disposing the stream.
        /// </summary>
        /// <param name="httpsUrl">The HTTPS download URL from a <see cref="BatchFile"/>.</param>
        public Stream DownloadBatchFile(string httpsUrl) => this.DownloadBatchFileAsync(httpsUrl).GetAwaiter().GetResult();
    }
}
