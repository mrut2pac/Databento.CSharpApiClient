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
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Reference timestamp associated with the statistic (e.g. session open).</summary>
        [JsonPropertyName("ts_ref")]
        public DateTime? TsRefUtc { get; set; }

        /// <summary>Statistic price value (display-scaled).</summary>
        [JsonPropertyName("price")]
        public double Price { get; set; }

        /// <summary>Statistic quantity value (e.g. open interest).</summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("sequence")]
        public uint Sequence { get; set; }

        [JsonPropertyName("ts_in_delta")]
        public long TsInDelta { get; set; }

        /// <summary>
        /// Statistic type identifier.
        /// Common values: 1=OpeningPrice, 2=IndicativeOpeningPrice, 3=SettlementPrice,
        /// 4=TradingSessionHighPrice, 5=TradingSessionLowPrice, 6=ClearedVolume, 7=LowestOffer,
        /// 8=HighestBid, 9=OpenInterest, 10=FixingPrice.
        /// </summary>
        [JsonPropertyName("stat_type")]
        public ushort StatType { get; set; }

        [JsonPropertyName("channel_id")]
        public ushort ChannelId { get; set; }

        [JsonPropertyName("update_action")]
        public byte UpdateAction { get; set; }

        [JsonPropertyName("stat_flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits StatFlags { get; set; }

        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
