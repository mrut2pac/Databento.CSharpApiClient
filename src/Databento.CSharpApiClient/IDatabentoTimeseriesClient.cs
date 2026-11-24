// Ignore Spelling: Api Databento Bbo Cbbo Tcbbo Ohlcv

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// The time-series calls whose records have a record interface (CBBO, BBO, TCBBO, trades, OHLCV and statistics), returning the
    /// interfaces. Code written against it takes either client, or another implementation such as a cache, chosen by configuration or
    /// replaced by a fake in tests. <see cref="DatabentoTimeseriesClientExtensions"/> adds the one-symbol and synchronous forms of each
    /// call, so an implementation writes only these.
    /// </summary>
    /// <remarks>
    /// <see cref="DatabentoJsonClient"/> and <see cref="DatabentoClient"/> implement it explicitly; their own methods still return their
    /// record classes. Each call returns a new array the caller owns and may change, so an implementation that keeps records, such as a
    /// cache, must not hand out an array it holds. The interface is disposable because whoever creates an implementation owns it; code
    /// that receives one from elsewhere does not dispose it. A call added in a later version comes with a default implementation that
    /// throws <see cref="NotSupportedException"/>, so implementations written against this version keep compiling.
    /// </remarks>
    public interface IDatabentoTimeseriesClient : IDisposable
    {
        /// <summary>
        /// Gets consolidated BBO 1-second snapshots for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<ICbboRecord[]> GetCbbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets consolidated BBO 1-minute snapshots for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<ICbboRecord[]> GetCbbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets BBO 1-second snapshots for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IBboRecord[]> GetBbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets BBO 1-minute snapshots for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IBboRecord[]> GetBbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets trades, each with the consolidated BBO just before it, for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<ITcbboRecord[]> GetTcbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets trades for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<ITradeRecord[]> GetTradesAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets 1-second OHLCV bars for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IOhlcvRecord[]> GetOhlcv1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets 1-minute OHLCV bars for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IOhlcvRecord[]> GetOhlcv1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets 1-hour OHLCV bars for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IOhlcvRecord[]> GetOhlcv1hAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets daily OHLCV bars for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IOhlcvRecord[]> GetOhlcv1dAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics, such as settlement prices and open interest, for several symbols.
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        Task<IStatisticsRecord[]> GetStatisticsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default);
    }
}
