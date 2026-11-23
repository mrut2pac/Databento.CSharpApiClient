using System;

using Databento.CSharpApiClient.DataModel.Json;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Converts the DBN records that have a record interface to the JSON model, with the values <see cref="DatabentoJsonClient"/>
    /// reads from the JSON encoding of the same record. So records downloaded as DBN can be saved as JSON and read back like
    /// records downloaded as JSON.
    /// </summary>
    public static class DbnToJsonExtensions
    {
        /// <summary>
        /// Returns <paramref name="record"/> as a <see cref="BboRecordJson"/>. An undefined <see cref="BboRecordDbn.Price"/> or event
        /// time becomes <see langword="null"/>, and an undefined bid or ask price stays <see cref="double.NaN"/>, as the JSON model
        /// reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        public static BboRecordJson ToJsonRecord(this BboRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new BboRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                Price = UndefinedValues.Price(record.Price),
                Size = record.Size,
                Side = record.Side,
                Flags = record.Flags,
                TsReceivedUtc = record.TsReceivedUtc,
                Sequence = record.Sequence,
                Levels = record.Level == null ? null : new[] { ToJsonLevel(record.Level) },
                Symbol = record.Symbol,
            };
        }

        /// <summary>
        /// Returns <paramref name="record"/> as a <see cref="CbboRecordJson"/>. An undefined last trade price or event time becomes
        /// <see langword="null"/>, and an undefined bid or ask price stays <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">A size is beyond <see cref="int.MaxValue"/>, which the JSON model can't hold.</exception>
        public static CbboRecordJson ToJsonRecord(this CbboRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new CbboRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                TsReceivedUtc = record.TsReceivedUtc,
                Side = record.Side,
                Price = UndefinedValues.Price(record.Price),
                Size = checked((int)record.Size),
                Flags = record.Flags,
                Levels = new[] { ToJsonLevel(record.BidPrice, record.AskPrice, record.BidSize, record.AskSize, record.BidPublisherId, record.AskPublisherId) },
                Symbol = record.Symbol,
            };
        }

        /// <summary>
        /// Returns <paramref name="record"/> as a <see cref="TcbboRecordJson"/>. An undefined event time becomes
        /// <see langword="null"/> and an undefined price stays <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The bid or ask size is beyond <see cref="int.MaxValue"/>, which the JSON model can't hold.</exception>
        public static TcbboRecordJson ToJsonRecord(this TcbboRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new TcbboRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                Price = record.Price,
                Size = record.Size,
                Action = record.Action,
                Side = record.Side,
                Flags = record.Flags,
                TsReceivedUtc = record.TsReceivedUtc,
                TsInDelta = record.TsInDelta,
                Levels = record.Level == null ? null : new[] { ToJsonLevel(record.Level) },
                Symbol = record.Symbol,
            };
        }

        /// <summary>
        /// Returns <paramref name="record"/> as a <see cref="TradeRecordJson"/>. An undefined event time becomes
        /// <see langword="null"/> and an undefined price stays <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        public static TradeRecordJson ToJsonRecord(this TradeRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new TradeRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                Price = record.Price,
                Size = record.Size,
                Action = record.Action,
                Side = record.Side,
                Flags = record.Flags,
                Depth = record.Depth,
                TsReceivedUtc = record.TsReceivedUtc,
                TsInDelta = record.TsInDelta,
                Sequence = record.Sequence,
                Symbol = record.Symbol,
            };
        }

        /// <summary>
        /// Returns <paramref name="record"/> as an <see cref="OhlcvRecordJson"/>. An undefined event time becomes
        /// <see langword="null"/> and an undefined price stays <see cref="double.NaN"/>, as the JSON model reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The volume is beyond <see cref="long.MaxValue"/>, which the JSON model can't hold.</exception>
        public static OhlcvRecordJson ToJsonRecord(this OhlcvRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new OhlcvRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                Open = record.Open,
                High = record.High,
                Low = record.Low,
                Close = record.Close,
                Volume = checked((long)record.Volume),
                Symbol = record.Symbol,
            };
        }

        /// <summary>
        /// Returns <paramref name="record"/> as a <see cref="StatisticsRecordJson"/>. An undefined event time becomes
        /// <see langword="null"/>, and an undefined price stays <see cref="double.NaN"/> and an undefined quantity its sentinel, as
        /// the JSON model reads them.
        /// </summary>
        /// <param name="record">The DBN record to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
        public static StatisticsRecordJson ToJsonRecord(this StatisticsRecordDbn record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return new StatisticsRecordJson
            {
                Header = ToJsonHeader(record.RecordType, record.PublisherId, record.InstrumentId, record.TsEventUtc),
                TsReceivedUtc = record.TsReceivedUtc,
                TsRefUtc = record.TsRefUtc,
                Price = record.Price,
                Quantity = record.Quantity,
                Sequence = record.Sequence,
                TsInDelta = record.TsInDelta,
                StatType = record.StatType,
                ChannelId = record.ChannelId,
                UpdateAction = record.UpdateAction,
                StatFlags = record.StatFlags,
                Symbol = record.Symbol,
            };
        }

        // a decoded record's header fields as the JSON encoding's header: an undefined event time, which the decoder reads as
        // DateTime.MinValue, is null in JSON
        private static RecordHeaderJson ToJsonHeader(RType recordType, ushort publisherId, uint instrumentId, DateTime tsEventUtc)
        {
            return new RecordHeaderJson
            {
                TsEventUtc = UndefinedValues.Timestamp(tsEventUtc),
                RecordType = recordType,
                PublisherId = publisherId,
                InstrumentId = instrumentId,
            };
        }

        // a BBO level as the JSON encoding carries it; an undefined price stays NaN, as the JSON model reads it
        private static Mbp1LevelJson ToJsonLevel(Mbp1LevelDbn level)
        {
            return new Mbp1LevelJson
            {
                BidPrice = level.BidPrice,
                AskPrice = level.AskPrice,
                BidSize = level.BidSize,
                AskSize = level.AskSize,
                BidCount = level.BidCount,
                AskCount = level.AskCount,
            };
        }

        private static CbboLevel1RecordJson ToJsonLevel(CbboLevelDbn level)
            => ToJsonLevel(level.BidPrice, level.AskPrice, level.BidSize, level.AskSize, level.BidPublisherId, level.AskPublisherId);

        // a consolidated level as the JSON encoding carries it, for TCBBO's level and CBBO's flat fields alike: the JSON model holds
        // sizes as int, and an undefined price stays NaN, as the JSON model reads it
        private static CbboLevel1RecordJson ToJsonLevel(double bidPrice, double askPrice, uint bidSize, uint askSize, ushort bidPublisherId, ushort askPublisherId)
        {
            return new CbboLevel1RecordJson
            {
                BidPrice = bidPrice,
                AskPrice = askPrice,
                BidSize = checked((int)bidSize),
                AskSize = checked((int)askSize),
                BidPublisherId = bidPublisherId,
                AskPublisherId = askPublisherId,
            };
        }
    }
}
