// Ignore Spelling: Api

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoClient
    {
        // =====================================================================================
        // Core fetch: CBBO (shared by cbbo-1s and cbbo-1m)
        // =====================================================================================

        private async Task<CbboRecordDbn[]> GetCbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string schema,
            CancellationToken cancellationToken)
        {
            return await this.GetRecordsAsync(dataset, symbols, schema, startUtc, endUtc, DbnRecordLayouts.Cbbo, cancellationToken).ConfigureAwait(false);
        }

        // =====================================================================================
        // Core fetch: OHLCV (shared by all 5 OHLCV intervals)
        // =====================================================================================

        private async Task<OhlcvRecordDbn[]> GetOhlcvAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string schema,
            CancellationToken cancellationToken)
        {
            return await this.GetRecordsAsync(dataset, symbols, schema, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken).ConfigureAwait(false);
        }

        // =====================================================================================
        // Core fetch: BBO (shared by bbo-1s and bbo-1m)
        // =====================================================================================

        private async Task<BboRecordDbn[]> GetBboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string schema,
            CancellationToken cancellationToken)
        {
            return await this.GetRecordsAsync(dataset, symbols, schema, startUtc, endUtc, DbnRecordLayouts.Bbo, cancellationToken).ConfigureAwait(false);
        }

        // =====================================================================================
        // Core fetch: every schema
        // =====================================================================================

        private async Task<TRecord[]> GetRecordsAsync<TRecord>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            DbnRecordLayout<TRecord> layout,
            CancellationToken cancellationToken)
        {
            ValidateTimeseriesArguments(dataset, symbols, schema);

            List<TRecord> records = new List<TRecord>();
            await foreach(TRecord record in this.StreamRecordsCoreAsync(dataset, symbols, schema, startUtc, endUtc, layout, cancellationToken).ConfigureAwait(false))
            {
                records.Add(record);
            }

            return records.ToArray();
        }

        private IAsyncEnumerable<TRecord> StreamRecordsAsync<TRecord>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            DbnRecordLayout<TRecord> layout,
            CancellationToken cancellationToken)
        {
            // checked here, when the method is called, rather than on the first MoveNextAsync of the iterator
            ValidateTimeseriesArguments(dataset, symbols, schema);
            return this.StreamRecordsCoreAsync(dataset, symbols, schema, startUtc, endUtc, layout, cancellationToken);
        }

        private async IAsyncEnumerable<TRecord> StreamRecordsCoreAsync<TRecord>(
            string dataset,
            IReadOnlyList<string> symbols,
            string schema,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            DbnRecordLayout<TRecord> layout,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.OpenGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                await foreach(TRecord record in DbnDecoder.DecodeAsync(stream, layout, cancellationToken).ConfigureAwait(false))
                {
                    yield return record;
                }
            }
        }

        private static void ValidateTimeseriesArguments(string dataset, IReadOnlyList<string> symbols, string schema)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }
            if(string.IsNullOrWhiteSpace(schema)) { throw new ArgumentException("schema required", nameof(schema)); }
        }

        // =====================================================================================
        // HTTP transport with retry
        // =====================================================================================

        private async Task<Stream> OpenGetAsync(string path, CancellationToken cancellationToken)
        {
            // only the status is retried, before any of the body is read: once records have been handed out, a retry would repeat them
            HttpResponseMessage response = await HttpRetryExecutor.SendWithRetryAsync(
                this.transport,
                () => new HttpRequestMessage(HttpMethod.Get, path),
                this.options,
                retryEnabled: true,
                cancellationToken).ConfigureAwait(false);

            try
            {
                if(!response.IsSuccessStatusCode)
                {
                    await ThrowHttpException(response, cancellationToken).ConfigureAwait(false);
                }

                Stream content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return new HttpResponseOwningStream(content, response);
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        private static async Task ThrowHttpException(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string body = string.Empty;
            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                body = string.Empty;
            }

            throw DatabentoHttpException.Create((int)response.StatusCode, body);
        }

        // =====================================================================================
        // Query builder
        // =====================================================================================

        private static string BuildTimeseriesQuery(
            string dataset,
            string schema,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string stypeIn)
        {
            string start = startUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string end = endUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            StringBuilder symbolList = new StringBuilder();
            for(int i = 0; i < symbols.Count; i++)
            {
                if(i > 0)
                {
                    symbolList.Append(',');
                }

                symbolList.Append(Uri.EscapeDataString(symbols[i]));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("timeseries.get_range");
            sb.Append("?dataset=").Append(Uri.EscapeDataString(dataset));
            sb.Append("&schema=").Append(Uri.EscapeDataString(schema));
            sb.Append("&symbols=").Append(symbolList);
            sb.Append("&start=").Append(Uri.EscapeDataString(start));
            sb.Append("&end=").Append(Uri.EscapeDataString(end));
            sb.Append("&stype_in=").Append(Uri.EscapeDataString(stypeIn ?? "raw_symbol"));
            sb.Append("&encoding=dbn&compression=none");
            return sb.ToString();
        }
    }
}
