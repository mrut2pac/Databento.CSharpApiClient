// Ignore Spelling: Api

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.Transport;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Databento historical data service DBN-binary client.
    /// Covers <c>timeseries.get_range</c> for CBBO, Trades, and MBP-1 schemas.
    /// </summary>
    public sealed class DatabentoClient : IDisposable
    {
        private readonly DatabentoOptions options;
        private readonly IHttpTransport transport;

        public DatabentoClient(DatabentoOptions options, IHttpTransport transport = null)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if(string.IsNullOrWhiteSpace(this.options.ApiKey))
            {
                throw new ArgumentException("ApiKey needs to be specified, otherwise no data will be provided!", nameof(options));
            }

            if(transport != null)
            {
                this.transport = transport;
            }
            else
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = this.options.BaseUri;
                httpClient.Timeout = this.options.Timeout;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(this.options.UserAgent ?? "DatabentoClient/1.0");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                string apiKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.options.ApiKey + ":"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKeyBase64);

                this.transport = new DefaultHttpTransport(httpClient);
            }
        }

        public void Dispose()
        {
            this.transport.Dispose();
        }

        // =====================================================================================
        // CBBO 1-second / 1-minute
        // =====================================================================================

        /// <summary>Gets consolidated BBO 1-second snapshots for a single symbol.</summary>
        public Task<CbboRecordDbn[]> GetCbbo1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetCbboAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.ConsolidatedBBO1Sec, cancellationToken);
        }

        /// <summary>Gets consolidated BBO 1-second snapshots for multiple symbols.</summary>
        public Task<CbboRecordDbn[]> GetCbbo1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetCbboAsync(dataset, symbols, startUtc, endUtc, Schema.ConsolidatedBBO1Sec, cancellationToken);
        }

        /// <summary>Gets consolidated BBO 1-minute snapshots for a single symbol.</summary>
        public Task<CbboRecordDbn[]> GetCbbo1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetCbboAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.ConsolidatedBBO1Min, cancellationToken);
        }

        /// <summary>Gets consolidated BBO 1-minute snapshots for multiple symbols.</summary>
        public Task<CbboRecordDbn[]> GetCbbo1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetCbboAsync(dataset, symbols, startUtc, endUtc, Schema.ConsolidatedBBO1Min, cancellationToken);
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCbbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public CbboRecordDbn[] GetCbbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCbbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCbbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public CbboRecordDbn[] GetCbbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCbbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCbbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public CbboRecordDbn[] GetCbbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCbbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCbbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public CbboRecordDbn[] GetCbbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCbbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Trades (schema=trades, rtype=Mbp0)
        // =====================================================================================

        /// <summary>Gets trade tick records for a single symbol.</summary>
        public Task<TradeRecordDbn[]> GetTradesAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetTradesAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets trade tick records for multiple symbols.</summary>
        public async Task<TradeRecordDbn[]> GetTradesAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Trades, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeTradesDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTradesAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TradeRecordDbn[] GetTrades(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTradesAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTradesAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TradeRecordDbn[] GetTrades(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTradesAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // MBP-1 (schema=mbp-1, rtype=Mbp1) — top-of-book with bid/ask level
        // =====================================================================================

        /// <summary>Gets MBP-1 top-of-book records for a single symbol.</summary>
        public Task<Mbp1RecordDbn[]> GetMbp1Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetMbp1Async(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets MBP-1 top-of-book records for multiple symbols.</summary>
        public async Task<Mbp1RecordDbn[]> GetMbp1Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Mbp1, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeMbp1Dbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Mbp1RecordDbn[] GetMbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMbp1Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Mbp1RecordDbn[] GetMbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMbp1Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Core fetch: CBBO
        // =====================================================================================

        private async Task<CbboRecordDbn[]> GetCbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            string schema,
            CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }
            if(string.IsNullOrWhiteSpace(schema)) { throw new ArgumentException("schema required", nameof(schema)); }

            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeCbboDbn(stream).ToArray();
            }
        }

        // =====================================================================================
        // HTTP transport with retry
        // =====================================================================================

        private async Task<Stream> SendGetAsync(string path, CancellationToken cancellationToken)
        {
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

                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                return new MemoryStream(bytes, writable: false);
            }
            finally
            {
                response.Dispose();
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

            if(body != null && body.Length > 512)
            {
                body = string.Concat(body.AsSpan(0, 512), "...");
            }

            string errorCase = null;
            if(!string.IsNullOrEmpty(body))
            {
                int caseIdx = body.IndexOf("\"case\"", StringComparison.Ordinal);
                if(caseIdx >= 0)
                {
                    int colonIdx = body.IndexOf(':', caseIdx);
                    if(colonIdx >= 0)
                    {
                        int quoteStart = body.IndexOf('"', colonIdx + 1);
                        if(quoteStart >= 0)
                        {
                            int quoteEnd = body.IndexOf('"', quoteStart + 1);
                            if(quoteEnd > quoteStart)
                            {
                                errorCase = body.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                            }
                        }
                    }
                }
            }

            throw new DatabentoHttpException((int)response.StatusCode, body ?? string.Empty, errorCase);
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

                symbolList.Append(symbols[i]);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("timeseries.get_range");
            sb.Append("?dataset=").Append(Uri.EscapeDataString(dataset));
            sb.Append("&schema=").Append(Uri.EscapeDataString(schema));
            sb.Append("&symbols=").Append(Uri.EscapeDataString(symbolList.ToString()));
            sb.Append("&start=").Append(Uri.EscapeDataString(start));
            sb.Append("&end=").Append(Uri.EscapeDataString(end));
            sb.Append("&stype_in=").Append(Uri.EscapeDataString(stypeIn ?? "raw_symbol"));
            sb.Append("&encoding=dbn&compression=none");
            return sb.ToString();
        }

        // =====================================================================================
        // DBN decode: CBBO
        // =====================================================================================

        private static List<CbboRecordDbn> DecodeCbboDbn(Stream stream)
        {
            List<CbboRecordDbn> list = new List<CbboRecordDbn>();
            using(BinaryReader reader = new BinaryReader(stream))
            {
                DbnMetadata metadata = DbnMetadata.FromBinaryReader(reader);
                if(metadata == null)
                {
                    throw new InvalidDataException("Invalid DBN stream: missing metadata header.");
                }

                while(true)
                {
                    DbnRecordHeader header = DbnRecordHeader.ReadFromBytes(reader);
                    if(header == null)
                    {
                        break;
                    }

                    if(header.RecordType != RType.Cbbo1S && header.RecordType != RType.Cbbo1M)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in CBBO stream.", (byte)header.RecordType));
                    }

                    list.Add(CbboRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        // =====================================================================================
        // DBN decode: Trades (rtype=Mbp0)
        // =====================================================================================

        private static List<TradeRecordDbn> DecodeTradesDbn(Stream stream)
        {
            List<TradeRecordDbn> list = new List<TradeRecordDbn>();
            using(BinaryReader reader = new BinaryReader(stream))
            {
                DbnMetadata metadata = DbnMetadata.FromBinaryReader(reader);
                if(metadata == null)
                {
                    throw new InvalidDataException("Invalid DBN stream: missing metadata header.");
                }

                while(true)
                {
                    DbnRecordHeader header = DbnRecordHeader.ReadFromBytes(reader);
                    if(header == null)
                    {
                        break;
                    }

                    if(header.RecordType != RType.Mbp0)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in Trades stream.", (byte)header.RecordType));
                    }

                    list.Add(TradeRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        // =====================================================================================
        // DBN decode: MBP-1 (rtype=Mbp1)
        // =====================================================================================

        private static List<Mbp1RecordDbn> DecodeMbp1Dbn(Stream stream)
        {
            List<Mbp1RecordDbn> list = new List<Mbp1RecordDbn>();
            using(BinaryReader reader = new BinaryReader(stream))
            {
                DbnMetadata metadata = DbnMetadata.FromBinaryReader(reader);
                if(metadata == null)
                {
                    throw new InvalidDataException("Invalid DBN stream: missing metadata header.");
                }

                while(true)
                {
                    DbnRecordHeader header = DbnRecordHeader.ReadFromBytes(reader);
                    if(header == null)
                    {
                        break;
                    }

                    if(header.RecordType != RType.Mbp1)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in MBP-1 stream.", (byte)header.RecordType));
                    }

                    list.Add(Mbp1RecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }
    }
}
