using System;
using System.Collections.Generic;

using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// The Databento schemas the archive has columns for, by name. Another schema is a contained addition: its record interface, archive
    /// schema and JSON reader.
    /// </summary>
    internal static class ArchiveSchemaHandlers
    {
        private static readonly IReadOnlyDictionary<string, IArchiveSchemaHandler> Handlers = Create();

        /// <summary>
        /// Gets the handler of a Databento schema
        /// </summary>
        /// <param name="schema">Databento schema, as its folder is named, e.g. <c>cbbo-1m</c></param>
        /// <param name="handler">The handler</param>
        /// <returns><c>false</c> when the archive has no columns for the schema</returns>
        public static bool TryGet(string schema, out IArchiveSchemaHandler handler)
        {
            return Handlers.TryGetValue(schema, out handler);
        }

        private static IReadOnlyDictionary<string, IArchiveSchemaHandler> Create()
        {
            IArchiveSchemaHandler[] handlers =
            {
                Cbbo(Schema.ConsolidatedBBO1Sec, RType.Cbbo1S),
                Cbbo(Schema.ConsolidatedBBO1Min, RType.Cbbo1M),
                Bbo(Schema.Bbo1Sec, RType.Bbo1S),
                Bbo(Schema.Bbo1Min, RType.Bbo1M),
                new ArchiveSchemaHandler<ITcbboRecord, TcbboRecordDbn, TcbboRecordJson>(Schema.Tcbbo, new TcbboArchiveSchema(RType.Tcbbo), JsonRecordReader.ReadTcbboAsync),
                new ArchiveSchemaHandler<ITradeRecord, TradeRecordDbn, TradeRecordJson>(Schema.Trades, new TradeArchiveSchema(RType.Mbp0), JsonRecordReader.ReadTradesAsync),
                Ohlcv(Schema.Ohlcv1Sec, RType.Ohlcv1S),
                Ohlcv(Schema.Ohlcv1Min, RType.Ohlcv1M),
                Ohlcv(Schema.Ohlcv1Hour, RType.Ohlcv1H),
                Ohlcv(Schema.Ohlcv1Day, RType.Ohlcv1D),
                Ohlcv(Schema.OhlcvEod, RType.OhlcvEod),
                new ArchiveSchemaHandler<IStatisticsRecord, StatisticsRecordDbn, StatisticsRecordJson>(Schema.Statistics, new StatisticsArchiveSchema(RType.Statistics), JsonRecordReader.ReadStatisticsAsync),
            };

            Dictionary<string, IArchiveSchemaHandler> byName = new Dictionary<string, IArchiveSchemaHandler>(StringComparer.Ordinal);
            foreach(IArchiveSchemaHandler handler in handlers)
            {
                byName.Add(handler.Schema, handler);
            }

            return byName;
        }

        private static IArchiveSchemaHandler Cbbo(string schema, RType recordType)
        {
            return new ArchiveSchemaHandler<ICbboRecord, CbboRecordDbn, CbboRecordJson>(schema, new CbboArchiveSchema(recordType), JsonRecordReader.ReadCbboAsync);
        }

        private static IArchiveSchemaHandler Bbo(string schema, RType recordType)
        {
            return new ArchiveSchemaHandler<IBboRecord, BboRecordDbn, BboRecordJson>(schema, new BboArchiveSchema(recordType), JsonRecordReader.ReadBboAsync);
        }

        private static IArchiveSchemaHandler Ohlcv(string schema, RType recordType)
        {
            return new ArchiveSchemaHandler<IOhlcvRecord, OhlcvRecordDbn, OhlcvRecordJson>(schema, new OhlcvArchiveSchema(recordType), JsonRecordReader.ReadOhlcvAsync);
        }
    }
}
