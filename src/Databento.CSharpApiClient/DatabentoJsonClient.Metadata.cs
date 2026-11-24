// Ignore Spelling: Api Databento Json

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;
using Databento.CSharpApiClient.Exceptions;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoJsonClient
    {
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
        /// Returns the data-quality condition of <paramref name="dataset"/> on one day, or <c>null</c> when the dataset has no entry
        /// for that day (a weekend, or before the dataset starts); a day after its available end throws <see cref="DatabentoHttpException"/>.
        /// When <paramref name="dateStr"/> is omitted the most recent day is returned, which downloads the dataset's whole daily history.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="dateStr">Optional ISO-8601 date string (e.g. <c>"2024-01-15"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<DatasetCondition> GetDatasetConditionAsync(string dataset, string dateStr = null, CancellationToken ct = default)
        {
            DatasetCondition[] days = await this.ListConditionsAsync(dataset, dateStr, dateStr, ct).ConfigureAwait(false);

            // ISO dates order as strings, so the latest day wins whatever order the API lists them in
            DatasetCondition latest = null;
            foreach(DatasetCondition day in days)
            {
                if(latest == null || string.CompareOrdinal(day.Date, latest.Date) > 0)
                {
                    latest = day;
                }
            }

            return latest;
        }

        /// <summary>
        /// Returns the data-quality condition of <paramref name="dataset"/> on one day, or <c>null</c> when the dataset has no entry
        /// for that day (a weekend, or before the dataset starts); a day after its available end throws <see cref="DatabentoHttpException"/>.
        /// When <paramref name="dateStr"/> is omitted the most recent day is returned, which downloads the dataset's whole daily history.
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

        /// <summary>
        /// Returns the daily data-quality conditions of <paramref name="dataset"/>, oldest first,
        /// optionally limited to a date range.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="startDateStr">Optional ISO-8601 start date string (inclusive), e.g. <c>"2024-01-01"</c>.</param>
        /// <param name="endDateStr">Optional ISO-8601 end date string (inclusive), e.g. <c>"2024-01-31"</c>.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<DatasetCondition[]> ListConditionsAsync(string dataset, string startDateStr = null, string endDateStr = null, CancellationToken ct = default)
        {
            StringBuilder path = new StringBuilder("metadata.get_dataset_condition?dataset=").Append(Uri.EscapeDataString(dataset));
            if(!string.IsNullOrEmpty(startDateStr))
            {
                path.Append("&start_date=").Append(Uri.EscapeDataString(startDateStr));
            }

            if(!string.IsNullOrEmpty(endDateStr))
            {
                path.Append("&end_date=").Append(Uri.EscapeDataString(endDateStr));
            }

            string json = await this.GetRawJsonAsync(path.ToString(), ct).ConfigureAwait(false);

            // the API answers with a JSON array, one element per day
            DatasetCondition[] results = JsonSerializer.Deserialize<DatasetCondition[]>(json, RecordDeserializeOptions) ?? Array.Empty<DatasetCondition>();
            foreach(DatasetCondition condition in results)
            {
                // the API doesn't send the dataset
                condition.Dataset = dataset;
            }

            return results;
        }

        /// <summary>
        /// Returns the daily data-quality conditions of <paramref name="dataset"/>, oldest first,
        /// optionally limited to a date range.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="startDateStr">Optional ISO-8601 start date string (inclusive).</param>
        /// <param name="endDateStr">Optional ISO-8601 end date string (inclusive).</param>
        public DatasetCondition[] ListConditions(string dataset, string startDateStr = null, string endDateStr = null)
            => this.ListConditionsAsync(dataset, startDateStr, endDateStr).GetAwaiter().GetResult();

        /// <summary>
        /// Returns the estimated record count for a query without transferring the actual data.
        /// Useful for capacity planning and cost estimation before submitting a full request.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols; pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full dataset.</param>
        /// <param name="schema">Schema identifier (e.g. <c>"trades"</c>).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<long> GetRecordCountAsync(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
        {
            string json = await this.PostRawJsonAsync("metadata.get_record_count", BuildMetadataCountForm(dataset, symbols, schema, startUtc, endUtc), ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<long>(json, RecordDeserializeOptions);
        }

        /// <summary>
        /// Returns the estimated record count for a query without transferring the actual data.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols.</param>
        /// <param name="schema">Schema identifier.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        public long GetRecordCount(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetRecordCountAsync(dataset, symbols, schema, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>
        /// Returns the billable uncompressed size in bytes for a query without transferring the actual data.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols; pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full dataset.</param>
        /// <param name="schema">Schema identifier (e.g. <c>"trades"</c>).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<long> GetBillableSizeAsync(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
        {
            string json = await this.PostRawJsonAsync("metadata.get_billable_size", BuildMetadataCountForm(dataset, symbols, schema, startUtc, endUtc), ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<long>(json, RecordDeserializeOptions);
        }

        /// <summary>
        /// Returns the billable uncompressed size in bytes for a query without transferring the actual data.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols.</param>
        /// <param name="schema">Schema identifier.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        public long GetBillableSize(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetBillableSizeAsync(dataset, symbols, schema, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>
        /// Returns the estimated cost in USD for a query without transferring the actual data.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols; pass <c>"ALL_SYMBOLS"</c> as the sole entry for the full dataset.</param>
        /// <param name="schema">Schema identifier (e.g. <c>"trades"</c>).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<double> GetCostAsync(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
        {
            string json = await this.PostRawJsonAsync("metadata.get_cost", BuildMetadataCountForm(dataset, symbols, schema, startUtc, endUtc), ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<double>(json, RecordDeserializeOptions);
        }

        /// <summary>
        /// Returns the estimated cost in USD for a query without transferring the actual data.
        /// </summary>
        /// <param name="dataset">The dataset identifier.</param>
        /// <param name="symbols">Instrument symbols.</param>
        /// <param name="schema">Schema identifier.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param>
        /// <param name="endUtc">Exclusive range end (UTC).</param>
        public double GetCost(string dataset, IReadOnlyList<string> symbols, string schema, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCostAsync(dataset, symbols, schema, startUtc, endUtc).GetAwaiter().GetResult();

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
            if(request.Symbols != null && request.Symbols.Length > 0)
            {
                // One comma-joined field: the endpoint keeps only the last of repeated "symbols" fields.
                form.Add(new KeyValuePair<string, string>("symbols", string.Join(",", request.Symbols)));
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
    }
}
