// Ignore Spelling: Cbbo

using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of OPRA consolidated BBO records (<c>cbbo-1s</c>, <c>cbbo-1m</c>)
    /// </summary>
    internal sealed class CbboArchiveSchema : ArchiveRecordSchema<ICbboRecord, CbboRecordDbn>
    {
        public CbboArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override CbboRecordDbn CreateRecord(string rawSymbol)
        {
            return new CbboRecordDbn
            {
                Symbol = rawSymbol,
            };
        }

        private static IEnumerable<ArchiveColumn<ICbboRecord, CbboRecordDbn>> CreateColumns()
        {
            return HeaderColumns(
                (record, value) => record.TsEventUtc = value ?? DateTime.MinValue,
                (record, value) => record.RecordType = value,
                (record, value) => record.PublisherId = value,
                (record, value) => record.InstrumentId = value).Concat(new[]
            {
                Timestamp("ts_recv", record => record.TsReceivedUtc, (record, value) => record.TsReceivedUtc = value ?? DateTime.MinValue),
                Value<byte>("side", record => (byte)record.Side, (record, value) => record.Side = (TradeAggressor)value),
                NullableValue<double>("price", record => record.Price, (record, value) => record.Price = value ?? double.NaN),
                Value<uint>("size", record => record.Size, (record, value) => record.Size = value),
                Value<byte>("flags", record => (byte)record.Flags, (record, value) => record.Flags = (MessageInfoBits)value),
                NullableValue<double>("bid_px", record => record.BidPrice, (record, value) => record.BidPrice = value ?? double.NaN),
                NullableValue<double>("ask_px", record => record.AskPrice, (record, value) => record.AskPrice = value ?? double.NaN),
                Value<uint>("bid_sz", record => record.BidSize, (record, value) => record.BidSize = value),
                Value<uint>("ask_sz", record => record.AskSize, (record, value) => record.AskSize = value),
                Value<ushort>("bid_pb", record => record.BidPublisherId, (record, value) => record.BidPublisherId = value),
                Value<ushort>("ask_pb", record => record.AskPublisherId, (record, value) => record.AskPublisherId = value),
            });
        }
    }
}
