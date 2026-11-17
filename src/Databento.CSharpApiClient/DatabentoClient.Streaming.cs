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
    }
}
