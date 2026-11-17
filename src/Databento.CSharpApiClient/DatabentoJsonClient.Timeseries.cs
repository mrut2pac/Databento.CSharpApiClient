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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        [Obsolete("No Databento dataset serves the ohlcv-eod schema, so this always fails. Use GetOhlcv1d.")]
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
        // Timeseries — MBO
        // ================================================================

        /// <summary>Returns Market-by-Order (L3) records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<MboRecordJson[]> GetMboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbo, startUtc, endUtc, DeserializeMboJson, ct);

        /// <summary>Returns Market-by-Order (L3) records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<MboRecordJson[]> GetMboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Mbo, startUtc, endUtc, DeserializeMboJson, ct);

        /// <summary>Returns Market-by-Order (L3) records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public MboRecordJson[] GetMbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns Market-by-Order (L3) records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public MboRecordJson[] GetMbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — MBP-10
        // ================================================================

        /// <summary>Returns Market-by-Price depth-10 records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Mbp10RecordJson[]> GetMbp10Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Mbp10, startUtc, endUtc, DeserializeMbp10Json, ct);

        /// <summary>Returns Market-by-Price depth-10 records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Mbp10RecordJson[]> GetMbp10Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Mbp10, startUtc, endUtc, DeserializeMbp10Json, ct);

        /// <summary>Returns Market-by-Price depth-10 records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Mbp10RecordJson[] GetMbp10(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMbp10Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns Market-by-Price depth-10 records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Mbp10RecordJson[] GetMbp10(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetMbp10Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — BBO (venue-local)
        // ================================================================

        /// <summary>Returns 1-second venue-local BBO snapshots for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<BboRecordJson[]> GetBbo1sAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Sec, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Returns 1-minute venue-local BBO snapshots for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<BboRecordJson[]> GetBbo1mAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Bbo1Min, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Returns 1-second venue-local BBO snapshots for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<BboRecordJson[]> GetBbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Bbo1Sec, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Returns 1-minute venue-local BBO snapshots for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        /// <param name="ct">Cancellation token.</param>
        public Task<BboRecordJson[]> GetBbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Bbo1Min, startUtc, endUtc, DeserializeBboJson, ct);

        /// <summary>Returns 1-second venue-local BBO snapshots for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public BboRecordJson[] GetBbo1s(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetBbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute venue-local BBO snapshots for a single symbol.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbol">Instrument symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public BboRecordJson[] GetBbo1m(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetBbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-second venue-local BBO snapshots for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public BboRecordJson[] GetBbo1s(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetBbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns 1-minute venue-local BBO snapshots for multiple symbols.</summary>
        /// <param name="dataset">Dataset identifier.</param><param name="symbols">Instrument symbols.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public BboRecordJson[] GetBbo1m(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetBbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — TBBO (Trade + venue BBO)
        // ================================================================

        /// <summary>Returns Trade + venue BBO records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TbboRecordJson[]> GetTbboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Tbbo, startUtc, endUtc, DeserializeTbboJson, ct);

        /// <summary>Returns Trade + venue BBO records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TbboRecordJson[]> GetTbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Tbbo, startUtc, endUtc, DeserializeTbboJson, ct);

        /// <summary>Returns Trade + venue BBO records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TbboRecordJson[] GetTbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTbboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns Trade + venue BBO records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TbboRecordJson[] GetTbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTbboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — TCBBO (Trade + Consolidated BBO)
        // ================================================================

        /// <summary>Returns Trade + Consolidated BBO records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TcbboRecordJson[]> GetTcbboAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Tcbbo, startUtc, endUtc, DeserializeTcbboJson, ct);

        /// <summary>Returns Trade + Consolidated BBO records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<TcbboRecordJson[]> GetTcbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Tcbbo, startUtc, endUtc, DeserializeTcbboJson, ct);

        /// <summary>Returns Trade + Consolidated BBO records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TcbboRecordJson[] GetTcbbo(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTcbboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns Trade + Consolidated BBO records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public TcbboRecordJson[] GetTcbbo(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetTcbboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — CMBP-1 (Consolidated MBP depth-1)
        // ================================================================

        /// <summary>Returns Consolidated MBP depth-1 records for a single symbol. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Cmbp1RecordJson[]> GetCmbp1Async(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Cmbp1, startUtc, endUtc, DeserializeCmbp1Json, ct);

        /// <summary>Returns Consolidated MBP depth-1 records for multiple symbols. Filters on <c>ts_recv</c>.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<Cmbp1RecordJson[]> GetCmbp1Async(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Cmbp1, startUtc, endUtc, DeserializeCmbp1Json, ct);

        /// <summary>Returns Consolidated MBP depth-1 records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Cmbp1RecordJson[] GetCmbp1(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCmbp1Async(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns Consolidated MBP depth-1 records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public Cmbp1RecordJson[] GetCmbp1(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetCmbp1Async(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — Status
        // ================================================================

        /// <summary>Returns trading-status / halt records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<StatusRecordJson[]> GetStatusAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Status, startUtc, endUtc, DeserializeStatusJson, ct);

        /// <summary>Returns trading-status / halt records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<StatusRecordJson[]> GetStatusAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Status, startUtc, endUtc, DeserializeStatusJson, ct);

        /// <summary>Returns trading-status / halt records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public StatusRecordJson[] GetStatus(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetStatusAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns trading-status / halt records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public StatusRecordJson[] GetStatus(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetStatusAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — Imbalance
        // ================================================================

        /// <summary>Returns auction imbalance records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<ImbalanceRecordJson[]> GetImbalanceAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.Imbalance, startUtc, endUtc, DeserializeImbalanceJson, ct);

        /// <summary>Returns auction imbalance records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        public Task<ImbalanceRecordJson[]> GetImbalanceAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.Imbalance, startUtc, endUtc, DeserializeImbalanceJson, ct);

        /// <summary>Returns auction imbalance records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public ImbalanceRecordJson[] GetImbalance(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetImbalanceAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns auction imbalance records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        public ImbalanceRecordJson[] GetImbalance(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetImbalanceAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();

        // ================================================================
        // Timeseries — SymbolMapping
        // ================================================================

        /// <summary>Returns symbol-mapping records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        [Obsolete("timeseries.get_range has no symbol_mapping schema, so this always fails with 400. Use ResolveSymbols for symbol mappings.")]
        public Task<SymbolMappingRecordJson[]> GetSymbolMappingsAsync(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, new[] { symbol }, Schema.SymbolMapping, startUtc, endUtc, DeserializeSymbolMappingJson, ct);

        /// <summary>Returns symbol-mapping records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param><param name="ct">Cancellation token.</param>
        [Obsolete("timeseries.get_range has no symbol_mapping schema, so this always fails with 400. Use ResolveSymbols for symbol mappings.")]
        public Task<SymbolMappingRecordJson[]> GetSymbolMappingsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
            => this.GetGenericRecordsAsync(dataset, symbols, Schema.SymbolMapping, startUtc, endUtc, DeserializeSymbolMappingJson, ct);

        /// <summary>Returns symbol-mapping records for a single symbol.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbol">Instrument raw symbol.</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        [Obsolete("timeseries.get_range has no symbol_mapping schema, so this always fails with 400. Use ResolveSymbols for symbol mappings.")]
        public SymbolMappingRecordJson[] GetSymbolMappings(string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetSymbolMappingsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();

        /// <summary>Returns symbol-mapping records for multiple symbols.</summary>
        /// <param name="dataset">The dataset code.</param><param name="symbols">Instrument raw symbols (up to 2,000).</param>
        /// <param name="startUtc">Inclusive range start (UTC).</param><param name="endUtc">Exclusive range end (UTC).</param>
        [Obsolete("timeseries.get_range has no symbol_mapping schema, so this always fails with 400. Use ResolveSymbols for symbol mappings.")]
        public SymbolMappingRecordJson[] GetSymbolMappings(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => this.GetSymbolMappingsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
    }
}
