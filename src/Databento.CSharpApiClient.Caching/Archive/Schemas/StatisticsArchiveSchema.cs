using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of statistics records (<c>statistics</c>), such as OPRA open interest. A read gives an undefined
    /// quantity as <see cref="long.MaxValue"/>, also one written as DBN v1's <see cref="int.MaxValue"/>; the interface reads both as null.
    /// </summary>
    internal sealed class StatisticsArchiveSchema : ArchiveRecordSchema<IStatisticsRecord, StatisticsRecordDbn>
    {
        public StatisticsArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override StatisticsRecordDbn CreateRecord(string rawSymbol)
        {
            return new StatisticsRecordDbn
            {
                Symbol = rawSymbol,
            };
        }

        private static IEnumerable<ArchiveColumn<IStatisticsRecord, StatisticsRecordDbn>> CreateColumns()
        {
            return HeaderColumns(
                (record, value) => record.TsEventUtc = value ?? DateTime.MinValue,
                (record, value) => record.RecordType = value,
                (record, value) => record.PublisherId = value,
                (record, value) => record.InstrumentId = value).Concat(new[]
            {
                Timestamp("ts_recv", record => record.TsReceivedUtc, (record, value) => record.TsReceivedUtc = value ?? DateTime.MinValue),
                Timestamp("ts_ref", record => record.TsRefUtc, (record, value) => record.TsRefUtc = value),
                NullableValue<double>("price", record => record.Price, (record, value) => record.Price = value ?? double.NaN),
                NullableValue<long>("quantity", record => record.Quantity, (record, value) => record.Quantity = value ?? long.MaxValue),
                Value<uint>("sequence", record => record.Sequence, (record, value) => record.Sequence = value),
                Value<int>("ts_in_delta", record => record.TsInDelta, (record, value) => record.TsInDelta = value),
                Value<ushort>("stat_type", record => record.StatType, (record, value) => record.StatType = value),
                Value<ushort>("channel_id", record => record.ChannelId, (record, value) => record.ChannelId = value),
                Value<byte>("update_action", record => record.UpdateAction, (record, value) => record.UpdateAction = value),
                Value<byte>("stat_flags", record => (byte)record.StatFlags, (record, value) => record.StatFlags = (MessageInfoBits)value),
            });
        }
    }
}
