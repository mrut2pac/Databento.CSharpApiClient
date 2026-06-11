using System;

using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// An instrument-definition record from the <c>definition</c> schema (JSON encoding).
    /// Carries point-in-time contract metadata for an instrument.
    /// Corresponds to DBN rtype <c>InstrumentDef</c>.
    /// </summary>
    public sealed class DefinitionRecordJson
    {
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonPropertyName("min_price_increment")]
        public double MinPriceIncrement { get; set; }

        [JsonPropertyName("display_factor")]
        public double DisplayFactor { get; set; }

        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }

        [JsonPropertyName("activation")]
        public DateTime? Activation { get; set; }

        [JsonPropertyName("high_limit_price")]
        public double HighLimitPrice { get; set; }

        [JsonPropertyName("low_limit_price")]
        public double LowLimitPrice { get; set; }

        [JsonPropertyName("max_price_variation")]
        public double MaxPriceVariation { get; set; }

        [JsonPropertyName("unit_of_measure_qty")]
        public double UnitOfMeasureQty { get; set; }

        [JsonPropertyName("contract_multiplier")]
        public double ContractMultiplier { get; set; }

        [JsonPropertyName("strike_price")]
        public double StrikePrice { get; set; }

        [JsonPropertyName("raw_symbol")]
        public string RawSymbol { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("asset")]
        public string Asset { get; set; }

        /// <summary>CFI code (ISO 10962) classifying the instrument type.</summary>
        [JsonPropertyName("cfi")]
        public string Cfi { get; set; }

        [JsonPropertyName("security_type")]
        public string SecurityType { get; set; }

        /// <summary>
        /// Single-character instrument class: 'F' futures, 'C' call option, 'P' put option,
        /// 'K' FX forward, 'S' stock, 'M' mixed spread.
        /// </summary>
        [JsonPropertyName("instrument_class")]
        public string InstrumentClass { get; set; }

        [JsonPropertyName("underlying")]
        public string Underlying { get; set; }

        /// <summary>Action that caused this definition message: "A" add, "M" modify, "D" delete.</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
