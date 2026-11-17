// Ignore Spelling: Api Databento Json

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;

namespace Databento.CSharpApiClient
{
    public sealed partial class DatabentoJsonClient
    {
        // ================================================================
        // Timeseries — streaming, one record at a time
        // ================================================================

        /// <summary>
        /// Streams the records <see cref="GetCbbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<CbboRecordJson> StreamCbbo1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, DeserializeCbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<CbboRecordJson> StreamCbbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCbbo1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetCbbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<CbboRecordJson> StreamCbbo1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.ConsolidatedBBO1Min, startUtc, endUtc, DeserializeCbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<CbboRecordJson> StreamCbbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCbbo1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetCbbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<CbboRecordJson> StreamCbbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.ConsolidatedBBO1Sec, startUtc, endUtc, DeserializeCbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<CbboRecordJson> StreamCbbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCbbo1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetCbbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<CbboRecordJson> StreamCbbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.ConsolidatedBBO1Min, startUtc, endUtc, DeserializeCbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCbbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<CbboRecordJson> StreamCbbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCbbo1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Sec, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Min, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1hAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1hAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Hour, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1hAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1h(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1hAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1dAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1dAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Ohlcv1Day, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1dAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1d(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1dAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Ohlcv1Sec, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Ohlcv1Min, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1hAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1hAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Ohlcv1Hour, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1hAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1h(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1hAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetOhlcv1dAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<OhlcvRecordJson> StreamOhlcv1dAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Ohlcv1Day, startUtc, endUtc, DeserializeOhlcvJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamOhlcv1dAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<OhlcvRecordJson> StreamOhlcv1d(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamOhlcv1dAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTradesAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TradeRecordJson> StreamTradesAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Trades, startUtc, endUtc, DeserializeTradesJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTradesAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TradeRecordJson> StreamTrades(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTradesAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTradesAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TradeRecordJson> StreamTradesAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Trades, startUtc, endUtc, DeserializeTradesJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTradesAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TradeRecordJson> StreamTrades(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTradesAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Mbp1RecordJson> StreamMbp1Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbp1, startUtc, endUtc, DeserializeMbp1Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Mbp1RecordJson> StreamMbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMbp1Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Mbp1RecordJson> StreamMbp1Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Mbp1, startUtc, endUtc, DeserializeMbp1Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Mbp1RecordJson> StreamMbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMbp1Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetStatisticsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<StatisticsRecordJson> StreamStatisticsAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Statistics, startUtc, endUtc, DeserializeStatisticsJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamStatisticsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<StatisticsRecordJson> StreamStatistics(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamStatisticsAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetStatisticsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<StatisticsRecordJson> StreamStatisticsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Statistics, startUtc, endUtc, DeserializeStatisticsJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamStatisticsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<StatisticsRecordJson> StreamStatistics(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamStatisticsAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetDefinitionsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<DefinitionRecordJson> StreamDefinitionsAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Definition, startUtc, endUtc, DeserializeDefinitionsJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamDefinitionsAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<DefinitionRecordJson> StreamDefinitions(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamDefinitionsAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetDefinitionsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<DefinitionRecordJson> StreamDefinitionsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Definition, startUtc, endUtc, DeserializeDefinitionsJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamDefinitionsAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<DefinitionRecordJson> StreamDefinitions(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamDefinitionsAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<MboRecordJson> StreamMboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbo, startUtc, endUtc, DeserializeMboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<MboRecordJson> StreamMbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<MboRecordJson> StreamMboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Mbo, startUtc, endUtc, DeserializeMboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<MboRecordJson> StreamMbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMbp10Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Mbp10RecordJson> StreamMbp10Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbp10, startUtc, endUtc, DeserializeMbp10Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMbp10Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Mbp10RecordJson> StreamMbp10(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMbp10Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetMbp10Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Mbp10RecordJson> StreamMbp10Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Mbp10, startUtc, endUtc, DeserializeMbp10Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamMbp10Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Mbp10RecordJson> StreamMbp10(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamMbp10Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetBbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<BboRecordJson> StreamBbo1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Sec, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1sAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<BboRecordJson> StreamBbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamBbo1sAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetBbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<BboRecordJson> StreamBbo1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Min, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1mAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<BboRecordJson> StreamBbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamBbo1mAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetBbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<BboRecordJson> StreamBbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Bbo1Sec, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1sAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<BboRecordJson> StreamBbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamBbo1sAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetBbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<BboRecordJson> StreamBbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Bbo1Min, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamBbo1mAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<BboRecordJson> StreamBbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamBbo1mAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TbboRecordJson> StreamTbboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Tbbo, startUtc, endUtc, DeserializeTbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TbboRecordJson> StreamTbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTbboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TbboRecordJson> StreamTbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Tbbo, startUtc, endUtc, DeserializeTbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TbboRecordJson> StreamTbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTbboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTcbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TcbboRecordJson> StreamTcbboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Tcbbo, startUtc, endUtc, DeserializeTcbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTcbboAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TcbboRecordJson> StreamTcbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTcbboAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetTcbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<TcbboRecordJson> StreamTcbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Tcbbo, startUtc, endUtc, DeserializeTcbboJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamTcbboAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<TcbboRecordJson> StreamTcbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamTcbboAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetCmbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Cmbp1RecordJson> StreamCmbp1Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Cmbp1, startUtc, endUtc, DeserializeCmbp1Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCmbp1Async(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Cmbp1RecordJson> StreamCmbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCmbp1Async(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetCmbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<Cmbp1RecordJson> StreamCmbp1Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Cmbp1, startUtc, endUtc, DeserializeCmbp1Json, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamCmbp1Async(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<Cmbp1RecordJson> StreamCmbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamCmbp1Async(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetStatusAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<StatusRecordJson> StreamStatusAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Status, startUtc, endUtc, DeserializeStatusJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamStatusAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<StatusRecordJson> StreamStatus(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamStatusAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetStatusAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<StatusRecordJson> StreamStatusAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Status, startUtc, endUtc, DeserializeStatusJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamStatusAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<StatusRecordJson> StreamStatus(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamStatusAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetImbalanceAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<ImbalanceRecordJson> StreamImbalanceAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, new[] { symbol }, Schema.Imbalance, startUtc, endUtc, DeserializeImbalanceJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamImbalanceAsync(string,string,DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<ImbalanceRecordJson> StreamImbalance(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamImbalanceAsync(dataset, symbol, startUtc, endUtc).ToBlockingEnumerable();

        /// <summary>
        /// Streams the records <see cref="GetImbalanceAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/> returns, one at a
        /// time as they are read from the response. Only the HTTP status is retried; a failure while reading the body is thrown. Each
        /// enumeration sends a new request.
        /// </summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public IAsyncEnumerable<ImbalanceRecordJson> StreamImbalanceAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.StreamGenericRecordsAsync(dataset, symbols, Schema.Imbalance, startUtc, endUtc, DeserializeImbalanceJson, ct);

        /// <summary>Synchronous counterpart of <see cref="StreamImbalanceAsync(string,IReadOnlyList{string},DateTimeOffset,DateTimeOffset,CancellationToken)"/>.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public IEnumerable<ImbalanceRecordJson> StreamImbalance(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.StreamImbalanceAsync(dataset, symbols, startUtc, endUtc).ToBlockingEnumerable();
    }
}
