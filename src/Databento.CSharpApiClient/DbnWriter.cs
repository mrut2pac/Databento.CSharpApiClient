using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Writes uncompressed DBN that <see cref="DbnReader"/> and Databento's own tools read: the metadata, in the layout of its
    /// <see cref="DbnMetadata.Version"/> (1, 2 or 3), then the records in that version's layout. The record types' <c>Symbol</c>
    /// is not written; the reader takes it from the metadata's symbol mappings again. To write <c>.dbn.zst</c>, pass a compressing
    /// stream from the zstd library of your choice.
    /// </summary>
    /// <remarks>
    /// Values DBN cannot hold are refused rather than altered: text that is not ASCII or too wide for its field, a timestamp
    /// before the UNIX epoch or after 2554, an infinite price, or a number too large for the field it goes in. A field the version's
    /// layout does not have, such as a definition's legs in v1, is not written. Undefined values are written as
    /// DBN's: a <see cref="double.NaN"/> price as <c>i64::MAX</c>, a <see cref="DateTime.MinValue"/> or <see langword="null"/>
    /// timestamp as <c>u64::MAX</c>. Records are held in memory in blocks of 64 KB before they are written, so a refused record
    /// stops the write with only the blocks before it in the stream.
    /// </remarks>
    public static class DbnWriter
    {
        /// <summary>Writes a DBN stream of consolidated BBO records (<c>cbbo-1s</c>, <c>cbbo-1m</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteCbboAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<CbboRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Cbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteCbboAsync(Stream, DbnMetadata, IAsyncEnumerable{CbboRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteCbbo(Stream stream, DbnMetadata metadata, IEnumerable<CbboRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Cbbo);
        }

        /// <summary>Writes a DBN stream of trade records (<c>trades</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteTradesAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<TradeRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Trades, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteTradesAsync(Stream, DbnMetadata, IAsyncEnumerable{TradeRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteTrades(Stream stream, DbnMetadata metadata, IEnumerable<TradeRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Trades);
        }

        /// <summary>Writes a DBN stream of MBP-1 records (<c>mbp-1</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteMbp1Async(Stream stream, DbnMetadata metadata, IAsyncEnumerable<Mbp1RecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Mbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteMbp1Async(Stream, DbnMetadata, IAsyncEnumerable{Mbp1RecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteMbp1(Stream stream, DbnMetadata metadata, IEnumerable<Mbp1RecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Mbp1);
        }

        /// <summary>Writes a DBN stream of MBO records (<c>mbo</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteMboAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<MboRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Mbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteMboAsync(Stream, DbnMetadata, IAsyncEnumerable{MboRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteMbo(Stream stream, DbnMetadata metadata, IEnumerable<MboRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Mbo);
        }

        /// <summary>Writes a DBN stream of MBP-10 records (<c>mbp-10</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteMbp10Async(Stream stream, DbnMetadata metadata, IAsyncEnumerable<Mbp10RecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Mbp10, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteMbp10Async(Stream, DbnMetadata, IAsyncEnumerable{Mbp10RecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteMbp10(Stream stream, DbnMetadata metadata, IEnumerable<Mbp10RecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Mbp10);
        }

        /// <summary>Writes a DBN stream of TBBO records (<c>tbbo</c>), with the MBP-1 record type the schema sends as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteTbboAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<TbboRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Tbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteTbboAsync(Stream, DbnMetadata, IAsyncEnumerable{TbboRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteTbbo(Stream stream, DbnMetadata metadata, IEnumerable<TbboRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Tbbo);
        }

        /// <summary>Writes a DBN stream of BBO records (<c>bbo-1s</c>, <c>bbo-1m</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteBboAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<BboRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Bbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteBboAsync(Stream, DbnMetadata, IAsyncEnumerable{BboRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteBbo(Stream stream, DbnMetadata metadata, IEnumerable<BboRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Bbo);
        }

        /// <summary>Writes a DBN stream of TCBBO records (<c>tcbbo</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteTcbboAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<TcbboRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Tcbbo, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteTcbboAsync(Stream, DbnMetadata, IAsyncEnumerable{TcbboRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteTcbbo(Stream stream, DbnMetadata metadata, IEnumerable<TcbboRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Tcbbo);
        }

        /// <summary>Writes a DBN stream of CMBP-1 records (<c>cmbp-1</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteCmbp1Async(Stream stream, DbnMetadata metadata, IAsyncEnumerable<Cmbp1RecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Cmbp1, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteCmbp1Async(Stream, DbnMetadata, IAsyncEnumerable{Cmbp1RecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteCmbp1(Stream stream, DbnMetadata metadata, IEnumerable<Cmbp1RecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Cmbp1);
        }

        /// <summary>Writes a DBN stream of OHLCV bars of any interval (<c>ohlcv-1s</c>, <c>-1m</c>, <c>-1h</c>, <c>-1d</c>, <c>-eod</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteOhlcvAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<OhlcvRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Ohlcv, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteOhlcvAsync(Stream, DbnMetadata, IAsyncEnumerable{OhlcvRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteOhlcv(Stream stream, DbnMetadata metadata, IEnumerable<OhlcvRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Ohlcv);
        }

        /// <summary>Writes a DBN stream of statistics records (<c>statistics</c>), in the layout of the metadata's DBN version as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteStatisticsAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<StatisticsRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Statistics, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteStatisticsAsync(Stream, DbnMetadata, IAsyncEnumerable{StatisticsRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteStatistics(Stream stream, DbnMetadata metadata, IEnumerable<StatisticsRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Statistics);
        }

        /// <summary>Writes a DBN stream of instrument definition records (<c>definition</c>) in the DBN v1 or v3 layout; DBN v2 definitions are refused, as the reader refuses them as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteDefinitionsAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<DefinitionRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Definitions, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteDefinitionsAsync(Stream, DbnMetadata, IAsyncEnumerable{DefinitionRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteDefinitions(Stream stream, DbnMetadata metadata, IEnumerable<DefinitionRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Definitions);
        }

        /// <summary>Writes a DBN stream of status records (<c>status</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteStatusAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<StatusRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Status, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteStatusAsync(Stream, DbnMetadata, IAsyncEnumerable{StatusRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteStatus(Stream stream, DbnMetadata metadata, IEnumerable<StatusRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Status);
        }

        /// <summary>Writes a DBN stream of imbalance records (<c>imbalance</c>) as they are enumerated.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        /// <param name="cancellationToken">Token to stop writing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/>, <paramref name="metadata"/> or <paramref name="records"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The metadata or a record holds what DBN cannot, or a record is <see langword="null"/> or of another record type.</exception>
        /// <exception cref="NotSupportedException">The metadata asks for <c>ts_out</c>, which the record classes do not keep.</exception>
        public static Task WriteImbalanceAsync(Stream stream, DbnMetadata metadata, IAsyncEnumerable<ImbalanceRecordDbn> records, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            return DbnEncoder.EncodeAsync(stream, metadata, records, DbnRecordLayouts.Imbalance, cancellationToken);
        }

        /// <summary>Synchronous counterpart of <see cref="WriteImbalanceAsync(Stream, DbnMetadata, IAsyncEnumerable{ImbalanceRecordDbn}, CancellationToken)"/>.</summary>
        /// <param name="stream">Stream to write to; left open.</param>
        /// <param name="metadata">The stream's metadata, whose <see cref="DbnMetadata.Version"/> sets the layout.</param>
        /// <param name="records">The records to write.</param>
        public static void WriteImbalance(Stream stream, DbnMetadata metadata, IEnumerable<ImbalanceRecordDbn> records)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(records);
            DbnEncoder.Encode(stream, metadata, records, DbnRecordLayouts.Imbalance);
        }
    }
}
