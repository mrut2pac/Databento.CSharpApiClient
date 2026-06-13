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
    /// Covers <c>timeseries.get_range</c> for all supported DBN schemas.
    /// </summary>
    public sealed class DatabentoClient : IDisposable
    {
        private readonly DatabentoOptions options;
        private readonly IHttpTransport transport;

        /// <summary>
        /// Initialises a new <see cref="DatabentoClient"/> using the supplied options.
        /// A default <see cref="HttpClient"/> with Basic authentication is created unless a custom
        /// <paramref name="transport"/> is provided (useful for unit testing).
        /// </summary>
        /// <param name="options">Connection and authentication settings.</param>
        /// <param name="transport">Optional custom HTTP transport; leave <see langword="null"/> to use the built-in client.</param>
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

        /// <summary>Releases the underlying HTTP transport (and its <see cref="HttpClient"/>).</summary>
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
        // MBP-1 (schema=mbp-1, rtype=Mbp1)
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
        // MBO (schema=mbo, rtype=Mbo)
        // =====================================================================================

        /// <summary>Gets Market-by-Order (Level 3) records for a single symbol.</summary>
        public Task<MboRecordDbn[]> GetMboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetMboAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets Market-by-Order (Level 3) records for multiple symbols.</summary>
        public async Task<MboRecordDbn[]> GetMboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Mbo, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeMboDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public MboRecordDbn[] GetMbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public MboRecordDbn[] GetMbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // MBP-10 (schema=mbp-10, rtype=Mbp10)
        // =====================================================================================

        /// <summary>Gets Market-by-Price depth-10 records for a single symbol.</summary>
        public Task<Mbp10RecordDbn[]> GetMbp10Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetMbp10Async(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets Market-by-Price depth-10 records for multiple symbols.</summary>
        public async Task<Mbp10RecordDbn[]> GetMbp10Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Mbp10, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeMbp10Dbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMbp10Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Mbp10RecordDbn[] GetMbp10(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMbp10Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetMbp10Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Mbp10RecordDbn[] GetMbp10(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetMbp10Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // TBBO (schema=tbbo, rtype=Tbbo)
        // =====================================================================================

        /// <summary>Gets trade-plus-venue-BBO records for a single symbol.</summary>
        public Task<TbboRecordDbn[]> GetTbboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetTbboAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets trade-plus-venue-BBO records for multiple symbols.</summary>
        public async Task<TbboRecordDbn[]> GetTbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Tbbo, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeTbboDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TbboRecordDbn[] GetTbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTbboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TbboRecordDbn[] GetTbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTbboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // BBO 1-second / 1-minute (venue-local, schema=bbo-1s / bbo-1m)
        // =====================================================================================

        /// <summary>Gets venue-local BBO 1-second snapshots for a single symbol.</summary>
        public Task<BboRecordDbn[]> GetBbo1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetBboAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Bbo1Sec, cancellationToken);
        }

        /// <summary>Gets venue-local BBO 1-second snapshots for multiple symbols.</summary>
        public Task<BboRecordDbn[]> GetBbo1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetBboAsync(dataset, symbols, startUtc, endUtc, Schema.Bbo1Sec, cancellationToken);
        }

        /// <summary>Gets venue-local BBO 1-minute snapshots for a single symbol.</summary>
        public Task<BboRecordDbn[]> GetBbo1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetBboAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Bbo1Min, cancellationToken);
        }

        /// <summary>Gets venue-local BBO 1-minute snapshots for multiple symbols.</summary>
        public Task<BboRecordDbn[]> GetBbo1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetBboAsync(dataset, symbols, startUtc, endUtc, Schema.Bbo1Min, cancellationToken);
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetBbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public BboRecordDbn[] GetBbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetBbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetBbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public BboRecordDbn[] GetBbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetBbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetBbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public BboRecordDbn[] GetBbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetBbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetBbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public BboRecordDbn[] GetBbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetBbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // TCBBO (schema=tcbbo, rtype=Tcbbo)
        // =====================================================================================

        /// <summary>Gets trade-plus-consolidated-BBO records for a single symbol.</summary>
        public Task<TcbboRecordDbn[]> GetTcbboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetTcbboAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets trade-plus-consolidated-BBO records for multiple symbols.</summary>
        public async Task<TcbboRecordDbn[]> GetTcbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Tcbbo, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeTcbboDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTcbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TcbboRecordDbn[] GetTcbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTcbboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetTcbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public TcbboRecordDbn[] GetTcbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetTcbboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // CMBP-1 (schema=cmbp-1, rtype=Cmbp1)
        // =====================================================================================

        /// <summary>Gets consolidated market-by-price depth-1 records for a single symbol.</summary>
        public Task<Cmbp1RecordDbn[]> GetCmbp1Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetCmbp1Async(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets consolidated market-by-price depth-1 records for multiple symbols.</summary>
        public async Task<Cmbp1RecordDbn[]> GetCmbp1Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Cmbp1, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeCmbp1Dbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCmbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Cmbp1RecordDbn[] GetCmbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCmbp1Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetCmbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public Cmbp1RecordDbn[] GetCmbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetCmbp1Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // OHLCV — 1s / 1m / 1h / 1d / eod
        // =====================================================================================

        /// <summary>Gets OHLCV 1-second bar records for a single symbol.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Ohlcv1Sec, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-second bar records for multiple symbols.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, symbols, startUtc, endUtc, Schema.Ohlcv1Sec, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-minute bar records for a single symbol.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Ohlcv1Min, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-minute bar records for multiple symbols.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, symbols, startUtc, endUtc, Schema.Ohlcv1Min, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-hour bar records for a single symbol.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1hAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Ohlcv1Hour, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-hour bar records for multiple symbols.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1hAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, symbols, startUtc, endUtc, Schema.Ohlcv1Hour, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-day bar records for a single symbol.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1dAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.Ohlcv1Day, cancellationToken);
        }

        /// <summary>Gets OHLCV 1-day bar records for multiple symbols.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcv1dAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, symbols, startUtc, endUtc, Schema.Ohlcv1Day, cancellationToken);
        }

        /// <summary>Gets OHLCV end-of-day bar records for a single symbol.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcvEodAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, new[] { symbol }, startUtc, endUtc, Schema.OhlcvEod, cancellationToken);
        }

        /// <summary>Gets OHLCV end-of-day bar records for multiple symbols.</summary>
        public Task<OhlcvRecordDbn[]> GetOhlcvEodAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetOhlcvAsync(dataset, symbols, startUtc, endUtc, Schema.OhlcvEod, cancellationToken);
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1hAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1h(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1hAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1hAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1h(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1hAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1dAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1d(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1dAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcv1dAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcv1d(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcv1dAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcvEodAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcvEod(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcvEodAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcvEodAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public OhlcvRecordDbn[] GetOhlcvEod(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcvEodAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Statistics (schema=statistics, rtype=Statistics)
        // =====================================================================================

        /// <summary>Gets publisher-statistics records (settlement, open interest, session hi/lo) for a single symbol.</summary>
        public Task<StatisticsRecordDbn[]> GetStatisticsAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetStatisticsAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets publisher-statistics records for multiple symbols.</summary>
        public async Task<StatisticsRecordDbn[]> GetStatisticsAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Statistics, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeStatisticsDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetStatisticsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public StatisticsRecordDbn[] GetStatistics(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetStatisticsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetStatisticsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public StatisticsRecordDbn[] GetStatistics(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetStatisticsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Definition (schema=definition, rtype=InstrumentDef)
        // =====================================================================================

        /// <summary>Gets instrument-definition records for a single symbol.</summary>
        public Task<DefinitionRecordDbn[]> GetDefinitionsAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetDefinitionsAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets instrument-definition records for multiple symbols.</summary>
        public async Task<DefinitionRecordDbn[]> GetDefinitionsAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Definition, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeDefinitionsDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetDefinitionsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public DefinitionRecordDbn[] GetDefinitions(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetDefinitionsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetDefinitionsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public DefinitionRecordDbn[] GetDefinitions(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetDefinitionsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Status (schema=status, rtype=Status)
        // =====================================================================================

        /// <summary>Gets trading-status messages for a single symbol.</summary>
        public Task<StatusRecordDbn[]> GetStatusAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetStatusAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets trading-status messages for multiple symbols.</summary>
        public async Task<StatusRecordDbn[]> GetStatusAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Status, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeStatusDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetStatusAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public StatusRecordDbn[] GetStatus(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetStatusAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetStatusAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public StatusRecordDbn[] GetStatus(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetStatusAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        // =====================================================================================
        // Imbalance (schema=imbalance, rtype=Imbalance)
        // =====================================================================================

        /// <summary>Gets auction-imbalance messages for a single symbol.</summary>
        public Task<ImbalanceRecordDbn[]> GetImbalanceAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.GetImbalanceAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>Gets auction-imbalance messages for multiple symbols.</summary>
        public async Task<ImbalanceRecordDbn[]> GetImbalanceAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, Schema.Imbalance, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeImbalanceDbn(stream).ToArray();
            }
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetImbalanceAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public ImbalanceRecordDbn[] GetImbalance(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetImbalanceAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetImbalanceAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public ImbalanceRecordDbn[] GetImbalance(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetImbalanceAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

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
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeOhlcvDbn(stream).ToArray();
            }
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
            if(string.IsNullOrWhiteSpace(dataset)) { throw new ArgumentException("dataset required", nameof(dataset)); }
            if(symbols == null || symbols.Count == 0) { throw new ArgumentException("at least one symbol required", nameof(symbols)); }

            string path = BuildTimeseriesQuery(dataset, schema, symbols, startUtc, endUtc, SymbolTypes.RawSymbol);
            using(Stream stream = await this.SendGetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                return DecodeBboDbn(stream).ToArray();
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

        // =====================================================================================
        // DBN decode helpers
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

        private static List<MboRecordDbn> DecodeMboDbn(Stream stream)
        {
            List<MboRecordDbn> list = new List<MboRecordDbn>();
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

                    if(header.RecordType != RType.Mbo)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in MBO stream.", (byte)header.RecordType));
                    }

                    list.Add(MboRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<Mbp10RecordDbn> DecodeMbp10Dbn(Stream stream)
        {
            List<Mbp10RecordDbn> list = new List<Mbp10RecordDbn>();
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

                    if(header.RecordType != RType.Mbp10)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in MBP-10 stream.", (byte)header.RecordType));
                    }

                    list.Add(Mbp10RecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<TbboRecordDbn> DecodeTbboDbn(Stream stream)
        {
            List<TbboRecordDbn> list = new List<TbboRecordDbn>();
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

                    if(header.RecordType != RType.Tbbo)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in TBBO stream.", (byte)header.RecordType));
                    }

                    list.Add(TbboRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<BboRecordDbn> DecodeBboDbn(Stream stream)
        {
            List<BboRecordDbn> list = new List<BboRecordDbn>();
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

                    if(header.RecordType != RType.Bbo1S && header.RecordType != RType.Bbo1M)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in BBO stream.", (byte)header.RecordType));
                    }

                    list.Add(BboRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<TcbboRecordDbn> DecodeTcbboDbn(Stream stream)
        {
            List<TcbboRecordDbn> list = new List<TcbboRecordDbn>();
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

                    if(header.RecordType != RType.Tcbbo)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in TCBBO stream.", (byte)header.RecordType));
                    }

                    list.Add(TcbboRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<Cmbp1RecordDbn> DecodeCmbp1Dbn(Stream stream)
        {
            List<Cmbp1RecordDbn> list = new List<Cmbp1RecordDbn>();
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

                    if(header.RecordType != RType.Cmbp1)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in CMBP-1 stream.", (byte)header.RecordType));
                    }

                    list.Add(Cmbp1RecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<OhlcvRecordDbn> DecodeOhlcvDbn(Stream stream)
        {
            List<OhlcvRecordDbn> list = new List<OhlcvRecordDbn>();
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

                    if(header.RecordType != RType.Ohlcv1S
                        && header.RecordType != RType.Ohlcv1M
                        && header.RecordType != RType.Ohlcv1H
                        && header.RecordType != RType.Ohlcv1D
                        && header.RecordType != RType.OhlcvEod)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in OHLCV stream.", (byte)header.RecordType));
                    }

                    list.Add(OhlcvRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<StatisticsRecordDbn> DecodeStatisticsDbn(Stream stream)
        {
            List<StatisticsRecordDbn> list = new List<StatisticsRecordDbn>();
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

                    if(header.RecordType != RType.Statistics)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in Statistics stream.", (byte)header.RecordType));
                    }

                    list.Add(StatisticsRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<DefinitionRecordDbn> DecodeDefinitionsDbn(Stream stream)
        {
            List<DefinitionRecordDbn> list = new List<DefinitionRecordDbn>();
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

                    if(header.RecordType != RType.InstrumentDef)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in Definition stream.", (byte)header.RecordType));
                    }

                    list.Add(DefinitionRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<StatusRecordDbn> DecodeStatusDbn(Stream stream)
        {
            List<StatusRecordDbn> list = new List<StatusRecordDbn>();
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

                    if(header.RecordType != RType.Status)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in Status stream.", (byte)header.RecordType));
                    }

                    list.Add(StatusRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }

        private static List<ImbalanceRecordDbn> DecodeImbalanceDbn(Stream stream)
        {
            List<ImbalanceRecordDbn> list = new List<ImbalanceRecordDbn>();
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

                    if(header.RecordType != RType.Imbalance)
                    {
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Unexpected rtype 0x{0:X2} in Imbalance stream.", (byte)header.RecordType));
                    }

                    list.Add(ImbalanceRecordDbn.ReadFromBytes(header, reader));
                }
            }

            return list;
        }
    }
}
