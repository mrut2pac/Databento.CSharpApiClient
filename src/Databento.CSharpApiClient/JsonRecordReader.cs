using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Reads JSON records the caller already has, such as a downloaded batch file, one line at a time, as
    /// <see cref="DatabentoJsonClient"/> reads them from a response. Prices and timestamps are read pretty or not; a record names
    /// its symbol only when the data does (a batch job's <c>map_symbols</c>). Compressed data must be decompressed by the caller:
    /// <see cref="System.IO.Compression.GZipStream"/> for gzip, a zstd library of your choice for zstd.
    /// </summary>
    public static class JsonRecordReader
    {
        /// <summary>Reads the consolidated BBO records (<c>cbbo-1s</c>, <c>cbbo-1m</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<CbboRecordJson> ReadCbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Cbbo, (CbboRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadCbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<CbboRecordJson> ReadCbbo(Stream stream)
        {
            return ReadCbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the trade records (<c>trades</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<TradeRecordJson> ReadTradesAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Trades, (TradeRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTradesAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TradeRecordJson> ReadTrades(Stream stream)
        {
            return ReadTradesAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBP-1 records (<c>mbp-1</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<Mbp1RecordJson> ReadMbp1Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Mbp1, (Mbp1RecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMbp1Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<Mbp1RecordJson> ReadMbp1(Stream stream)
        {
            return ReadMbp1Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBO records (<c>mbo</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<MboRecordJson> ReadMboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Mbo, (MboRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<MboRecordJson> ReadMbo(Stream stream)
        {
            return ReadMboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the MBP-10 records (<c>mbp-10</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<Mbp10RecordJson> ReadMbp10Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Mbp10, (Mbp10RecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadMbp10Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<Mbp10RecordJson> ReadMbp10(Stream stream)
        {
            return ReadMbp10Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the TBBO records (<c>tbbo</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<TbboRecordJson> ReadTbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Tbbo, (TbboRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TbboRecordJson> ReadTbbo(Stream stream)
        {
            return ReadTbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the BBO records (<c>bbo-1s</c>, <c>bbo-1m</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<BboRecordJson> ReadBboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Bbo, (BboRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadBboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<BboRecordJson> ReadBbo(Stream stream)
        {
            return ReadBboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the TCBBO records (<c>tcbbo</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<TcbboRecordJson> ReadTcbboAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Tcbbo, (TcbboRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadTcbboAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TcbboRecordJson> ReadTcbbo(Stream stream)
        {
            return ReadTcbboAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the CMBP-1 records (<c>cmbp-1</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<Cmbp1RecordJson> ReadCmbp1Async(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Cmbp1, (Cmbp1RecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadCmbp1Async(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<Cmbp1RecordJson> ReadCmbp1(Stream stream)
        {
            return ReadCmbp1Async(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the OHLCV bars of any interval (<c>ohlcv-1s</c>, <c>-1m</c>, <c>-1h</c>, <c>-1d</c>, <c>-eod</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<OhlcvRecordJson> ReadOhlcvAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Ohlcv, (OhlcvRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadOhlcvAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<OhlcvRecordJson> ReadOhlcv(Stream stream)
        {
            return ReadOhlcvAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the statistics records (<c>statistics</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<StatisticsRecordJson> ReadStatisticsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Statistics, (StatisticsRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadStatisticsAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<StatisticsRecordJson> ReadStatistics(Stream stream)
        {
            return ReadStatisticsAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the instrument definition records (<c>definition</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<DefinitionRecordJson> ReadDefinitionsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Definitions, (DefinitionRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadDefinitionsAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<DefinitionRecordJson> ReadDefinitions(Stream stream)
        {
            return ReadDefinitionsAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the status records (<c>status</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<StatusRecordJson> ReadStatusAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Status, (StatusRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadStatusAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<StatusRecordJson> ReadStatus(Stream stream)
        {
            return ReadStatusAsync(stream).ToBlockingEnumerable();
        }

        /// <summary>Reads the imbalance records (<c>imbalance</c>) of JSON data as they are read.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <param name="cancellationToken">Token to stop reading.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidDataException">A line is not a JSON record, or holds a record of another schema.</exception>
        public static IAsyncEnumerable<ImbalanceRecordJson> ReadImbalanceAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return ReadAsync(stream, DbnRecordLayouts.Imbalance, (ImbalanceRecordJson record) => record.Header, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="ReadImbalanceAsync(Stream, CancellationToken)"/>.</summary>
        /// <param name="stream">JSON data, one record per line, positioned at its start; left open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public static IEnumerable<ImbalanceRecordJson> ReadImbalance(Stream stream)
        {
            return ReadImbalanceAsync(stream).ToBlockingEnumerable();
        }

        // the records of one schema, read as the JSON client reads a response: a record of another schema's record type fails, as
        // in DbnReader, and so does a line that isn't a JSON record
        private static async IAsyncEnumerable<TJson> ReadAsync<TJson, TDbn>(
            Stream stream,
            DbnRecordLayout<TDbn> schema,
            Func<TJson, RecordHeaderJson> header,
            [EnumeratorCancellation] CancellationToken cancellationToken)
            where TJson : class
        {
            await foreach(TJson record in JsonRecordLines.ReadAsync(stream, header, requireHeader: true, MalformedLine, cancellationToken).ConfigureAwait(false))
            {
                RType recordType = header(record).RecordType;
                if(!schema.Accepts(recordType))
                {
                    throw new InvalidDataException(
                        string.Format(CultureInfo.InvariantCulture, "Unexpected rtype 0x{0:X2} in {1} data.", (byte)recordType, schema.Name));
                }

                yield return record;
            }
        }

        private static Exception MalformedLine(string recordType, long lineNumber, Exception ex)
        {
            return new InvalidDataException(
                string.Format(CultureInfo.InvariantCulture, "Failed to read a {0} record from line {1} of the JSON data: {2}", recordType, lineNumber, ex.Message),
                ex);
        }
    }
}
