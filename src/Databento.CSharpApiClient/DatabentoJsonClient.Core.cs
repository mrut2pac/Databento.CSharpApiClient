// Ignore Spelling: Api Databento Json

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.JsonSupport;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoJsonClient
    {
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

        // Serves batch.submit_job and symbology.resolve, and retries neither: batch.submit_job is NOT idempotent, and
        // retrying it could create duplicate jobs, so retries are disabled here on purpose. symbology.resolve could be
        // retried safely, but goes through here too. Do not route batch.submit_job through the retrying GET helpers.
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

        private Task<CbboRecordJson[]> GetCbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken ct)
        {
            return this.GetGenericRecordsAsync(dataset, symbols, schema, startUtc, endUtc, DeserializeCbboJson, ct);
        }

        private Task<OhlcvRecordJson[]> GetOhlcvAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken ct)
        {
            return this.GetGenericRecordsAsync(dataset, symbols, schema, startUtc, endUtc, DeserializeOhlcvJson, ct);
        }

        private async Task<T[]> GetGenericRecordsAsync<T>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            Func<Stream, CancellationToken, IAsyncEnumerable<T>> deserialize,
            CancellationToken ct)
        {
            List<T> records = new List<T>();
            await foreach(T record in this.StreamGenericRecordsAsync(dataset, symbols, schema, startUtc, endUtc, deserialize, ct).ConfigureAwait(false))
            {
                records.Add(record);
            }

            return records.ToArray();
        }

        private IAsyncEnumerable<T> StreamGenericRecordsAsync<T>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            Func<Stream, CancellationToken, IAsyncEnumerable<T>> deserialize,
            CancellationToken ct)
        {
            // built here, when the method is called, so bad arguments are refused then rather than on the first MoveNextAsync
            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc);
            string requestedSymbol = CoversMoreThanOneSymbol(symbols) ? null : symbols[0];
            return this.StreamGenericRecordsCoreAsync(path, requestedSymbol, deserialize, ct);
        }

        private async IAsyncEnumerable<T> StreamGenericRecordsCoreAsync<T>(
            string path,
            string requestedSymbol,
            Func<Stream, CancellationToken, IAsyncEnumerable<T>> deserialize,
            [EnumeratorCancellation] CancellationToken ct)
        {
            // only the status is retried, before any of the body is read: once records have been handed out, a retry would repeat them
            using(HttpResponseMessage response = await this.GetWithRetryAsync(path, ct).ConfigureAwait(false))
            {
                if(!response.IsSuccessStatusCode)
                {
                    string errBody = await ReadBodyAsync(response, ct).ConfigureAwait(false);
                    if(IsNoDataResponse((int)response.StatusCode, errBody))
                    {
                        yield break;
                    }

                    throw BuildHttpException((int)response.StatusCode, errBody);
                }

                using(Stream stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
                {
                    await foreach(T record in deserialize(stream, ct).ConfigureAwait(false))
                    {
                        AssignRequestedSymbol(record, requestedSymbol);
                        yield return record;
                    }
                }
            }
        }

        /// <summary>
        /// Gives a record of a single-symbol request the symbol requested: such a request sends no map_symbols, so the
        /// response names none, and every record belongs to that symbol.
        /// </summary>
        private static void AssignRequestedSymbol<T>(T record, string requestedSymbol)
        {
            if(requestedSymbol != null && record is ISymbolRecord named && named.Symbol == null)
            {
                named.Symbol = requestedSymbol;
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

            if(CoversMoreThanOneSymbol(symbols))
            {
                // The response interleaves the requested symbols, so each record has to name its own.
                // It is deliberately not requested for a single symbol: the caller already knows the
                // answer, and the field is repeated on every record rather than sent once per response.
                sb.Append("&map_symbols=true");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Tells whether a request can return records of more than one symbol, which is the only case where
        /// a record needs to name the symbol it belongs to.
        /// </summary>
        private static bool CoversMoreThanOneSymbol(IReadOnlyList<string> symbols)
        {
            return symbols.Count > 1
                || string.Equals(symbols[0], AllSymbols, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildMetadataCountQuery(
            string endpoint,
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0)
            {
                throw new ArgumentException("At least one symbol is required. Pass \"ALL_SYMBOLS\" explicitly to request the whole dataset.", nameof(symbols));
            }

            string start = startUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string end = endUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string symbolParam = string.Join(",", symbols);

            StringBuilder sb = new StringBuilder();
            sb.Append(endpoint);
            sb.Append("?dataset=").Append(Uri.EscapeDataString(dataset));
            sb.Append("&schema=").Append(Uri.EscapeDataString(schema));
            sb.Append("&symbols=").Append(Uri.EscapeDataString(symbolParam));
            sb.Append("&start=").Append(Uri.EscapeDataString(start));
            sb.Append("&end=").Append(Uri.EscapeDataString(end));
            sb.Append("&stype_in=").Append(Uri.EscapeDataString(SymbolTypes.RawSymbol));
            return sb.ToString();
        }

        // ================================================================
        // JSON-Lines / array deserializers
        // ================================================================

        private static IAsyncEnumerable<CbboRecordJson> DeserializeCbboJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<CbboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<OhlcvRecordJson> DeserializeOhlcvJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<OhlcvRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<TradeRecordJson> DeserializeTradesJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<TradeRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<Mbp1RecordJson> DeserializeMbp1Json(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<Mbp1RecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<StatisticsRecordJson> DeserializeStatisticsJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<StatisticsRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<DefinitionRecordJson> DeserializeDefinitionsJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<DefinitionRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<MboRecordJson> DeserializeMboJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<MboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<Mbp10RecordJson> DeserializeMbp10Json(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<Mbp10RecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<BboRecordJson> DeserializeBboJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<BboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<TbboRecordJson> DeserializeTbboJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<TbboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<TcbboRecordJson> DeserializeTcbboJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<TcbboRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<Cmbp1RecordJson> DeserializeCmbp1Json(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<Cmbp1RecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<StatusRecordJson> DeserializeStatusJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<StatusRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<ImbalanceRecordJson> DeserializeImbalanceJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<ImbalanceRecordJson>(stream, rec => rec?.Header != null, ct);

        private static IAsyncEnumerable<SymbolMappingRecordJson> DeserializeSymbolMappingJson(Stream stream, CancellationToken ct)
            => ReadJsonLinesAsync<SymbolMappingRecordJson>(stream, rec => rec?.Header != null, ct);

        private static async IAsyncEnumerable<T> ReadJsonLinesAsync<T>(Stream stream, Func<T, bool> isValid, [EnumeratorCancellation] CancellationToken ct)
        {
            using(StreamReader sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: false))
            {
                string line = await sr.ReadLineAsync(ct).ConfigureAwait(false);

                // A JSON array document starts with '['; it is read whole, so it does not stream. Anything else is
                // newline-delimited JSON (the timeseries encoding), read one line, and so one record, at a time.
                if(line != null && line.StartsWith('['))
                {
                    string json = line + "\n" + await sr.ReadToEndAsync(ct).ConfigureAwait(false);
                    List<T> arr = JsonSerializer.Deserialize<List<T>>(json, RecordDeserializeOptions);
                    foreach(T item in arr ?? new List<T>())
                    {
                        ct.ThrowIfCancellationRequested();
                        if(isValid(item))
                        {
                            yield return item;
                        }
                    }

                    yield break;
                }

                long lineNumber = 0;
                for(; line != null; line = await sr.ReadLineAsync(ct).ConfigureAwait(false))
                {
                    ct.ThrowIfCancellationRequested();
                    lineNumber++;

                    // A blank separator line (e.g. a trailing newline) is legal in JSONL and carries
                    // no record. Any non-blank line is expected to be a complete record object.
                    if(string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    T item = DeserializeJsonLine<T>(line, lineNumber);
                    if(isValid(item))
                    {
                        yield return item;
                    }
                }
            }
        }

        private static T DeserializeJsonLine<T>(string line, long lineNumber)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(line, RecordDeserializeOptions);
            }
            catch(Exception ex) when(ex is JsonException or FormatException or OverflowException or NotSupportedException)
            {
                // A record that fails to parse is a real error, not "no data". Swallowing it
                // here previously returned zero rows for responses that actually contained
                // data (e.g. string-encoded numeric fields), so surface it to the caller.
                // A custom converter may throw Format/Overflow rather than JsonException.
                throw new DatabentoException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Failed to deserialize a {0} record from the JSONL response at line {1}: {2}",
                        typeof(T).Name,
                        lineNumber,
                        ex.Message),
                    ex);
            }
        }

        // ================================================================
        // Error helpers
        // ================================================================

        private static bool IsNoDataResponse(int statusCode, string body)
        {
            return statusCode == 422
                && DatabentoHttpException.MapErrorCase(DatabentoHttpException.ExtractErrorCase(body)) == DatabentoErrorCase.DataEndAfterAvailableEnd;
        }

        private static DatabentoHttpException BuildHttpException(int statusCode, string body)
        {
            return DatabentoHttpException.Create(statusCode, body);
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
