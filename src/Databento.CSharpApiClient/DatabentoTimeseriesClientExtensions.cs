// Ignore Spelling: Api Databento Bbo Cbbo Tcbbo Ohlcv

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// The one-symbol and synchronous forms of the <see cref="IDatabentoTimeseriesClient"/> calls. A one-symbol call asks for a list of
    /// that one symbol, which both clients send exactly as their own one-symbol calls; on a client's own type its methods are called
    /// instead of these.
    /// </summary>
    public static class DatabentoTimeseriesClientExtensions
    {
        /// <summary>
        /// Gets consolidated BBO 1-second snapshots for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<ICbboRecord[]> GetCbbo1sAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetCbbo1sAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets consolidated BBO 1-second snapshots for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ICbboRecord[] GetCbbo1s(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetCbbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets consolidated BBO 1-second snapshots for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ICbboRecord[] GetCbbo1s(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetCbbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets consolidated BBO 1-minute snapshots for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<ICbboRecord[]> GetCbbo1mAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetCbbo1mAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets consolidated BBO 1-minute snapshots for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ICbboRecord[] GetCbbo1m(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetCbbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets consolidated BBO 1-minute snapshots for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ICbboRecord[] GetCbbo1m(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetCbbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets BBO 1-second snapshots for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IBboRecord[]> GetBbo1sAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetBbo1sAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets BBO 1-second snapshots for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IBboRecord[] GetBbo1s(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetBbo1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets BBO 1-second snapshots for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IBboRecord[] GetBbo1s(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetBbo1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets BBO 1-minute snapshots for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IBboRecord[]> GetBbo1mAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetBbo1mAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets BBO 1-minute snapshots for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IBboRecord[] GetBbo1m(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetBbo1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets BBO 1-minute snapshots for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IBboRecord[] GetBbo1m(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetBbo1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets trades, each with the consolidated BBO just before it, for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<ITcbboRecord[]> GetTcbboAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetTcbboAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets trades, each with the consolidated BBO just before it, for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ITcbboRecord[] GetTcbbo(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetTcbboAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets trades, each with the consolidated BBO just before it, for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ITcbboRecord[] GetTcbbo(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetTcbboAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets trades for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<ITradeRecord[]> GetTradesAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetTradesAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets trades for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ITradeRecord[] GetTrades(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetTradesAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets trades for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static ITradeRecord[] GetTrades(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetTradesAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-second OHLCV bars for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IOhlcvRecord[]> GetOhlcv1sAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1sAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets 1-second OHLCV bars for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1s(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetOhlcv1sAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-second OHLCV bars for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1s(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1sAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-minute OHLCV bars for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IOhlcvRecord[]> GetOhlcv1mAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1mAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets 1-minute OHLCV bars for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1m(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetOhlcv1mAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-minute OHLCV bars for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1m(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1mAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-hour OHLCV bars for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IOhlcvRecord[]> GetOhlcv1hAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1hAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets 1-hour OHLCV bars for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1h(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetOhlcv1hAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets 1-hour OHLCV bars for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1h(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1hAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets daily OHLCV bars for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IOhlcvRecord[]> GetOhlcv1dAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1dAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets daily OHLCV bars for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1d(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetOhlcv1dAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets daily OHLCV bars for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IOhlcvRecord[] GetOhlcv1d(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetOhlcv1dAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets statistics, such as settlement prices and open interest, for one symbol.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static Task<IStatisticsRecord[]> GetStatisticsAsync(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetStatisticsAsync(dataset, new[] { symbol }, startUtc, endUtc, cancellationToken);
        }

        /// <summary>
        /// Gets statistics, such as settlement prices and open interest, for one symbol, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbol">Databento raw symbol.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IStatisticsRecord[] GetStatistics(this IDatabentoTimeseriesClient client, string dataset, string symbol, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            return client.GetStatisticsAsync(dataset, symbol, startUtc, endUtc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets statistics, such as settlement prices and open interest, for several symbols, synchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c>.</param>
        /// <param name="symbols">Databento raw symbols, or <c>ALL_SYMBOLS</c> alone for every symbol of the dataset.</param>
        /// <param name="startUtc">Start of the range, inclusive.</param>
        /// <param name="endUtc">End of the range, exclusive.</param>
        /// <returns>The range's records: a new array the caller owns, empty when there are none, never null.</returns>
        public static IStatisticsRecord[] GetStatistics(this IDatabentoTimeseriesClient client, string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetStatisticsAsync(dataset, symbols, startUtc, endUtc).GetAwaiter().GetResult();
        }
    }
}
