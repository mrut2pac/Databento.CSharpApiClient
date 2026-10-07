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
    /// <remarks>
    /// A venue that doesn't set an integer field sends the largest value of its type as "undefined", e.g.
    /// 2147483647 for an <see cref="int"/>, 4294967295 for a <see cref="uint"/>, 65535 for a <see cref="ushort"/>,
    /// 255 for a <see cref="byte"/> and 127 for an <see cref="sbyte"/>. An undefined price reads as <see cref="double.NaN"/>.
    /// </remarks>
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

        /// <summary>Option strike price (display-scaled). <see cref="double.NaN"/> when undefined (e.g. non-option instruments).</summary>
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
        [JsonPropertyName("security_update_action")]
        public string Action { get; set; }

        /// <summary>Trading reference price (display-scaled); <see cref="double.NaN"/> when undefined. Sent in the DBN v1 layout only (e.g. OPRA.PILLAR, XNAS.ITCH).</summary>
        [JsonPropertyName("trading_reference_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double TradingReferencePrice { get; set; }

        /// <summary>Value of one <see cref="MinPriceIncrement"/> tick in the contract currency; <see cref="double.NaN"/> when undefined.</summary>
        [JsonPropertyName("min_price_increment_amount")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double MinPriceIncrementAmount { get; set; }

        /// <summary>Price ratio between the legs of a spread; <see cref="double.NaN"/> when undefined.</summary>
        [JsonPropertyName("price_ratio")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double PriceRatio { get; set; }

        /// <summary>Venue-defined bitmap of instrument eligibility attributes.</summary>
        [JsonPropertyName("inst_attrib_value")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int InstrumentAttributeValue { get; set; }

        /// <summary>Instrument ID of the underlying instrument; 0 when there is none.</summary>
        [JsonPropertyName("underlying_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint UnderlyingId { get; set; }

        /// <summary>The instrument ID the venue assigned.</summary>
        [JsonPropertyName("raw_instrument_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong RawInstrumentId { get; set; }

        /// <summary>Implied book depth the venue publishes.</summary>
        [JsonPropertyName("market_depth_implied")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MarketDepthImplied { get; set; }

        /// <summary>Outright book depth the venue publishes.</summary>
        [JsonPropertyName("market_depth")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MarketDepth { get; set; }

        /// <summary>Venue market segment.</summary>
        [JsonPropertyName("market_segment_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint MarketSegmentId { get; set; }

        /// <summary>Maximum order size.</summary>
        [JsonPropertyName("max_trade_vol")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint MaxTradeVolume { get; set; }

        /// <summary>Minimum lot size.</summary>
        [JsonPropertyName("min_lot_size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MinLotSize { get; set; }

        /// <summary>Minimum lot size for a block trade.</summary>
        [JsonPropertyName("min_lot_size_block")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MinLotSizeBlock { get; set; }

        /// <summary>Minimum lot size for a round lot.</summary>
        [JsonPropertyName("min_lot_size_round_lot")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MinLotSizeRoundLot { get; set; }

        /// <summary>Minimum order size.</summary>
        [JsonPropertyName("min_trade_vol")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint MinTradeVolume { get; set; }

        /// <summary>Contracts that decay daily, for a decaying contract.</summary>
        [JsonPropertyName("decay_quantity")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int DecayQuantity { get; set; }

        /// <summary>Contract size of each instrument before decay.</summary>
        [JsonPropertyName("original_contract_size")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int OriginalContractSize { get; set; }

        /// <summary>Trading session date of <see cref="TradingReferencePrice"/>, in days since the UNIX epoch. Sent in the DBN v1 layout only.</summary>
        [JsonPropertyName("trading_reference_date")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort TradingReferenceDate { get; set; }

        /// <summary>Venue channel (application) ID.</summary>
        [JsonPropertyName("appl_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public short ApplId { get; set; }

        /// <summary>Calendar year of the contract maturity.</summary>
        [JsonPropertyName("maturity_year")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort MaturityYear { get; set; }

        /// <summary>Date decay starts, in days since the UNIX epoch.</summary>
        [JsonPropertyName("decay_start_date")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort DecayStartDate { get; set; }

        /// <summary>Feed channel the instrument is published on.</summary>
        [JsonPropertyName("channel_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort ChannelId { get; set; }

        /// <summary>Currency of the prices (e.g. <c>"USD"</c>).</summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        /// <summary>Settlement currency, when it differs from <see cref="Currency"/>.</summary>
        [JsonPropertyName("settl_currency")]
        public string SettlementCurrency { get; set; }

        /// <summary>Strategy type of a spread.</summary>
        [JsonPropertyName("secsubtype")]
        public string SecuritySubType { get; set; }

        /// <summary>Product group code.</summary>
        [JsonPropertyName("group")]
        public string Group { get; set; }

        /// <summary>Unit of measure of the underlying (e.g. <c>"USD"</c>, <c>"IPNT"</c>).</summary>
        [JsonPropertyName("unit_of_measure")]
        public string UnitOfMeasure { get; set; }

        /// <summary>Currency of <see cref="StrikePrice"/>.</summary>
        [JsonPropertyName("strike_price_currency")]
        public string StrikePriceCurrency { get; set; }

        /// <summary>Venue matching algorithm code.</summary>
        [JsonPropertyName("match_algorithm")]
        public string MatchAlgorithm { get; set; }

        /// <summary>Trading status of the instrument. Sent in the DBN v1 layout only.</summary>
        [JsonPropertyName("md_security_trading_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte MdSecurityTradingStatus { get; set; }

        /// <summary>Price denominator of the main fraction.</summary>
        [JsonPropertyName("main_fraction")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte MainFraction { get; set; }

        /// <summary>Number of digits to the right of the tick mark, used for display.</summary>
        [JsonPropertyName("price_display_format")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte PriceDisplayFormat { get; set; }

        /// <summary>Settlement price type. Sent in the DBN v1 layout only.</summary>
        [JsonPropertyName("settl_price_type")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte SettlementPriceType { get; set; }

        /// <summary>Price denominator of the sub-fraction.</summary>
        [JsonPropertyName("sub_fraction")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte SubFraction { get; set; }

        /// <summary>Product complex of the instrument.</summary>
        [JsonPropertyName("underlying_product")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte UnderlyingProduct { get; set; }

        /// <summary>Calendar month of the contract maturity.</summary>
        [JsonPropertyName("maturity_month")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte MaturityMonth { get; set; }

        /// <summary>Calendar day of the contract maturity.</summary>
        [JsonPropertyName("maturity_day")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte MaturityDay { get; set; }

        /// <summary>Calendar week of the contract maturity.</summary>
        [JsonPropertyName("maturity_week")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte MaturityWeek { get; set; }

        /// <summary><c>"Y"</c> for a user-defined instrument, <c>"N"</c> otherwise.</summary>
        [JsonPropertyName("user_defined_instrument")]
        public string UserDefinedInstrument { get; set; }

        /// <summary>Type of <see cref="ContractMultiplier"/>.</summary>
        [JsonPropertyName("contract_multiplier_unit")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public sbyte ContractMultiplierUnit { get; set; }

        /// <summary>Schedule for delivering electricity.</summary>
        [JsonPropertyName("flow_schedule_type")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public sbyte FlowScheduleType { get; set; }

        /// <summary>Tick rule of the spread.</summary>
        [JsonPropertyName("tick_rule")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte TickRule { get; set; }

        /// <summary>Number of legs of a multi-leg instrument; 0 for an outright. Sent in the DBN v3 layout only (e.g. GLBX.MDP3), as are the other <c>Leg*</c> properties.</summary>
        [JsonPropertyName("leg_count")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort LegCount { get; set; }

        /// <summary>Index of the leg this record describes, for a multi-leg instrument.</summary>
        [JsonPropertyName("leg_index")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ushort LegIndex { get; set; }

        /// <summary>Instrument ID of the leg.</summary>
        [JsonPropertyName("leg_instrument_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint LegInstrumentId { get; set; }

        /// <summary>Raw symbol of the leg.</summary>
        [JsonPropertyName("leg_raw_symbol")]
        public string LegRawSymbol { get; set; }

        /// <summary>Instrument class of the leg; <see langword="null"/> for an outright.</summary>
        [JsonPropertyName("leg_instrument_class")]
        public string LegInstrumentClass { get; set; }

        /// <summary>Side of the leg: <c>"A"</c>, <c>"B"</c> or <c>"N"</c> (none).</summary>
        [JsonPropertyName("leg_side")]
        public string LegSide { get; set; }

        /// <summary>Price of the leg (display-scaled); <see cref="double.NaN"/> when undefined.</summary>
        [JsonPropertyName("leg_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double LegPrice { get; set; }

        /// <summary>Delta of the leg; <see cref="double.NaN"/> when undefined.</summary>
        [JsonPropertyName("leg_delta")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double LegDelta { get; set; }

        /// <summary>Numerator of the leg price ratio.</summary>
        [JsonPropertyName("leg_ratio_price_numerator")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LegRatioPriceNumerator { get; set; }

        /// <summary>Denominator of the leg price ratio.</summary>
        [JsonPropertyName("leg_ratio_price_denominator")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LegRatioPriceDenominator { get; set; }

        /// <summary>Numerator of the leg quantity ratio.</summary>
        [JsonPropertyName("leg_ratio_qty_numerator")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LegRatioQtyNumerator { get; set; }

        /// <summary>Denominator of the leg quantity ratio.</summary>
        [JsonPropertyName("leg_ratio_qty_denominator")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LegRatioQtyDenominator { get; set; }

        /// <summary>Instrument ID of the leg underlying.</summary>
        [JsonPropertyName("leg_underlying_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint LegUnderlyingId { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }

        /// <summary>The raw symbol this record belongs to. Populated only when the request covered more than one symbol; <see langword="null"/> otherwise.</summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }
    }
}
