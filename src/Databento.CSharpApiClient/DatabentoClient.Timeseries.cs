// Ignore Spelling: Api

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoClient
    {
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
    }
}
