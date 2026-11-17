// Ignore Spelling: Api

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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
                HttpClientHandler handler = this.options.RequestCompressedResponses
                    ? HttpCompression.CreateDecompressingHandler()
                    : HttpCompression.CreatePlainHandler();

                HttpClient httpClient = new HttpClient(handler, disposeHandler: true);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Trades, startUtc, endUtc, DbnRecordLayouts.Trades, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Mbp1, startUtc, endUtc, DbnRecordLayouts.Mbp1, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Mbo, startUtc, endUtc, DbnRecordLayouts.Mbo, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Mbp10, startUtc, endUtc, DbnRecordLayouts.Mbp10, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Tbbo, startUtc, endUtc, DbnRecordLayouts.Tbbo, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Tcbbo, startUtc, endUtc, DbnRecordLayouts.Tcbbo, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Cmbp1, startUtc, endUtc, DbnRecordLayouts.Cmbp1, cancellationToken).ConfigureAwait(false);
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
        public OhlcvRecordDbn[] GetOhlcvEod(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.GetOhlcvEodAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>Synchronous convenience wrapper for <see cref="GetOhlcvEodAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Statistics, startUtc, endUtc, DbnRecordLayouts.Statistics, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Definition, startUtc, endUtc, DbnRecordLayouts.Definitions, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Status, startUtc, endUtc, DbnRecordLayouts.Status, cancellationToken).ConfigureAwait(false);
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
            return await this.GetRecordsAsync(dataset, symbols, Schema.Imbalance, startUtc, endUtc, DbnRecordLayouts.Imbalance, cancellationToken).ConfigureAwait(false);
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
        // Streaming: every schema, one record at a time
        // =====================================================================================

        /// <summary>
        /// Streams the records <see cref="GetCbbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<CbboRecordDbn> StreamCbbo1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<CbboRecordDbn> StreamCbbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCbbo1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetCbbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<CbboRecordDbn> StreamCbbo1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<CbboRecordDbn> StreamCbbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCbbo1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetCbbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<CbboRecordDbn> StreamCbbo1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Min, startUtc, endUtc, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<CbboRecordDbn> StreamCbbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCbbo1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetCbbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<CbboRecordDbn> StreamCbbo1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.ConsolidatedBBO1Min, startUtc, endUtc, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<CbboRecordDbn> StreamCbbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCbbo1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTradesAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TradeRecordDbn> StreamTradesAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Trades, startUtc, endUtc, DbnRecordLayouts.Trades, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTradesAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TradeRecordDbn> StreamTrades(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTradesAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTradesAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TradeRecordDbn> StreamTradesAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Trades, startUtc, endUtc, DbnRecordLayouts.Trades, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTradesAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TradeRecordDbn> StreamTrades(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTradesAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Mbp1RecordDbn> StreamMbp1Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Mbp1, startUtc, endUtc, DbnRecordLayouts.Mbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Mbp1RecordDbn> StreamMbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMbp1Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Mbp1RecordDbn> StreamMbp1Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Mbp1, startUtc, endUtc, DbnRecordLayouts.Mbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Mbp1RecordDbn> StreamMbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMbp1Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<MboRecordDbn> StreamMboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Mbo, startUtc, endUtc, DbnRecordLayouts.Mbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<MboRecordDbn> StreamMbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<MboRecordDbn> StreamMboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Mbo, startUtc, endUtc, DbnRecordLayouts.Mbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<MboRecordDbn> StreamMbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMbp10Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Mbp10RecordDbn> StreamMbp10Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Mbp10, startUtc, endUtc, DbnRecordLayouts.Mbp10, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMbp10Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Mbp10RecordDbn> StreamMbp10(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMbp10Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetMbp10Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Mbp10RecordDbn> StreamMbp10Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Mbp10, startUtc, endUtc, DbnRecordLayouts.Mbp10, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamMbp10Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Mbp10RecordDbn> StreamMbp10(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamMbp10Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TbboRecordDbn> StreamTbboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Tbbo, startUtc, endUtc, DbnRecordLayouts.Tbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TbboRecordDbn> StreamTbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTbboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TbboRecordDbn> StreamTbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Tbbo, startUtc, endUtc, DbnRecordLayouts.Tbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TbboRecordDbn> StreamTbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTbboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetBbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<BboRecordDbn> StreamBbo1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Sec, startUtc, endUtc, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<BboRecordDbn> StreamBbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamBbo1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetBbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<BboRecordDbn> StreamBbo1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Bbo1Sec, startUtc, endUtc, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<BboRecordDbn> StreamBbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamBbo1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetBbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<BboRecordDbn> StreamBbo1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Min, startUtc, endUtc, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<BboRecordDbn> StreamBbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamBbo1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetBbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<BboRecordDbn> StreamBbo1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Bbo1Min, startUtc, endUtc, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<BboRecordDbn> StreamBbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamBbo1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTcbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TcbboRecordDbn> StreamTcbboAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Tcbbo, startUtc, endUtc, DbnRecordLayouts.Tcbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTcbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TcbboRecordDbn> StreamTcbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTcbboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetTcbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<TcbboRecordDbn> StreamTcbboAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Tcbbo, startUtc, endUtc, DbnRecordLayouts.Tcbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamTcbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<TcbboRecordDbn> StreamTcbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamTcbboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetCmbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Cmbp1RecordDbn> StreamCmbp1Async(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Cmbp1, startUtc, endUtc, DbnRecordLayouts.Cmbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCmbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Cmbp1RecordDbn> StreamCmbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCmbp1Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetCmbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<Cmbp1RecordDbn> StreamCmbp1Async(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Cmbp1, startUtc, endUtc, DbnRecordLayouts.Cmbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamCmbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<Cmbp1RecordDbn> StreamCmbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamCmbp1Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1sAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Sec, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1sAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Ohlcv1Sec, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1mAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Min, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1mAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Ohlcv1Min, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1hAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1hAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Hour, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1hAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1h(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1hAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1hAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1hAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Ohlcv1Hour, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1hAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1h(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1hAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1dAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1dAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Day, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1dAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1d(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1dAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1dAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<OhlcvRecordDbn> StreamOhlcv1dAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Ohlcv1Day, startUtc, endUtc, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1dAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<OhlcvRecordDbn> StreamOhlcv1d(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamOhlcv1dAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetStatisticsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<StatisticsRecordDbn> StreamStatisticsAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Statistics, startUtc, endUtc, DbnRecordLayouts.Statistics, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamStatisticsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<StatisticsRecordDbn> StreamStatistics(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamStatisticsAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetStatisticsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<StatisticsRecordDbn> StreamStatisticsAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Statistics, startUtc, endUtc, DbnRecordLayouts.Statistics, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamStatisticsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<StatisticsRecordDbn> StreamStatistics(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamStatisticsAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetDefinitionsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<DefinitionRecordDbn> StreamDefinitionsAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Definition, startUtc, endUtc, DbnRecordLayouts.Definitions, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamDefinitionsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<DefinitionRecordDbn> StreamDefinitions(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamDefinitionsAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetDefinitionsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<DefinitionRecordDbn> StreamDefinitionsAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Definition, startUtc, endUtc, DbnRecordLayouts.Definitions, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamDefinitionsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<DefinitionRecordDbn> StreamDefinitions(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamDefinitionsAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetStatusAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<StatusRecordDbn> StreamStatusAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Status, startUtc, endUtc, DbnRecordLayouts.Status, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamStatusAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<StatusRecordDbn> StreamStatus(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamStatusAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetStatusAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<StatusRecordDbn> StreamStatusAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Status, startUtc, endUtc, DbnRecordLayouts.Status, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamStatusAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<StatusRecordDbn> StreamStatus(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamStatusAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetImbalanceAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<ImbalanceRecordDbn> StreamImbalanceAsync(
            string dataset,
            string symbol,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, new[] { symbol }, Schema.Imbalance, startUtc, endUtc, DbnRecordLayouts.Imbalance, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamImbalanceAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<ImbalanceRecordDbn> StreamImbalance(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamImbalanceAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();
        }

        /// <summary>
        /// Streams the records <see cref="GetImbalanceAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a time as they are decoded from the response. Only the
        /// HTTP status is retried; a failure while reading the body is thrown. Each enumeration sends a new request.
        /// </summary>
        public IAsyncEnumerable<ImbalanceRecordDbn> StreamImbalanceAsync(
            string dataset,
            IReadOnlyList<string> symbols,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
        {
            return this.StreamRecordsAsync(dataset, symbols, Schema.Imbalance, startUtc, endUtc, DbnRecordLayouts.Imbalance, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="StreamImbalanceAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        public IEnumerable<ImbalanceRecordDbn> StreamImbalance(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return this.StreamImbalanceAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
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
