using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Reads DBN data the caller already has, such as a downloaded batch file, one record at a time; the record types that
    /// carry a <c>Symbol</c> get it from the data's own metadata. Zstandard-compressed data (<c>.dbn.zst</c>) must be
    /// decompressed by the caller: pass a decompressing stream from the zstd library of your choice.
    /// </summary>
    public static class DbnReader
    {
        /// <summary>Reads the consolidated BBO records (<c>cbbo-1s</c>, <c>cbbo-1m</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<CbboRecordDbn> ReadCbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadCbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<CbboRecordDbn> ReadCbbo(Stream stream)
        {
            return ReadCbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the trade records (<c>trades</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<TradeRecordDbn> ReadTradesAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Trades, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTradesAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<TradeRecordDbn> ReadTrades(Stream stream)
        {
            return ReadTradesAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBP-1 records (<c>mbp-1</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<Mbp1RecordDbn> ReadMbp1Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Mbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMbp1Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<Mbp1RecordDbn> ReadMbp1(Stream stream)
        {
            return ReadMbp1Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBO records (<c>mbo</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<MboRecordDbn> ReadMboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Mbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<MboRecordDbn> ReadMbo(Stream stream)
        {
            return ReadMboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBP-10 records (<c>mbp-10</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<Mbp10RecordDbn> ReadMbp10Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Mbp10, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMbp10Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<Mbp10RecordDbn> ReadMbp10(Stream stream)
        {
            return ReadMbp10Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the TBBO records (<c>tbbo</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<TbboRecordDbn> ReadTbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Tbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<TbboRecordDbn> ReadTbbo(Stream stream)
        {
            return ReadTbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the BBO records (<c>bbo-1s</c>, <c>bbo-1m</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<BboRecordDbn> ReadBboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadBboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<BboRecordDbn> ReadBbo(Stream stream)
        {
            return ReadBboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the TCBBO records (<c>tcbbo</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<TcbboRecordDbn> ReadTcbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Tcbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTcbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<TcbboRecordDbn> ReadTcbbo(Stream stream)
        {
            return ReadTcbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the CMBP-1 records (<c>cmbp-1</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<Cmbp1RecordDbn> ReadCmbp1Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Cmbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadCmbp1Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<Cmbp1RecordDbn> ReadCmbp1(Stream stream)
        {
            return ReadCmbp1Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the OHLCV bars of any interval (<c>ohlcv-1s</c>, <c>-1m</c>, <c>-1h</c>, <c>-1d</c>, <c>-eod</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<OhlcvRecordDbn> ReadOhlcvAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadOhlcvAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<OhlcvRecordDbn> ReadOhlcv(Stream stream)
        {
            return ReadOhlcvAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the statistics records (<c>statistics</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<StatisticsRecordDbn> ReadStatisticsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Statistics, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadStatisticsAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<StatisticsRecordDbn> ReadStatistics(Stream stream)
        {
            return ReadStatisticsAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the instrument definition records (<c>definition</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<DefinitionRecordDbn> ReadDefinitionsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Definitions, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadDefinitionsAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<DefinitionRecordDbn> ReadDefinitions(Stream stream)
        {
            return ReadDefinitionsAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the status records (<c>status</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<StatusRecordDbn> ReadStatusAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Status, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadStatusAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<StatusRecordDbn> ReadStatus(Stream stream)
        {
            return ReadStatusAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the imbalance records (<c>imbalance</c>) of a DBN stream as they are decoded.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">The data is not DBN, is cut short or malformed, or holds records of another schema.</exception>
        public static IAsyncEnumerable<ImbalanceRecordDbn> ReadImbalanceAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return DbnDecoder.DecodeAsync(stream, DbnRecordLayouts.Imbalance, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadImbalanceAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">DBN data, positioned at its start; left open.</param>
        public static IEnumerable<ImbalanceRecordDbn> ReadImbalance(Stream stream)
        {
            return ReadImbalanceAsync(stream).ToBlockingEnumerable();
        }
    }
}
