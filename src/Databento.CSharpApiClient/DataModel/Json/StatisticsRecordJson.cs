using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A publisher-statistics record from the <c>statistics</c> schema (JSON encoding).
    /// Carries settlement price, open interest, and other per-instrument statistics.
    /// Corresponds to DBN rtype <c>Statistics</c>.
    /// </summary>
    public sealed class StatisticsRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Reference timestamp associated with the statistic (e.g. session open).</summary>
        [JsonPropertyName("ts_ref")]
        public DateTime? TsRefUtc { get; set; }

        /// <summary>Statistic price value (display-scaled).</summary>
        [JsonPropertyName("price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Price { get; set; }

        /// <summary>Statistic quantity value (e.g. open interest).</summary>
        [JsonPropertyName("quantity")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Quantity { get; set; }

        /// <summary>Venue sequence number.</summary>
        [JsonPropertyName("sequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Sequence { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        [JsonPropertyName("ts_in_delta")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long TsInDelta { get; set; }

        /// <summary>
        /// Statistic type identifier.
        /// Common values: 1=OpeningPrice, 2=IndicativeOpeningPrice, 3=SettlementPrice,
        /// 4=TradingSessionHighPrice, 5=TradingSessionLowPrice, 6=ClearedVolume, 7=LowestOffer,
        /// 8=HighestBid, 9=OpenInterest, 10=FixingPrice.
        /// </summary>
        [JsonPropertyName("stat_type")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort StatType { get; set; }

        /// <summary>Publisher channel identifier.</summary>
        [JsonPropertyName("channel_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort ChannelId { get; set; }

        /// <summary>Action that generated this update: 1=Add, 2=Delete.</summary>
        [JsonPropertyName("update_action")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte UpdateAction { get; set; }

        /// <summary>Statistics message info flags.</summary>
        [JsonPropertyName("stat_flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits StatFlags { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
