// Ignore Spelling: Api Databento Bbo Cbbo Tcbbo Ohlcv Dbn

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient
{
    // each call copies its records into a new array of the record interface, so the caller may store any implementation in it
    public sealed partial class DatabentoClient : IDatabentoTimeseriesClient
    {
        async Task<ICbboRecord[]> IDatabentoTimeseriesClient.GetCbbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<CbboRecordDbn, ICbboRecord>(await this.GetCbbo1sAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<ICbboRecord[]> IDatabentoTimeseriesClient.GetCbbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<CbboRecordDbn, ICbboRecord>(await this.GetCbbo1mAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IBboRecord[]> IDatabentoTimeseriesClient.GetBbo1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<BboRecordDbn, IBboRecord>(await this.GetBbo1sAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IBboRecord[]> IDatabentoTimeseriesClient.GetBbo1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<BboRecordDbn, IBboRecord>(await this.GetBbo1mAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<ITcbboRecord[]> IDatabentoTimeseriesClient.GetTcbboAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<TcbboRecordDbn, ITcbboRecord>(await this.GetTcbboAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<ITradeRecord[]> IDatabentoTimeseriesClient.GetTradesAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<TradeRecordDbn, ITradeRecord>(await this.GetTradesAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IOhlcvRecord[]> IDatabentoTimeseriesClient.GetOhlcv1sAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<OhlcvRecordDbn, IOhlcvRecord>(await this.GetOhlcv1sAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IOhlcvRecord[]> IDatabentoTimeseriesClient.GetOhlcv1mAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<OhlcvRecordDbn, IOhlcvRecord>(await this.GetOhlcv1mAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IOhlcvRecord[]> IDatabentoTimeseriesClient.GetOhlcv1hAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<OhlcvRecordDbn, IOhlcvRecord>(await this.GetOhlcv1hAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IOhlcvRecord[]> IDatabentoTimeseriesClient.GetOhlcv1dAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<OhlcvRecordDbn, IOhlcvRecord>(await this.GetOhlcv1dAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }

        async Task<IStatisticsRecord[]> IDatabentoTimeseriesClient.GetStatisticsAsync(string dataset, IReadOnlyList<string> symbols, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken cancellationToken)
        {
            return Utils.AsInterfaces<StatisticsRecordDbn, IStatisticsRecord>(await this.GetStatisticsAsync(dataset, symbols, startUtc, endUtc, cancellationToken).ConfigureAwait(false));
        }
    }
}
