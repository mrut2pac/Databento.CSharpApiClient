using System;
using System.Collections.Generic;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Offline tests of <see cref="ResponseModelGuard"/> itself, on hand-written responses; they need no API key.
    /// </summary>
    public class ResponseModelGuardTests
    {
        private const string Header = "\"hd\":{\"ts_event\":\"2024-05-01T14:00:00.000000000Z\",\"rtype\":0,\"publisher_id\":2,\"instrument_id\":15144}";

        private const string Trade = "{\"ts_recv\":\"2024-05-01T14:00:00.000000000Z\"," + Header + ",\"action\":\"T\",\"side\":\"B\",\"depth\":0,"
            + "\"price\":\"500.870000000\",\"size\":100,\"flags\":130,\"ts_in_delta\":0,\"sequence\":1}";

        private static IReadOnlyList<string> Check(string endpointAndQuery, string body, string contentType = "application/json", string requestBody = "")
            => ResponseModelGuard.Check(new CapturedResponse(new Uri("https://hist.databento.com/v0/" + endpointAndQuery), requestBody, contentType, body));

        [Fact]
        public void Check_RecordsMatchingTheirModel_ReportsNothing()
        {
            IReadOnlyList<string> problems = Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", Trade + "\n" + Trade + "\n");

            Assert.Empty(problems);
        }

        [Fact]
        public void Check_KeyNoPropertyMaps_ReportsItUnmapped()
        {
            string trade = Trade.Replace("\"sequence\":1}", "\"sequence\":1,\"new_field\":7}", StringComparison.Ordinal);

            IReadOnlyList<string> problems = Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", trade);

            Assert.Equal("unmapped key: TradeRecordJson has no property for \"new_field\" (timeseries.get_range)", Assert.Single(problems));
        }

        [Fact]
        public void Check_MappedKeyNoRecordCarries_ReportsItNeverSent()
        {
            string trade = Trade.Replace(",\"ts_in_delta\":0", string.Empty, StringComparison.Ordinal);

            IReadOnlyList<string> problems = Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", trade);

            Assert.Equal("never sent: TradeRecordJson.TsInDelta maps \"ts_in_delta\", which no object carries (timeseries.get_range)", Assert.Single(problems));
        }

        [Fact]
        public void Check_SymbolMissing_IsExpectedOnlyWhenSymbolsWereNotMapped()
        {
            Assert.Empty(Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", Trade));

            IReadOnlyList<string> problems = Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades&map_symbols=true", Trade);

            Assert.Contains(problems, problem => problem.StartsWith("never sent: TradeRecordJson.Symbol", StringComparison.Ordinal));
        }

        [Fact]
        public void Check_NestedDictionaryOfModels_ChecksEachValue()
        {
            // symbology.resolve: result is a map of symbol -> intervals, each interval its own model
            string body = "{\"result\":{\"SPY\":[{\"d0\":\"2024-05-01\",\"d1\":\"2024-05-02\",\"s\":\"15144\",\"x\":1}]},\"symbols\":[\"SPY\"],"
                + "\"stype_in\":\"raw_symbol\",\"stype_out\":\"instrument_id\",\"start_date\":\"2024-05-01\",\"end_date\":\"2024-05-02\","
                + "\"partial\":[],\"not_found\":[],\"message\":\"OK\",\"status\":0}";

            IReadOnlyList<string> problems = Check("symbology.resolve", body);

            Assert.Equal("unmapped key: MappedSymbol has no property for \"x\" (symbology.resolve)", Assert.Single(problems));
        }

        [Fact]
        public void Check_PrettyPrintedSingleObject_IsReadWhole()
        {
            string body = "{\n  \"start\": \"2018-05-01T00:00:00.000000000Z\",\n  \"end\": \"2026-10-07T00:00:00.000000000Z\",\n"
                + "  \"schema\": {\"mbo\": {\"start\": \"2018-05-01T00:00:00.000000000Z\", \"end\": \"2026-10-07T00:00:00.000000000Z\"}}\n}";

            Assert.Empty(Check("metadata.get_dataset_range?dataset=XNAS.ITCH", body));
        }

        [Fact]
        public void Check_KeyDifferingOnlyInCase_MatchesAsTheClientReadsIt()
        {
            Assert.Empty(Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", Trade.Replace("\"price\"", "\"Price\"", StringComparison.Ordinal)));
        }

        [Fact]
        public void Check_ResponseItCannotRead_FailsInsteadOfPassing()
        {
            Assert.StartsWith("unchecked:", Assert.Single(Check("timeseries.get_range?dataset=XNAS.ITCH&schema=trades", Trade, contentType: "text/plain")));
            Assert.StartsWith("unchecked:", Assert.Single(Check("timeseries.get_range?dataset=XNAS.ITCH&schema=new-schema", Trade)));
            Assert.StartsWith("unchecked:", Assert.Single(Check("metadata.new_endpoint", "[]")));
        }

        [Fact]
        public void Check_PlainValueOrDownload_IsNotChecked()
        {
            Assert.Empty(Check("metadata.get_record_count?dataset=XNAS.ITCH", "42"));
            Assert.Empty(Check("batch/download/job-1/file.dbn.zst", "binary", contentType: "application/octet-stream"));
        }

        [Fact]
        public void Check_DefinitionLayouts_MayOnlyMissTheOtherLayoutsKeys()
        {
            // a v3 record (it carries leg_count) that lost leg_price must fail; a v1 record never carries legs
            string v3 = DefinitionWith("\"leg_count\":0,\"leg_index\":0,\"leg_instrument_id\":0,\"leg_raw_symbol\":\"\",\"leg_instrument_class\":null,"
                + "\"leg_side\":\"N\",\"leg_delta\":null,\"leg_ratio_price_numerator\":0,\"leg_ratio_price_denominator\":0,"
                + "\"leg_ratio_qty_numerator\":0,\"leg_ratio_qty_denominator\":0,\"leg_underlying_id\":0");
            string v1 = DefinitionWith("\"trading_reference_price\":null,\"trading_reference_date\":65535,\"md_security_trading_status\":255,\"settl_price_type\":255");

            Assert.Empty(Check("timeseries.get_range?dataset=OPRA.PILLAR&schema=definition", v1));
            Assert.Contains(
                Check("timeseries.get_range?dataset=GLBX.MDP3&schema=definition", v3),
                problem => problem.StartsWith("never sent: DefinitionRecordJson.LegPrice", StringComparison.Ordinal));
        }

        [Fact]
        public void Check_SummaryEndpoint_RequiresOnlyMappedKeys()
        {
            string summary = "[{\"id\":\"XNAS-20261007-QNNA8K8TYH\",\"state\":\"done\",\"ts_received\":\"2026-10-07T15:27:59.842454000Z\"}]";

            Assert.Empty(Check("batch.list_jobs", summary));
            Assert.Single(Check("batch.list_jobs", summary.Replace("\"state\"", "\"status\"", StringComparison.Ordinal)));
        }

        // a definition carrying every key both layouts share, plus the layout-specific ones given
        private static string DefinitionWith(string layoutKeys)
            => "{\"ts_recv\":\"2024-05-01T00:00:00.000000000Z\"," + Header + ",\"raw_symbol\":\"X\",\"security_update_action\":\"A\","
                + "\"instrument_class\":\"F\",\"min_price_increment\":null,\"display_factor\":null,\"expiration\":null,\"activation\":null,"
                + "\"high_limit_price\":null,\"low_limit_price\":null,\"max_price_variation\":null,\"unit_of_measure_qty\":null,"
                + "\"min_price_increment_amount\":null,\"price_ratio\":null,\"inst_attrib_value\":0,\"underlying_id\":0,\"raw_instrument_id\":0,"
                + "\"market_depth_implied\":0,\"market_depth\":0,\"market_segment_id\":0,\"max_trade_vol\":0,\"min_lot_size\":0,"
                + "\"min_lot_size_block\":0,\"min_lot_size_round_lot\":0,\"min_trade_vol\":0,\"contract_multiplier\":0,\"decay_quantity\":0,"
                + "\"original_contract_size\":0,\"appl_id\":0,\"maturity_year\":0,\"decay_start_date\":0,\"channel_id\":0,\"currency\":\"\","
                + "\"settl_currency\":\"\",\"secsubtype\":\"\",\"group\":\"\",\"exchange\":\"\",\"asset\":\"\",\"cfi\":\"\",\"security_type\":\"\","
                + "\"unit_of_measure\":\"\",\"underlying\":\"\",\"strike_price_currency\":\"\",\"strike_price\":null,\"match_algorithm\":\" \","
                + "\"main_fraction\":0,\"price_display_format\":0,\"sub_fraction\":0,\"underlying_product\":0,\"maturity_month\":0,"
                + "\"maturity_day\":0,\"maturity_week\":0,\"user_defined_instrument\":\"N\",\"contract_multiplier_unit\":0,"
                + "\"flow_schedule_type\":0,\"tick_rule\":0," + layoutKeys + "}";

        [Fact]
        public void Check_SchemaInAPostedForm_SelectsTheModel()
        {
            string trade = Trade.Replace("\"sequence\":1}", "\"sequence\":1,\"new_field\":7}", StringComparison.Ordinal);

            IReadOnlyList<string> problems = Check("timeseries.get_range", trade, requestBody: "dataset=XNAS.ITCH&schema=trades");

            Assert.Contains(problems, problem => problem.Contains("new_field", StringComparison.Ordinal));
        }
    }
}
