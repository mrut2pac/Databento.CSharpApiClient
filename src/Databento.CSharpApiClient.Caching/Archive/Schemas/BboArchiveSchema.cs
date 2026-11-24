// Ignore Spelling: Bbo

using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of CME Globex BBO records (<c>bbo-1s</c>, <c>bbo-1m</c>)
    /// </summary>
    internal sealed class BboArchiveSchema : ArchiveRecordSchema<IBboRecord, BboRecordDbn>
    {
        public BboArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override BboRecordDbn CreateRecord(string rawSymbol)
        {
            return new BboRecordDbn
            {
                Symbol = rawSymbol,
                Level = new Mbp1LevelDbn(),
            };
        }

        private static IEnumerable<ArchiveColumn<IBboRecord, BboRecordDbn>> CreateColumns()
        {
            return HeaderColumns(
                (record, value) => record.TsEventUtc = value ?? DateTime.MinValue,
                (record, value) => record.RecordType = value,
                (record, value) => record.PublisherId = value,
                (record, value) => record.InstrumentId = value).Concat(new[]
            {
                NullableValue<double>("price", record => record.Price, (record, value) => record.Price = value ?? double.NaN),
                Value<uint>("size", record => record.Size, (record, value) => record.Size = value),
                Value<byte>("side", record => (byte)record.Side, (record, value) => record.Side = (TradeAggressor)value),
                Value<byte>("flags", record => (byte)record.Flags, (record, value) => record.Flags = (MessageInfoBits)value),
                Timestamp("ts_recv", record => record.TsReceivedUtc, (record, value) => record.TsReceivedUtc = value ?? DateTime.MinValue),
                Value<uint>("sequence", record => record.Sequence, (record, value) => record.Sequence = value),
                NullableValue<double>("bid_px", record => record.BidPrice, (record, value) => record.Level.BidPrice = value ?? double.NaN),
                NullableValue<double>("ask_px", record => record.AskPrice, (record, value) => record.Level.AskPrice = value ?? double.NaN),
                Value<uint>("bid_sz", record => record.BidSize, (record, value) => record.Level.BidSize = value),
                Value<uint>("ask_sz", record => record.AskSize, (record, value) => record.Level.AskSize = value),
                Value<uint>("bid_ct", record => record.BidCount, (record, value) => record.Level.BidCount = value),
                Value<uint>("ask_ct", record => record.AskCount, (record, value) => record.Level.AskCount = value),
            });
        }
    }
}
