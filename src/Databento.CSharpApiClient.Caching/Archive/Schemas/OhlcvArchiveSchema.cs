// Ignore Spelling: Ohlcv

using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.Caching.Archive.Columns;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.Caching.Archive.Schemas
{
    /// <summary>
    /// Archive columns of OHLCV bar records (<c>ohlcv-1s</c>, <c>ohlcv-1m</c>, ...)
    /// </summary>
    internal sealed class OhlcvArchiveSchema : ArchiveRecordSchema<IOhlcvRecord, OhlcvRecordDbn>
    {
        public OhlcvArchiveSchema(RType recordType)
            : base(recordType, CreateColumns())
        {
        }

        internal override OhlcvRecordDbn CreateRecord(string rawSymbol)
        {
            return new OhlcvRecordDbn
            {
                Symbol = rawSymbol,
            };
        }

        private static IEnumerable<ArchiveColumn<IOhlcvRecord, OhlcvRecordDbn>> CreateColumns()
        {
            return HeaderColumns(
                (record, value) => record.TsEventUtc = value ?? DateTime.MinValue,
                (record, value) => record.RecordType = value,
                (record, value) => record.PublisherId = value,
                (record, value) => record.InstrumentId = value).Concat(new[]
            {
                Value<double>("open", record => record.Open, (record, value) => record.Open = value),
                Value<double>("high", record => record.High, (record, value) => record.High = value),
                Value<double>("low", record => record.Low, (record, value) => record.Low = value),
                Value<double>("close", record => record.Close, (record, value) => record.Close = value),
                Value<ulong>("volume", record => record.Volume, (record, value) => record.Volume = value),
            });
        }
    }
}
