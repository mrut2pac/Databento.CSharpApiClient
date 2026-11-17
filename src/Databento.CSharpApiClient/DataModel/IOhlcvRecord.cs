namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// An open-high-low-close-volume bar (<c>ohlcv-1s</c>, <c>ohlcv-1m</c>, <c>ohlcv-1h</c>, <c>ohlcv-1d</c>, <c>ohlcv-eod</c>);
    /// its event time is the bar's open. Implemented by <see cref="Json.OhlcvRecordJson"/> and <see cref="Dbn.OhlcvRecordDbn"/>.
    /// </summary>
    public interface IOhlcvRecord : IRecord
    {
        /// <summary>Gets the open price.</summary>
        double Open { get; }

        /// <summary>Gets the high price.</summary>
        double High { get; }

        /// <summary>Gets the low price.</summary>
        double Low { get; }

        /// <summary>Gets the close price.</summary>
        double Close { get; }

        /// <summary>Gets the traded volume.</summary>
        ulong Volume { get; }
    }
}
