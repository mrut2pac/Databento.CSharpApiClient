using System;

using Newtonsoft.Json;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// An instrument-definition record from the <c>definition</c> schema (JSON encoding).
    /// Carries point-in-time contract metadata for an instrument.
    /// Corresponds to DBN rtype <c>InstrumentDef</c>.
    /// </summary>
    public sealed class DefinitionRecordJson
    {
        [JsonProperty("hd")]
        public RecordHeaderJson Header { get; set; }

        [JsonProperty("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        [JsonProperty("min_price_increment")]
        public double MinPriceIncrement { get; set; }

        [JsonProperty("display_factor")]
        public double DisplayFactor { get; set; }

        [JsonProperty("expiration")]
        public DateTime? Expiration { get; set; }

        [JsonProperty("activation")]
        public DateTime? Activation { get; set; }

        [JsonProperty("high_limit_price")]
        public double HighLimitPrice { get; set; }

        [JsonProperty("low_limit_price")]
        public double LowLimitPrice { get; set; }

        [JsonProperty("max_price_variation")]
        public double MaxPriceVariation { get; set; }

        [JsonProperty("unit_of_measure_qty")]
        public double UnitOfMeasureQty { get; set; }

        [JsonProperty("contract_multiplier")]
        public double ContractMultiplier { get; set; }

        [JsonProperty("strike_price")]
        public double StrikePrice { get; set; }

        [JsonProperty("raw_symbol")]
        public string RawSymbol { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("asset")]
        public string Asset { get; set; }

        /// <summary>CFI code (ISO 10962) classifying the instrument type.</summary>
        [JsonProperty("cfi")]
        public string Cfi { get; set; }

        [JsonProperty("security_type")]
        public string SecurityType { get; set; }

        /// <summary>
        /// Single-character instrument class: 'F' futures, 'C' call option, 'P' put option,
        /// 'K' FX forward, 'S' stock, 'M' mixed spread.
        /// </summary>
        [JsonProperty("instrument_class")]
        public string InstrumentClass { get; set; }

        [JsonProperty("underlying")]
        public string Underlying { get; set; }

        /// <summary>Action that caused this definition message: "A" add, "M" modify, "D" delete.</summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
