using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of trade records (<c>trades</c>), e.g. the DBIX.CORE index values
    /// </summary>
    internal sealed class TradeArchiveSchema : ArchiveRecordSchema<ITradeRecord, TradeRecordDbn>
    {
        public TradeArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override TradeRecordDbn CreateRecord(string rawSymbol)
        {
            return new TradeRecordDbn
            {
                Symbol = rawSymbol,
            };
        }

        private static IEnumerable<ArchiveColumn<ITradeRecord, TradeRecordDbn>> CreateColumns()
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
                Value<byte>("depth", record => record.Depth, (record, value) => record.Depth = value),
                Timestamp("ts_recv", record => record.TsReceivedUtc, (record, value) => record.TsReceivedUtc = value ?? DateTime.MinValue),
                Value<int>("ts_in_delta", record => record.TsInDelta, (record, value) => record.TsInDelta = value),
                Value<uint>("sequence", record => record.Sequence, (record, value) => record.Sequence = value),
            });
        }
    }
}
