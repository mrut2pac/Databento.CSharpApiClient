// Ignore Spelling: Api Databento Json

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Batch;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Databento Historical HTTP API client — JSON encoding.
    /// Covers timeseries (all supported schemas), metadata, batch, and symbology endpoints.
    /// </summary>
    public sealed class DatabentoJsonClient : IDisposable
    {
        private readonly DatabentoOptions options;
        private readonly IHttpTransport transport;

        // STJ options: property names are set via [JsonPropertyName] attributes on each model class,
        // so no special naming policy is needed here. Unknown JSON properties are silently ignored
        // by default. DateTime values from Databento carry Z-suffix when pretty_ts=true, so STJ
        // correctly sets Kind=Utc without additional configuration.
        private static readonly JsonSerializerOptions RecordDeserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Initialises a new <see cref="DatabentoJsonClient"/> using the supplied options.
        /// A default <see cref="HttpClient"/> with Basic authentication is created unless a custom
        /// <paramref name="transport"/> is provided (useful for unit testing).
        /// </summary>
        /// <param name="options">Connection and authentication settings.</param>
        /// <param name="transport">Optional custom HTTP transport; leave <see langword="null"/> to use the built-in client.</param>
        public DatabentoJsonClient(DatabentoOptions options, IHttpTransport transport = null)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if(string.IsNullOrWhiteSpace(this.options.ApiKey))
            {
                throw new ArgumentException("ApiKey must be set.", nameof(options));
            }

            if(transport != null)
            {
                this.transport = transport;
            }
            else
            {
                HttpClient httpClient = new HttpClient
                {
                    BaseAddress = this.options.BaseUri,
                    Timeout = this.options.Timeout,
                };
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(this.options.UserAgent ?? "DatabentoJsonClient/1.0");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string apiKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.options.ApiKey + ":"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKeyBase64);

                this.transport = new DefaultHttpTransport(httpClient);
            }
        }

        /// <summary>Releases the underlying HTTP transport (and its <see cref="HttpClient"/>).</summary>
        public void Dispose() => this.transport.Dispose();

        // ================================================================
        // Metadata
        // ================================================================

        /// <summary>Returns all dataset identifiers available to the authenticated account.</summary>
        /// <param name="ct">Cancellation token.</param>
        public Task<string[]> ListDatasetsAsync(CancellationToken ct = default)
            => this.GetJsonArrayAsync<string>("metadata.list_datasets", ct);

        /// <summary>Returns all dataset identifiers available to the authenticated account.</summary>
        public string[] ListDatasets() => this.ListDatasetsAsync().GetAwaiter().GetResult();

        /// <summary>Returns the schema identifiers supported by <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier (e.g. <c>"XNAS.ITCH"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<string[]> ListSchemasAsync(string dataset, CancellationToken ct = default)
            => this.GetJsonArrayAsync<string>("metadata.list_schemas?dataset=" + Uri.EscapeDataString(dataset), ct);

        /// <summary>Returns the schema identifiers supported by <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier (e.g. <c>"XNAS.ITCH"</c>).</param>
        public string[] ListSchemas(string dataset) => this.ListSchemasAsync(dataset).GetAwaiter().GetResult();

        /// <summary>Returns metadata for all publishers available in the Databento network.</summary>
        /// <param name="ct">Cancellation token.</param>
        public Task<PublisherInfo[]> ListPublishersAsync(CancellationToken ct = default)
            => this.GetJsonArrayAsync<PublisherInfo>("metadata.list_publishers", ct);

        /// <summary>Returns metadata for all publishers available in the Databento network.</summary>
        public PublisherInfo[] ListPublishers() => this.ListPublishersAsync().GetAwaiter().GetResult();

        /// <summary>Returns the field names and types available for <paramref name="schema"/>.</summary>
        /// <param name="schema">Schema identifier (e.g. <c>"trades"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<FieldInfo[]> ListFieldsAsync(string schema, CancellationToken ct = default)
            => this.GetJsonArrayAsync<FieldInfo>("metadata.list_fields?schema=" + Uri.EscapeDataString(schema) + "&encoding=json", ct);

        /// <summary>Returns the field names and types available for <paramref name="schema"/>.</summary>
        /// <param name="schema">Schema identifier (e.g. <c>"trades"</c>).</param>
        public FieldInfo[] ListFields(string schema) => this.ListFieldsAsync(schema).GetAwaiter().GetResult();

        /// <summary>Returns per-schema unit prices for <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<UnitPriceInfo[]> ListUnitPricesAsync(string dataset, CancellationToken ct = default)
            => this.GetJsonArrayAsync<UnitPriceInfo>("metadata.list_unit_prices?dataset=" + Uri.EscapeDataString(dataset), ct);

        /// <summary>Returns per-schema unit prices for <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier.</param>
        public UnitPriceInfo[] ListUnitPrices(string dataset) => this.ListUnitPricesAsync(dataset).GetAwaiter().GetResult();

        /// <summary>
        /// Returns the data-quality condition for <paramref name="dataset"/> on a given date.
        /// When <paramref name="dateStr"/> is omitted the most recent condition entry is returned.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="dateStr">Optional ISO-8601 date string (e.g. <c>"2024-01-15"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<DatasetCondition> GetDatasetConditionAsync(string dataset, string dateStr = null, CancellationToken ct = default)
        {
            string path = "metadata.get_dataset_condition?dataset=" + Uri.EscapeDataString(dataset);
            if(!string.IsNullOrEmpty(dateStr))
            {
                path += "&date=" + Uri.EscapeDataString(dateStr);
            }

            string json = await this.GetRawJsonAsync(path, ct).ConfigureAwait(false);

            // The API returns an array of daily conditions (no "dataset" field in each element —
            // only "date", "condition", "last_modified_date"). Return the first entry and back-fill
            // the dataset identifier from the request parameter.
            DatasetCondition result;
            using(JsonDocument doc = JsonDocument.Parse(json))
            {
                if(doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    string firstElementJson = null;
                    foreach(JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        firstElementJson = element.GetRawText();
                        break;
                    }

                    if(firstElementJson == null)
                    {
                        return null;
                    }

                    result = JsonSerializer.Deserialize<DatasetCondition>(firstElementJson, RecordDeserializeOptions);
                }
                else
                {
                    result = JsonSerializer.Deserialize<DatasetCondition>(json, RecordDeserializeOptions);
                }
            }

            if(result != null && result.Dataset == null)
            {
                result.Dataset = dataset;
            }

            return result;
        }

        /// <summary>
        /// Returns the data-quality condition for <paramref name="dataset"/> on a given date.
        /// When <paramref name="dateStr"/> is omitted the most recent condition entry is returned.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="dateStr">Optional ISO-8601 date string (e.g. <c>"2024-01-15"</c>).</param>
        public DatasetCondition GetDatasetCondition(string dataset, string dateStr = null)
            => this.GetDatasetConditionAsync(dataset, dateStr).GetAwaiter().GetResult();

        /// <summary>Returns the available date range for <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<DateRange> GetDatasetRangeAsync(string dataset, CancellationToken ct = default)
        {
            string path = "metadata.get_dataset_range?dataset=" + Uri.EscapeDataString(dataset);
            string json = await this.GetRawJsonAsync(path, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DateRange>(json, RecordDeserializeOptions);
        }

        /// <summary>Returns the available date range for <paramref name="dataset"/>.</summary>
        /// <param name="dataset">The dataset identifier.</param>
        public DateRange GetDatasetRange(string dataset) => this.GetDatasetRangeAsync(dataset).GetAwaiter().GetResult();

        // ================================================================
        // Symbology
        // ================================================================

        /// <summary>Resolves symbols from one symbology type to another for a given date range.</summary>
        /// <param name="request">The resolution request parameters.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<SymbologyResolution> ResolveSymbolsAsync(SymbologyRequest request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            // The symbology.resolve endpoint requires form-encoded POST (not JSON body).
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();
            if(!string.IsNullOrEmpty(request.Dataset))
                form.Add(new KeyValuePair<string, string>("dataset", request.Dataset));
            if(request.Symbols != null)
            {
                foreach(string sym in request.Symbols)
                    form.Add(new KeyValuePair<string, string>("symbols", sym));
            }
            if(!string.IsNullOrEmpty(request.StypeIn))
                form.Add(new KeyValuePair<string, string>("stype_in", request.StypeIn));
            if(!string.IsNullOrEmpty(request.StypeOut))
                form.Add(new KeyValuePair<string, string>("stype_out", request.StypeOut));
            if(!string.IsNullOrEmpty(request.StartDate))
                form.Add(new KeyValuePair<string, string>("start_date", request.StartDate));
            if(!string.IsNullOrEmpty(request.EndDate))
                form.Add(new KeyValuePair<string, string>("end_date", request.EndDate));

            string json = await this.PostFormAsync("symbology.resolve", form, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<SymbologyResolution>(json, RecordDeserializeOptions);
        }

        /// <summary>Resolves symbols from one symbology type to another for a given date range.</summary>
        /// <param name="request">The resolution request parameters.</param>
        public SymbologyResolution ResolveSymbols(SymbologyRequest request)
            => this.ResolveSymbolsAsync(request).GetAwaiter().GetResult();

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

        /// <summary>Returns all batch jobs, optionally filtered by dataset and/or state.</summary>
        /// <param name="dataset">Optional dataset filter.</param>
        /// <param name="state">Optional state filter (e.g. <c>"received"</c>, <c>"done"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<BatchJob[]> ListBatchJobsAsync(string dataset = null, string state = null, CancellationToken ct = default)
        {
            StringBuilder path = new StringBuilder("batch.list_jobs");
            string sep = "?";
            if(!string.IsNullOrEmpty(dataset))
            {
                path.Append(sep).Append("dataset=").Append(Uri.EscapeDataString(dataset));
                sep = "&";
            }

            if(!string.IsNullOrEmpty(state))
            {
                path.Append(sep).Append("state=").Append(Uri.EscapeDataString(state));
            }

            return await this.GetJsonArrayAsync<BatchJob>(path.ToString(), ct).ConfigureAwait(false);
        }

        /// <summary>Returns all batch jobs, optionally filtered by dataset and/or state.</summary>
        /// <param name="dataset">Optional dataset filter.</param>
        /// <param name="state">Optional state filter (e.g. <c>"received"</c>, <c>"done"</c>).</param>
        public BatchJob[] ListBatchJobs(string dataset = null, string state = null)
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
                this.transport,
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

        // ================================================================
        // Timeseries — CBBO
        // ================================================================

        /// <summary>Returns 1-second consolidated best-bid-and-offer bars for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<CbboRecordJson[]> GetCbbo1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetCbboAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, ct);

        /// <summary>Returns 1-minute consolidated best-bid-and-offer bars for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<CbboRecordJson[]> GetCbbo1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetCbboAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Min, startUtc, endUtc, ct);

        /// <summary>Returns 1-second consolidated best-bid-and-offer bars for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public CbboRecordJson[] GetCbbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCbbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute consolidated best-bid-and-offer bars for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public CbboRecordJson[] GetCbbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCbbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-second consolidated best-bid-and-offer bars for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<CbboRecordJson[]> GetCbbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetCbboAsync(dataset, symbols, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, ct);

        /// <summary>Returns 1-minute consolidated best-bid-and-offer bars for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<CbboRecordJson[]> GetCbbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetCbboAsync(dataset, symbols, Schema.ConsolidatedBBO1Min, startUtc, endUtc, ct);

        /// <summary>Returns 1-second consolidated best-bid-and-offer bars for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public CbboRecordJson[] GetCbbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCbbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute consolidated best-bid-and-offer bars for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public CbboRecordJson[] GetCbbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCbbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — OHLCV
        // ================================================================

        /// <summary>Returns 1-second OHLCV bars for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code (e.g. <c>"XNAS.ITCH"</c>).</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, new[] { symbol }, Schema.Ohlcv1Sec, startUtc, endUtc, ct);

        /// <summary>Returns 1-minute OHLCV bars for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, new[] { symbol }, Schema.Ohlcv1Min, startUtc, endUtc, ct);

        /// <summary>Returns 1-hour OHLCV bars for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1hAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, new[] { symbol }, Schema.Ohlcv1Hour, startUtc, endUtc, ct);

        /// <summary>Returns 1-day OHLCV bars for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1dAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, new[] { symbol }, Schema.Ohlcv1Day, startUtc, endUtc, ct);

        /// <summary>Returns end-of-day OHLCV bars for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcvEodAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, new[] { symbol }, Schema.OhlcvEod, startUtc, endUtc, ct);

        /// <summary>Returns 1-second OHLCV bars for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute OHLCV bars for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-hour OHLCV bars for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1h(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1hAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-day OHLCV bars for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1d(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1dAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns end-of-day OHLCV bars for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcvEod(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcvEodAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-second OHLCV bars for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000). Pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full universe.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, symbols, Schema.Ohlcv1Sec, startUtc, endUtc, ct);

        /// <summary>Returns 1-minute OHLCV bars for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, symbols, Schema.Ohlcv1Min, startUtc, endUtc, ct);

        /// <summary>Returns 1-hour OHLCV bars for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1hAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, symbols, Schema.Ohlcv1Hour, startUtc, endUtc, ct);

        /// <summary>Returns 1-day OHLCV bars for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcv1dAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, symbols, Schema.Ohlcv1Day, startUtc, endUtc, ct);

        /// <summary>Returns end-of-day OHLCV bars for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<OhlcvRecordJson[]> GetOhlcvEodAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetOhlcvAsync(dataset, symbols, Schema.OhlcvEod, startUtc, endUtc, ct);

        /// <summary>Returns 1-second OHLCV bars for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute OHLCV bars for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-hour OHLCV bars for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1h(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1hAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-day OHLCV bars for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcv1d(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcv1dAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns end-of-day OHLCV bars for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public OhlcvRecordJson[] GetOhlcvEod(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetOhlcvEodAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — Trades
        // ================================================================

        /// <summary>Returns individual trade tick records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TradeRecordJson[]> GetTradesAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Trades, startUtc, endUtc, DeserializeTradesJson, ct);

        /// <summary>Returns individual trade tick records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TradeRecordJson[]> GetTradesAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Trades, startUtc, endUtc, DeserializeTradesJson, ct);

        /// <summary>Returns individual trade tick records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TradeRecordJson[] GetTrades(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTradesAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns individual trade tick records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TradeRecordJson[] GetTrades(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTradesAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — MBP-1
        // ================================================================

        /// <summary>Returns market-by-price depth-1 (top-of-book) records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Mbp1RecordJson[]> GetMbp1Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbp1, startUtc, endUtc, DeserializeMbp1Json, ct);

        /// <summary>Returns market-by-price depth-1 (top-of-book) records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Mbp1RecordJson[]> GetMbp1Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Mbp1, startUtc, endUtc, DeserializeMbp1Json, ct);

        /// <summary>Returns market-by-price depth-1 (top-of-book) records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Mbp1RecordJson[] GetMbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMbp1Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns market-by-price depth-1 (top-of-book) records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Mbp1RecordJson[] GetMbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMbp1Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — Statistics
        // ================================================================

        /// <summary>Returns end-of-day statistics records (settlement, open interest, etc.) for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<StatisticsRecordJson[]> GetStatisticsAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Statistics, startUtc, endUtc, DeserializeStatisticsJson, ct);

        /// <summary>Returns end-of-day statistics records (settlement, open interest, etc.) for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<StatisticsRecordJson[]> GetStatisticsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Statistics, startUtc, endUtc, DeserializeStatisticsJson, ct);

        /// <summary>Returns end-of-day statistics records (settlement, open interest, etc.) for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public StatisticsRecordJson[] GetStatistics(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetStatisticsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns end-of-day statistics records (settlement, open interest, etc.) for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public StatisticsRecordJson[] GetStatistics(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetStatisticsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — Definitions
        // ================================================================

        /// <summary>Returns instrument definition records (contract metadata) for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<DefinitionRecordJson[]> GetDefinitionsAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Definition, startUtc, endUtc, DeserializeDefinitionsJson, ct);

        /// <summary>Returns instrument definition records (contract metadata) for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<DefinitionRecordJson[]> GetDefinitionsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Definition, startUtc, endUtc, DeserializeDefinitionsJson, ct);

        /// <summary>Returns instrument definition records (contract metadata) for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public DefinitionRecordJson[] GetDefinitions(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetDefinitionsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns instrument definition records (contract metadata) for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public DefinitionRecordJson[] GetDefinitions(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetDefinitionsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Core HTTP helpers (with retry)
        // ================================================================

        private Task<HttpResponseMessage> GetWithRetryAsync(string path, CancellationToken ct)
        {
            return HttpRetryExecutor.SendWithRetryAsync(
                this.transport,
                () => new HttpRequestMessage(HttpMethod.Get, path),
                this.options,
                retryEnabled: true,
                ct);
        }

        private async Task<string> GetRawJsonAsync(string path, CancellationToken ct)
        {
            using(HttpResponseMessage response = await this.GetWithRetryAsync(path, ct).ConfigureAwait(false))
            {
                string body = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                if(!response.IsSuccessStatusCode)
                {
                    throw BuildHttpException((int)response.StatusCode, body);
                }

                return body;
            }
        }

        private async Task<T[]> GetJsonArrayAsync<T>(string path, CancellationToken ct)
        {
            string json = await this.GetRawJsonAsync(path, ct).ConfigureAwait(false);
            using(JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                if(root.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<T[]>(json, RecordDeserializeOptions);
                }

                // Some endpoints wrap the array in an envelope object; try common keys.
                if(root.ValueKind == JsonValueKind.Object)
                {
                    foreach(string key in new[] { "result", "data", "items" })
                    {
                        JsonElement arr;
                        if(root.TryGetProperty(key, out arr) && arr.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<T[]>(arr.GetRawText(), RecordDeserializeOptions);
                        }
                    }
                }
            }

            throw new InvalidDataException("Unexpected JSON structure for array endpoint: " + path);
        }

        // Used only for symbology.resolve, which is idempotent — safe to retry on transient errors.
        private async Task<string> PostJsonAsync(string path, string jsonBody, CancellationToken ct)
        {
            using(HttpResponseMessage response = await HttpRetryExecutor.SendWithRetryAsync(
                this.transport,
                () => new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                },
                this.options,
                retryEnabled: true,
                ct).ConfigureAwait(false))
            {
                string body = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                if(!response.IsSuccessStatusCode)
                {
                    throw BuildHttpException((int)response.StatusCode, body);
                }

                return body;
            }
        }

        // batch.submit_job is NOT idempotent — retrying could create duplicate jobs, so retries are disabled
        // here on purpose. Do not route this through the retrying GET/POST helpers.
        private async Task<string> PostFormAsync(string path, IEnumerable<KeyValuePair<string, string>> form, CancellationToken ct)
        {
            using(HttpResponseMessage response = await HttpRetryExecutor.SendWithRetryAsync(
                this.transport,
                () => new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = new FormUrlEncodedContent(form),
                },
                this.options,
                retryEnabled: false,
                ct).ConfigureAwait(false))
            {
                string body = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                if(!response.IsSuccessStatusCode)
                {
                    throw BuildHttpException((int)response.StatusCode, body);
                }

                return body;
            }
        }

        // ================================================================
        // Timeseries helpers
        // ================================================================

        private async Task<CbboRecordJson[]> GetCbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken ct)
        {
            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc);
            using(HttpResponseMessage response = await this.GetWithRetryAsync(path, ct).ConfigureAwait(false))
            {
                if(!response.IsSuccessStatusCode)
                {
                    string errBody = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                    if(IsNoDataResponse((int)response.StatusCode, errBody))
                    {
                        return Array.Empty<CbboRecordJson>();
                    }

                    throw BuildHttpException((int)response.StatusCode, errBody);
                }

                using(Stream stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
                {
                    return DeserializeCbboJson(stream, ct);
                }
            }
        }

        private async Task<OhlcvRecordJson[]> GetOhlcvAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken ct)
        {
            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc);
            using(HttpResponseMessage response = await this.GetWithRetryAsync(path, ct).ConfigureAwait(false))
            {
                if(!response.IsSuccessStatusCode)
                {
                    string errBody = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                    if(IsNoDataResponse((int)response.StatusCode, errBody))
                    {
                        return Array.Empty<OhlcvRecordJson>();
                    }

                    throw BuildHttpException((int)response.StatusCode, errBody);
                }

                using(Stream stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
                {
                    return DeserializeOhlcvJson(stream, ct);
                }
            }
        }

        private async Task<T[]> GetGenericRecordsAsync<T>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            Func<Stream, CancellationToken, T[]> deserialize,
            CancellationToken ct)
        {
            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc);
            using(HttpResponseMessage response = await this.GetWithRetryAsync(path, ct).ConfigureAwait(false))
            {
                if(!response.IsSuccessStatusCode)
                {
                    string errBody = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                    if(IsNoDataResponse((int)response.StatusCode, errBody))
                    {
                        return Array.Empty<T>();
                    }

                    throw BuildHttpException((int)response.StatusCode, errBody);
                }

                using(Stream stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
                {
                    return deserialize(stream, ct);
                }
            }
        }

        // ================================================================
        // Query builder
        // ================================================================

        private static string BuildTimeseriesQuery(
            string dataset,
            string schema,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string stypeIn = null)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0)
            {
                // An empty list silently meaning "the entire universe" is a costly footgun. Callers who
                // genuinely want every symbol must opt in explicitly with a single "ALL_SYMBOLS" entry.
                throw new ArgumentException("At least one symbol is required. Pass \"ALL_SYMBOLS\" explicitly to request the whole dataset.", nameof(symbols));
            }

            string start = startUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string end = endUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string symbolParam = string.Join(",", symbols);

            StringBuilder sb = new StringBuilder();
            sb.Append("timeseries.get_range");
            sb.Append("?dataset=").Append(Uri.EscapeDataString(dataset));
            sb.Append("&schema=").Append(Uri.EscapeDataString(schema));
            sb.Append("&symbols=").Append(Uri.EscapeDataString(symbolParam));
            sb.Append("&start=").Append(Uri.EscapeDataString(start));
            sb.Append("&end=").Append(Uri.EscapeDataString(end));
            sb.Append("&stype_in=").Append(Uri.EscapeDataString(stypeIn ?? SymbolTypes.RawSymbol));
            sb.Append("&pretty_px=true&pretty_ts=true&encoding=json&compression=none");
            return sb.ToString();
        }

        // ================================================================
        // JSON-Lines / array deserializers
        // ================================================================

        private static CbboRecordJson[] DeserializeCbboJson(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<CbboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static OhlcvRecordJson[] DeserializeOhlcvJson(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<OhlcvRecordJson>(stream, rec => rec?.Header != null, ct);

        private static TradeRecordJson[] DeserializeTradesJson(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<TradeRecordJson>(stream, rec => rec?.Header != null, ct);

        private static Mbp1RecordJson[] DeserializeMbp1Json(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<Mbp1RecordJson>(stream, rec => rec?.Header != null, ct);

        private static StatisticsRecordJson[] DeserializeStatisticsJson(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<StatisticsRecordJson>(stream, rec => rec?.Header != null, ct);

        private static DefinitionRecordJson[] DeserializeDefinitionsJson(Stream stream, CancellationToken ct)
            => DeserializeJsonLines<DefinitionRecordJson>(stream, rec => rec?.Header != null, ct);

        private static T[] DeserializeJsonLines<T>(Stream stream, Func<T, bool> isValid, CancellationToken ct)
        {
            List<T> results = new List<T>();

            using(StreamReader sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: false))
            {
                int peek;
                do { peek = sr.Peek(); } while(peek == ' ' || peek == '\t' || peek == '\r' || peek == '\n');

                if(peek == '[')
                {
                    string json = sr.ReadToEnd();
                    List<T> arr = JsonSerializer.Deserialize<List<T>>(json, RecordDeserializeOptions);
                    if(arr != null)
                    {
                        foreach(T item in arr)
                        {
                            ct.ThrowIfCancellationRequested();
                            if(isValid(item))
                            {
                                results.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    string line;
                    while((line = sr.ReadLine()) != null)
                    {
                        ct.ThrowIfCancellationRequested();
                        if(string.IsNullOrWhiteSpace(line) || line[0] != '{')
                        {
                            continue;
                        }

                        T item;
                        try
                        {
                            item = JsonSerializer.Deserialize<T>(line, RecordDeserializeOptions);
                        }
                        catch
                        {
                            continue;
                        }

                        if(isValid(item))
                        {
                            results.Add(item);
                        }
                    }
                }
            }

            return results.ToArray();
        }

        // ================================================================
        // Error helpers
        // ================================================================

        private static bool IsNoDataResponse(int statusCode, string body)
        {
            string errorCase = ExtractErrorCase(body);
            return statusCode == 422 && errorCase == "data_end_after_available_end";
        }

        private static string ExtractErrorCase(string body)
        {
            if(string.IsNullOrEmpty(body))
            {
                return null;
            }

            try
            {
                using(JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement detail;
                    if(!doc.RootElement.TryGetProperty("detail", out detail))
                    {
                        return null;
                    }

                    if(detail.ValueKind == JsonValueKind.Object)
                    {
                        JsonElement caseEl;
                        if(detail.TryGetProperty("case", out caseEl))
                        {
                            return caseEl.GetString();
                        }
                    }
                }
            }
            catch
            {
                // Not a structured JSON error.
            }

            return null;
        }

        private static DatabentoHttpException BuildHttpException(int statusCode, string body)
        {
            string errorCase = ExtractErrorCase(body);
            return new DatabentoHttpException(statusCode, body, errorCase);
        }

        private static async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
