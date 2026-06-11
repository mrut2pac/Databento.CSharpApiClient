using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// An instrument-definition record from the <c>definition</c> schema (JSON encoding).
    /// Carries point-in-time contract metadata for an instrument.
    /// Corresponds to DBN rtype <c>InstrumentDef</c>.
    /// </summary>
    public sealed class DefinitionRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Minimum allowed price increment (tick size), display-scaled.</summary>
        [JsonPropertyName("min_price_increment")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double MinPriceIncrement { get; set; }

        /// <summary>Multiplier to convert the venue's raw price to a display price.</summary>
        [JsonPropertyName("display_factor")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double DisplayFactor { get; set; }

        /// <summary>Contract expiry timestamp (UTC). <see langword="null"/> for non-expiring instruments.</summary>
        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }

        /// <summary>First trading date/time for this instrument (UTC).</summary>
        [JsonPropertyName("activation")]
        public DateTime? Activation { get; set; }

        /// <summary>Upper price limit for the instrument (display-scaled).</summary>
        [JsonPropertyName("high_limit_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double HighLimitPrice { get; set; }

        /// <summary>Lower price limit for the instrument (display-scaled).</summary>
        [JsonPropertyName("low_limit_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double LowLimitPrice { get; set; }

        /// <summary>Maximum price movement allowed between trades (display-scaled).</summary>
        [JsonPropertyName("max_price_variation")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double MaxPriceVariation { get; set; }

        /// <summary>Contract unit of measure quantity (e.g. 1000 barrels per lot).</summary>
        [JsonPropertyName("unit_of_measure_qty")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double UnitOfMeasureQty { get; set; }

        /// <summary>Contract size multiplier (e.g. 100 shares per equity option contract).</summary>
        [JsonPropertyName("contract_multiplier")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double ContractMultiplier { get; set; }

        /// <summary>Option strike price (display-scaled). Zero for non-option instruments.</summary>
        [JsonPropertyName("strike_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double StrikePrice { get; set; }

        /// <summary>Venue-native symbol string.</summary>
        [JsonPropertyName("raw_symbol")]
        public string RawSymbol { get; set; }

        /// <summary>Exchange MIC code where this instrument trades.</summary>
        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        /// <summary>Underlying asset code (e.g. <c>"SPX"</c> for S&amp;P 500 options).</summary>
        [JsonPropertyName("asset")]
        public string Asset { get; set; }

        /// <summary>CFI code (ISO 10962) classifying the instrument type.</summary>
        [JsonPropertyName("cfi")]
        public string Cfi { get; set; }

        /// <summary>Exchange-specific security type code.</summary>
        [JsonPropertyName("security_type")]
        public string SecurityType { get; set; }

        /// <summary>
        /// Single-character instrument class: 'F' futures, 'C' call option, 'P' put option,
        /// 'K' FX forward, 'S' stock, 'M' mixed spread.
        /// </summary>
        [JsonPropertyName("instrument_class")]
        public string InstrumentClass { get; set; }

        /// <summary>Underlying instrument symbol for derivatives.</summary>
        [JsonPropertyName("underlying")]
        public string Underlying { get; set; }

        /// <summary>Action that caused this definition message: "A" add, "M" modify, "D" delete.</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
