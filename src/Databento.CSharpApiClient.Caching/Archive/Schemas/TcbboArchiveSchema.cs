// Ignore Spelling: Tcbbo

using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of OPRA trades with the consolidated BBO just before each (<c>tcbbo</c>)
    /// </summary>
    internal sealed class TcbboArchiveSchema : ArchiveRecordSchema<ITcbboRecord, TcbboRecordDbn>
    {
        public TcbboArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override TcbboRecordDbn CreateRecord(string rawSymbol)
        {
            return new TcbboRecordDbn
            {
                Symbol = rawSymbol,
                Level = new CbboLevelDbn(),
            };
        }

        private static IEnumerable<ArchiveColumn<ITcbboRecord, TcbboRecordDbn>> CreateColumns()
        {
            return HeaderColumns(
                (record, value) => record.TsEventUtc = value ?? DateTime.MinValue,
                (record, value) => record.RecordType = value,
                (record, value) => record.PublisherId = value,
                (record, value) => record.InstrumentId = value).Concat(new[]
            {
                NullableValue<double>("price", record => record.Price, (record, value) => record.Price = value ?? double.NaN),
                Value<uint>("size", record => record.Size, (record, value) => record.Size = value),
                Value<byte>("action", record => (byte)record.Action, (record, value) => record.Action = (OrderBookAction)value),
                Value<byte>("side", record => (byte)record.Side, (record, value) => record.Side = (TradeAggressor)value),
                Value<byte>("flags", record => (byte)record.Flags, (record, value) => record.Flags = (MessageInfoBits)value),
                Timestamp("ts_recv", record => record.TsReceivedUtc, (record, value) => record.TsReceivedUtc = value ?? DateTime.MinValue),
                Value<int>("ts_in_delta", record => record.TsInDelta, (record, value) => record.TsInDelta = value),
                NullableValue<double>("bid_px", record => record.BidPrice, (record, value) => record.Level.BidPrice = value ?? double.NaN),
                NullableValue<double>("ask_px", record => record.AskPrice, (record, value) => record.Level.AskPrice = value ?? double.NaN),
                Value<uint>("bid_sz", record => record.BidSize, (record, value) => record.Level.BidSize = value),
                Value<uint>("ask_sz", record => record.AskSize, (record, value) => record.Level.AskSize = value),
                Value<ushort>("bid_pb", record => record.BidPublisherId, (record, value) => record.Level.BidPublisherId = value),
                Value<ushort>("ask_pb", record => record.AskPublisherId, (record, value) => record.Level.AskPublisherId = value),
            });
        }
    }
}
