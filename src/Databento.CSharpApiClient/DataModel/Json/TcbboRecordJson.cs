using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// A Trade + Consolidated BBO record from the <c>tcbbo</c> schema (JSON encoding).
    /// Each message records a trade event alongside the consolidated (national) BBO just before it.
    /// Corresponds to DBN rtype <c>Tcbbo</c>.
    /// </summary>
    public sealed class TcbboRecordJson : ITcbboRecord, IRequestedSymbol
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Trade price (display-scaled).</summary>
        [JsonPropertyName("price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double Price { get; set; }

        /// <summary>Trade quantity in lots.</summary>
        [JsonPropertyName("size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Size { get; set; }

        /// <summary>Order-book action; always "T" for trade events in this schema.</summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(OrderBookActionConverter))]
        public OrderBookAction Action { get; set; }

        /// <summary>Aggressor side: Buyer, Seller, or None.</summary>
        [JsonPropertyName("side")]
        [JsonConverter(typeof(TradeAggressorConverter))]
        public TradeAggressor Side { get; set; }

        /// <summary>Message info flags.</summary>
        [JsonPropertyName("flags")]
        [JsonConverter(typeof(FlagsConverter))]
        public MessageInfoBits Flags { get; set; }

        /// <summary>Always 0: the tcbbo schema carries no <c>depth</c>.</summary>
        [Obsolete("The tcbbo schema carries no depth, so this is always 0.")]
        [JsonPropertyName("depth")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Depth { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Nanosecond latency delta from venue receipt to gateway receipt.</summary>
        [JsonPropertyName("ts_in_delta")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long TsInDelta { get; set; }

        /// <summary>Always 0: the tcbbo schema carries no <c>sequence</c>.</summary>
        [Obsolete("The tcbbo schema carries no sequence, so this is always 0.")]
        [JsonPropertyName("sequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint Sequence { get; set; }

        /// <summary>Consolidated BBO immediately before the trade. Always length 1; use <see cref="Level1"/> for convenient access.</summary>
        [JsonPropertyName("levels")]
        public CbboLevel1RecordJson[] Levels { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>Convenience accessor for the single consolidated BBO level. <see langword="null"/> if <see cref="Levels"/> is empty.</summary>
        [JsonIgnore]
        public CbboLevel1RecordJson Level1 => this.Levels?.Length > 0 ? this.Levels[0] : null;

        /// <summary>
        /// The requested symbol this record belongs to: from the response's symbol mapping when the request covered more than
        /// one symbol, otherwise the symbol requested. <see langword="null"/> for an <c>ALL_SYMBOLS</c> record the response leaves unmapped.
        /// </summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        RType IRecord.RecordType => this.Header?.RecordType ?? default;

        ushort IRecord.PublisherId => this.Header?.PublisherId ?? 0;

        uint IRecord.InstrumentId => this.Header?.InstrumentId ?? 0;

        DateTime? IRecord.TsEventUtc => UndefinedValues.Timestamp(this.Header?.TsEventUtc);

        double? ITcbboRecord.Price => UndefinedValues.Price(this.Price);

        int ITcbboRecord.TsInDelta => checked((int)this.TsInDelta);

        double? ITcbboRecord.BidPrice => UndefinedValues.Price(this.Level1?.BidPrice);

        double? ITcbboRecord.AskPrice => UndefinedValues.Price(this.Level1?.AskPrice);

        uint ITcbboRecord.BidSize => checked((uint)(this.Level1?.BidSize ?? 0));

        uint ITcbboRecord.AskSize => checked((uint)(this.Level1?.AskSize ?? 0));

        ushort ITcbboRecord.BidPublisherId => checked((ushort)(this.Level1?.BidPublisherId ?? 0));

        ushort ITcbboRecord.AskPublisherId => checked((ushort)(this.Level1?.AskPublisherId ?? 0));

        DateTime? ITcbboRecord.TsReceivedUtc => UndefinedValues.Timestamp(this.TsReceivedUtc);
    }
}
