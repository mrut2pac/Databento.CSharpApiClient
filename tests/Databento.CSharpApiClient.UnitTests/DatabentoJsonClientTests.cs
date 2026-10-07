// Ignore Spelling: Databento Mbo Mbp Bbo Tbbo Tcbbo Cmbp Json Cbbo Ohlcv

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;
using Databento.CSharpApiClient.Exceptions;
using Databento.CSharpApiClient.Transport;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="DatabentoJsonClient"/>: JSON deserialization, path construction,
    /// and no-data (empty-array) behaviour for every supported schema and metadata endpoint.
    /// Each test uses a mock <see cref="IHttpTransport"/> so no network calls are made.
    /// </summary>
    [TestClass]
    public class DatabentoJsonClientTests
    {
        private const string AnyDataset = "XNAS.ITCH";
        private const string AnySymbol = "SPY";
        private const string AnyApiKey = "test-api-key-0000";

        private static readonly DateTimeOffset AnyStart = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AnyEnd   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

        // =====================================================================
        // Helpers
        // =====================================================================

        private static DatabentoJsonClient BuildClient(string jsonLinesResponse)
        {
            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonLinesResponse, Encoding.UTF8, "application/json"),
                });
            transport.Setup(t => t.Dispose());

            return new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
        }

        private static DatabentoJsonClient BuildClientWithStatusCode(HttpStatusCode status, string body)
        {
            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                });
            transport.Setup(t => t.Dispose());

            return new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
        }

        private static DatabentoJsonClient BuildClientCapturingRequest(out List<HttpRequestMessage> captured)
            => BuildClientCapturingRequest(string.Empty, out captured);

        private static DatabentoJsonClient BuildClientCapturingRequest(string responseBody, out List<HttpRequestMessage> captured)
        {
            List<HttpRequestMessage> requests = new List<HttpRequestMessage>();
            captured = requests;

            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => requests.Add(request))
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
                });
            transport.Setup(t => t.Dispose());

            return new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
        }

        private static string MakeHeader(int publisherId = 1, int instrumentId = 42, string tsEvent = "2022-05-16T13:30:00.000000000Z", int rtype = 10)
            => $"\"hd\":{{\"rtype\":{rtype},\"publisher_id\":{publisherId},\"instrument_id\":{instrumentId},\"ts_event\":\"{tsEvent}\"}}";

        // =====================================================================
        // MBO
        // =====================================================================

        [TestMethod]
        public async Task GetMboAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string json = "{" + MakeHeader(rtype: 160) + ",\"price\":\"419.50\",\"size\":100,\"action\":\"A\",\"side\":\"A\","
                + "\"flags\":0,\"channel_id\":1,\"order_id\":123456,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":100,\"sequence\":1}";

            using DatabentoJsonClient client = BuildClient(json);
            MboRecordJson[] records = await client.GetMboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual((ushort)1, records[0].Header.PublisherId);
            Assert.AreEqual(42u, records[0].Header.InstrumentId);
            Assert.AreEqual(100u, records[0].Size);
            Assert.AreEqual(123456UL, records[0].OrderId);
            Assert.AreEqual((ushort)1, records[0].ChannelId);
        }

        [TestMethod]
        public async Task GetMboAsync_NoDataResponse_ReturnsEmptyArray()
        {
            string errorBody = "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"end is after available range\"}}";
            using DatabentoJsonClient client = BuildClientWithStatusCode(HttpStatusCode.UnprocessableEntity, errorBody);

            MboRecordJson[] records = await client.GetMboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(0, records.Length);
        }

        // =====================================================================
        // MBP-10
        // =====================================================================

        [TestMethod]
        public async Task GetMbp10Async_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.49\",\"ask_px\":\"419.51\",\"bid_sz\":10,\"ask_sz\":5,\"bid_ct\":2,\"ask_ct\":3}";
            string json = "{" + MakeHeader(rtype: 11) + ",\"price\":\"419.50\",\"size\":1,\"action\":\"A\",\"side\":\"A\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":50,\"sequence\":1,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            Mbp10RecordJson[] records = await client.GetMbp10Async(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsNotNull(records[0].Level1);
            Assert.AreEqual(10u, records[0].Level1.BidSize);
            Assert.AreEqual(5u, records[0].Level1.AskSize);
        }

        // =====================================================================
        // BBO-1s / BBO-1m
        // =====================================================================

        [TestMethod]
        public async Task GetBbo1sAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.49\",\"ask_px\":\"419.51\",\"bid_sz\":10,\"ask_sz\":5,\"bid_ct\":2,\"ask_ct\":3}";
            string json = "{" + MakeHeader(rtype: 70) + ",\"size\":0,\"side\":\"N\","
                + "\"flags\":0,\"ts_recv\":\"2022-05-16T13:30:01.000000000Z\","
                + "\"ts_in_delta\":0,\"sequence\":1,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            BboRecordJson[] records = await client.GetBbo1sAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsNotNull(records[0].Level1);
            Assert.IsTrue(records[0].Level1.AskPrice > 0);
        }

        [TestMethod]
        public async Task GetBbo1mAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.49\",\"ask_px\":\"419.51\",\"bid_sz\":100,\"ask_sz\":50,\"bid_ct\":5,\"ask_ct\":8}";
            string json = "{" + MakeHeader(rtype: 71) + ",\"size\":0,\"side\":\"N\","
                + "\"flags\":0,\"ts_recv\":\"2022-05-16T13:31:00.000000000Z\","
                + "\"ts_in_delta\":0,\"sequence\":2,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            BboRecordJson[] records = await client.GetBbo1mAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual(100u, records[0].Level1.BidSize);
        }

        [TestMethod]
        public async Task GetBbo1sAsync_NoDataResponse_ReturnsEmptyArray()
        {
            string errorBody = "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"end is after available range\"}}";
            using DatabentoJsonClient client = BuildClientWithStatusCode(HttpStatusCode.UnprocessableEntity, errorBody);

            BboRecordJson[] records = await client.GetBbo1sAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(0, records.Length);
        }

        // =====================================================================
        // TBBO
        // =====================================================================

        [TestMethod]
        public async Task GetTbboAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.49\",\"ask_px\":\"419.51\",\"bid_sz\":10,\"ask_sz\":5,\"bid_ct\":2,\"ask_ct\":3}";
            string json = "{" + MakeHeader(rtype: 19) + ",\"price\":\"419.50\",\"size\":200,\"action\":\"T\",\"side\":\"A\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":75,\"sequence\":10,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            TbboRecordJson[] records = await client.GetTbboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsTrue(records[0].Price > 0);
            Assert.AreEqual(200u, records[0].Size);
            Assert.IsNotNull(records[0].Level1);
        }

        // =====================================================================
        // TCBBO
        // =====================================================================

        [TestMethod]
        public async Task GetTcbboAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.48\",\"ask_px\":\"419.52\",\"bid_sz\":15,\"ask_sz\":20,\"bid_pb\":1,\"ask_pb\":2}";
            string json = "{" + MakeHeader(rtype: 20) + ",\"price\":\"419.50\",\"size\":100,\"action\":\"T\",\"side\":\"B\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":50,\"sequence\":5,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            TcbboRecordJson[] records = await client.GetTcbboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsNotNull(records[0].Level1);
            Assert.AreEqual(1, records[0].Level1.BidPublisherId);
            Assert.AreEqual(2, records[0].Level1.AskPublisherId);
        }

        // =====================================================================
        // CMBP-1
        // =====================================================================

        [TestMethod]
        public async Task GetCmbp1Async_ValidJsonLine_ReturnsParsedRecord()
        {
            string level = "{\"bid_px\":\"419.48\",\"ask_px\":\"419.52\",\"bid_sz\":25,\"ask_sz\":30,\"bid_pb\":3,\"ask_pb\":4}";
            string json = "{" + MakeHeader(rtype: 21) + ",\"price\":\"419.50\",\"size\":1,\"action\":\"M\",\"side\":\"A\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":25,\"sequence\":7,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            Cmbp1RecordJson[] records = await client.GetCmbp1Async(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual(25, records[0].Level1.BidSize);
        }

        // =====================================================================
        // Status
        // =====================================================================

        [TestMethod]
        public async Task GetStatusAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            // Shape of a live XNAS.ITCH status record: action, reason and trading_event are numeric codes.
            string json = "{" + MakeHeader(rtype: 18) + ",\"ts_recv\":\"2022-05-16T13:30:00.000000000Z\","
                + "\"action\":7,\"reason\":2,\"trading_event\":1,"
                + "\"is_trading\":\"N\",\"is_quoting\":\"~\",\"is_short_sell_restricted\":\"N\"}";

            using DatabentoJsonClient client = BuildClient(json);
            StatusRecordJson[] records = await client.GetStatusAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual((ushort)7, records[0].Action);
            Assert.AreEqual((ushort)2, records[0].Reason);
            Assert.AreEqual((ushort)1, records[0].TradingEvent);
            Assert.AreEqual("N", records[0].IsTrading);
            Assert.AreEqual("~", records[0].IsQuoting);
        }

        // =====================================================================
        // Statistics
        // =====================================================================

        [TestMethod]
        public async Task GetStatisticsAsync_UndefinedQuantity_ParsesTheInt64Sentinel()
        {
            // A live GLBX.MDP3 statistics record as the client requests it (pretty timestamps and prices): quantity is a 64-bit integer sent
            // as a string, INT64_MAX when undefined, and an undefined ts_ref is null.
            string json = "{\"ts_recv\":\"2024-03-15T00:22:48.057050737Z\",\"hd\":{\"ts_event\":\"2024-03-15T00:22:48.056637873Z\",\"rtype\":24,"
                + "\"publisher_id\":1,\"instrument_id\":17077},\"ts_ref\":null,\"price\":\"5155.250000000\",\"quantity\":\"9223372036854775807\","
                + "\"sequence\":58896890,\"ts_in_delta\":15240,\"stat_type\":5,\"channel_id\":0,\"update_action\":1,\"stat_flags\":0}";

            using DatabentoJsonClient client = BuildClient(json);
            StatisticsRecordJson[] records = await client.GetStatisticsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual(long.MaxValue, records[0].Quantity);
            Assert.IsNull(records[0].TsRefUtc);
            Assert.AreEqual(58896890u, records[0].Sequence);
            Assert.AreEqual((ushort)5, records[0].StatType);
        }

        [TestMethod]
        public async Task GetStatisticsAsync_QuantityBeyondInt32_ParsesIt()
        {
            string json = "{" + MakeHeader(rtype: 24) + ",\"ts_recv\":\"2024-03-15T00:22:48.057050737Z\","
                + "\"price\":\"5155.25\",\"quantity\":\"4294967296\",\"sequence\":1,\"ts_in_delta\":0,"
                + "\"stat_type\":6,\"channel_id\":0,\"update_action\":1,\"stat_flags\":0}";

            using DatabentoJsonClient client = BuildClient(json);
            StatisticsRecordJson[] records = await client.GetStatisticsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(4294967296L, records[0].Quantity);
        }

        // =====================================================================
        // Definition
        // =====================================================================

        [TestMethod]
        public async Task GetDefinitionsAsync_LiveOptionDefinition_ReadsTheSecurityUpdateAction()
        {
            // A live OPRA.PILLAR definition as the client requests it (pretty timestamps and prices), trimmed to the fields under test.
            string json = "{\"ts_recv\":\"2022-02-07T14:31:00.000000000Z\",\"hd\":{\"ts_event\":\"2022-02-07T14:31:00.000000000Z\",\"rtype\":19,"
                + "\"publisher_id\":30,\"instrument_id\":1310693},\"raw_symbol\":\"SPXW  220207C04295000\",\"security_update_action\":\"A\","
                + "\"instrument_class\":\"C\",\"expiration\":\"2022-02-07T00:00:00.000000000Z\",\"activation\":null,\"exchange\":\"OPRA\","
                + "\"asset\":\"SPXW\",\"security_type\":\"OPT\",\"underlying\":\"SPXW\",\"strike_price\":\"4295.000000000\"}";

            using DatabentoJsonClient client = BuildClient(json);
            DefinitionRecordJson[] records = await client.GetDefinitionsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual("A", records[0].Action);
            Assert.AreEqual("C", records[0].InstrumentClass);
            Assert.AreEqual(4295.0, records[0].StrikePrice, 1e-9);
        }

        [TestMethod]
        public async Task GetDefinitionsAsync_LiveGlbxFutureInDbnV3Layout_ReadsEveryField()
        {
            // a live GLBX.MDP3 definition, served in the DBN v3 layout: it carries the leg fields, and raw_instrument_id as a string
            string json = "{\"ts_recv\":\"2024-05-01T00:00:00.000000000Z\",\"hd\":{\"ts_event\":\"2024-04-28T11:03:56.364648873Z\","
                + "\"rtype\":19,\"publisher_id\":1,\"instrument_id\":5602},\"raw_symbol\":\"ESM4\","
                + "\"security_update_action\":\"A\",\"instrument_class\":\"F\",\"min_price_increment\":\"0.250000000\","
                + "\"display_factor\":\"0.010000000\",\"expiration\":\"2024-06-21T13:30:00.000000000Z\","
                + "\"activation\":\"2022-03-18T13:30:00.000000000Z\",\"high_limit_price\":\"5488.000000000\","
                + "\"low_limit_price\":\"4774.500000000\",\"max_price_variation\":\"6.000000000\","
                + "\"unit_of_measure_qty\":\"50.000000000\",\"min_price_increment_amount\":\"0.125000000\",\"price_ratio\":null,"
                + "\"inst_attrib_value\":270351,\"underlying_id\":0,\"raw_instrument_id\":\"5602\",\"market_depth_implied\":0,"
                + "\"market_depth\":10,\"market_segment_id\":64,\"max_trade_vol\":3000,\"min_lot_size\":0,"
                + "\"min_lot_size_block\":0,\"min_lot_size_round_lot\":0,\"min_trade_vol\":1,\"contract_multiplier\":2147483647,"
                + "\"decay_quantity\":2147483647,\"original_contract_size\":2147483647,\"appl_id\":310,\"maturity_year\":2024,"
                + "\"decay_start_date\":65535,\"channel_id\":0,\"currency\":\"USD\",\"settl_currency\":\"\",\"secsubtype\":\"\","
                + "\"group\":\"ES\",\"exchange\":\"XCME\",\"asset\":\"ES\",\"cfi\":\"FFIXSX\",\"security_type\":\"FUT\","
                + "\"unit_of_measure\":\"IPNT\",\"underlying\":\"\",\"strike_price_currency\":\"\",\"strike_price\":null,"
                + "\"match_algorithm\":\"F\",\"main_fraction\":255,\"price_display_format\":255,\"sub_fraction\":255,"
                + "\"underlying_product\":5,\"maturity_month\":6,\"maturity_day\":255,\"maturity_week\":255,"
                + "\"user_defined_instrument\":\"N\",\"contract_multiplier_unit\":127,\"flow_schedule_type\":127,"
                + "\"tick_rule\":255,\"leg_count\":0,\"leg_index\":0,\"leg_instrument_id\":0,\"leg_raw_symbol\":\"\","
                + "\"leg_instrument_class\":null,\"leg_side\":\"N\",\"leg_price\":null,\"leg_delta\":null,"
                + "\"leg_ratio_price_numerator\":0,\"leg_ratio_price_denominator\":0,\"leg_ratio_qty_numerator\":0,"
                + "\"leg_ratio_qty_denominator\":0,\"leg_underlying_id\":0}";

            using DatabentoJsonClient client = BuildClient(json);
            DefinitionRecordJson record = (await client.GetDefinitionsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd))[0];

            Assert.AreEqual(5602UL, record.RawInstrumentId);
            Assert.AreEqual(0.125, record.MinPriceIncrementAmount, 1e-9);
            Assert.IsTrue(double.IsNaN(record.PriceRatio));
            Assert.AreEqual(270351, record.InstrumentAttributeValue);
            Assert.AreEqual(10, record.MarketDepth);
            Assert.AreEqual(64u, record.MarketSegmentId);
            Assert.AreEqual(3000u, record.MaxTradeVolume);
            Assert.AreEqual((short)310, record.ApplId);
            Assert.AreEqual((ushort)2024, record.MaturityYear);
            Assert.AreEqual((byte)6, record.MaturityMonth);
            Assert.AreEqual("USD", record.Currency);
            Assert.AreEqual("ES", record.Group);
            Assert.AreEqual("IPNT", record.UnitOfMeasure);
            Assert.AreEqual("F", record.MatchAlgorithm);
            Assert.AreEqual((byte)5, record.UnderlyingProduct);
            Assert.AreEqual((sbyte)127, record.ContractMultiplierUnit);
            Assert.AreEqual((ushort)0, record.LegCount);
            Assert.AreEqual("N", record.LegSide);
            Assert.IsNull(record.LegInstrumentClass);
            Assert.IsTrue(double.IsNaN(record.LegPrice));
        }

        [TestMethod]
        public async Task GetDefinitionsAsync_LiveOpraOptionInDbnV1Layout_ReadsEveryField()
        {
            // a live OPRA.PILLAR definition, served in the DBN v1 layout: no leg fields, raw_instrument_id as a number
            string json = "{\"ts_recv\":\"2024-05-01T10:30:00.680511059Z\",\"hd\":{\"ts_event\":\"2024-05-01T10:30:00.680302848Z\","
                + "\"rtype\":19,\"publisher_id\":30,\"instrument_id\":1275068601},\"raw_symbol\":\"SPY   240501P00501000\","
                + "\"security_update_action\":\"A\",\"instrument_class\":\"P\",\"min_price_increment\":null,"
                + "\"display_factor\":null,\"expiration\":\"2024-05-01T00:00:00.000000000Z\",\"activation\":null,"
                + "\"high_limit_price\":null,\"low_limit_price\":null,\"max_price_variation\":null,"
                + "\"trading_reference_price\":null,\"unit_of_measure_qty\":null,\"min_price_increment_amount\":null,"
                + "\"price_ratio\":null,\"inst_attrib_value\":2147483647,\"underlying_id\":1308622850,"
                + "\"raw_instrument_id\":1275068601,\"market_depth_implied\":2147483647,\"market_depth\":2147483647,"
                + "\"market_segment_id\":4294967295,\"max_trade_vol\":4294967295,\"min_lot_size\":2147483647,"
                + "\"min_lot_size_block\":2147483647,\"min_lot_size_round_lot\":2147483647,\"min_trade_vol\":4294967295,"
                + "\"contract_multiplier\":2147483647,\"decay_quantity\":2147483647,\"original_contract_size\":2147483647,"
                + "\"trading_reference_date\":65535,\"appl_id\":32767,\"maturity_year\":65535,\"decay_start_date\":65535,"
                + "\"channel_id\":76,\"currency\":\"USD\",\"settl_currency\":\"\",\"secsubtype\":\"\",\"group\":\"popra-77\","
                + "\"exchange\":\"OPRA\",\"asset\":\"SPY\",\"cfi\":\"\",\"security_type\":\"OPT\",\"unit_of_measure\":\"USD\","
                + "\"underlying\":\"SPY\",\"strike_price_currency\":\"USD\",\"strike_price\":\"501.000000000\","
                + "\"match_algorithm\":\" \",\"md_security_trading_status\":255,\"main_fraction\":255,"
                + "\"price_display_format\":255,\"settl_price_type\":255,\"sub_fraction\":255,\"underlying_product\":255,"
                + "\"maturity_month\":255,\"maturity_day\":255,\"maturity_week\":255,\"user_defined_instrument\":\"N\","
                + "\"contract_multiplier_unit\":127,\"flow_schedule_type\":127,\"tick_rule\":255}";

            using DatabentoJsonClient client = BuildClient(json);
            DefinitionRecordJson record = (await client.GetDefinitionsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd))[0];

            Assert.AreEqual(1275068601UL, record.RawInstrumentId);
            Assert.AreEqual(1308622850u, record.UnderlyingId);
            Assert.IsTrue(double.IsNaN(record.TradingReferencePrice));
            Assert.AreEqual((ushort)65535, record.TradingReferenceDate);
            Assert.AreEqual((ushort)76, record.ChannelId);
            Assert.AreEqual("popra-77", record.Group);
            Assert.AreEqual("USD", record.StrikePriceCurrency);
            Assert.AreEqual((byte)255, record.MdSecurityTradingStatus);
            Assert.AreEqual((byte)255, record.SettlementPriceType);
            Assert.AreEqual("N", record.UserDefinedInstrument);
            Assert.AreEqual(501.0, record.StrikePrice, 1e-9);
        }

        // =====================================================================
        // Imbalance
        // =====================================================================

        [TestMethod]
        public async Task GetImbalanceAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string json = "{" + MakeHeader(rtype: 14) + ",\"ts_recv\":\"2022-05-16T19:59:58.000000000Z\","
                + "\"ref_price\":\"419.50\",\"cont_book_clr_price\":\"419.48\",\"auct_interest_clr_price\":\"419.47\","
                + "\"ssr_filling_price\":\"0\",\"ind_match_price\":\"419.50\",\"upper_collar\":\"420.00\","
                + "\"lower_collar\":\"419.00\",\"paired_qty\":1000,\"total_imbalance_qty\":5000,"
                + "\"market_imbalance_qty\":0,\"unpaired_qty\":5000,\"auction_type\":\"C\","
                + "\"side\":\"B\",\"auction_status\":0,\"freeze_status\":0,\"num_extensions\":0,"
                + "\"unpaired_side\":\"B\",\"significant_imbalance\":\"N\"}";

            using DatabentoJsonClient client = BuildClient(json);
            ImbalanceRecordJson[] records = await client.GetImbalanceAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual("C", records[0].AuctionType);
            Assert.AreEqual("B", records[0].Side);
            Assert.AreEqual(1000u, records[0].PairedQty);
            Assert.AreEqual(5000u, records[0].TotalImbalanceQty);
        }

        [TestMethod]
        public async Task GetImbalanceAsync_AuctionTime_ReadsTheScheduledTimeOrNull()
        {
            // the auction_time values ARCX.PILLAR (closing auction) and XNAS.ITCH send live
            string json = "{\"ts_recv\":\"2024-05-01T19:55:00.000000000Z\",\"hd\":{\"ts_event\":\"2024-05-01T19:55:00.000000000Z\",\"rtype\":20,"
                + "\"publisher_id\":12,\"instrument_id\":1},\"auction_time\":\"2024-05-01T16:00:00.000000000Z\"}\n"
                + "{\"ts_recv\":\"2024-05-01T19:50:00.039320280Z\",\"hd\":{\"ts_event\":\"2024-05-01T19:50:00.039148508Z\",\"rtype\":20,"
                + "\"publisher_id\":2,\"instrument_id\":15144},\"auction_time\":null}";

            using DatabentoJsonClient client = BuildClient(json);
            ImbalanceRecordJson[] records = await client.GetImbalanceAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(new DateTime(2024, 5, 1, 16, 0, 0, DateTimeKind.Utc), records[0].AuctionTime.Value.ToUniversalTime());
            Assert.IsNull(records[1].AuctionTime);
        }

        // =====================================================================
        // SymbolMapping
        // =====================================================================

        [TestMethod]
        public void MethodsTheHistoricalApiCannotServe_AreObsolete()
        {
            // timeseries.get_range has no symbol_mapping schema, and no dataset lists ohlcv-eod
            string[] names = { "GetSymbolMappings", "GetSymbolMappingsAsync", "GetOhlcvEod", "GetOhlcvEodAsync" };
            int checkedMethods = 0;
            foreach(Type type in new[] { typeof(DatabentoJsonClient), typeof(DatabentoClient) })
            {
                foreach(System.Reflection.MethodInfo method in type.GetMethods())
                {
                    if(Array.IndexOf(names, method.Name) >= 0)
                    {
                        Assert.IsNotNull(Attribute.GetCustomAttribute(method, typeof(ObsoleteAttribute)), type.Name + "." + method.Name);
                        checkedMethods++;
                    }
                }
            }

            Assert.AreEqual(12, checkedMethods);
        }

        [TestMethod]
        public void PropertiesTheApiNeverSends_AreObsolete()
        {
            (Type Type, string Property)[] neverSent =
            {
                (typeof(UnitPriceInfo), "UnitPrice"),
                (typeof(DatasetCondition), "DateGenerated"),
                (typeof(PublisherInfo), "Name"),
                (typeof(FieldInfo), "Description"),
                (typeof(BboRecordJson), "TsInDelta"),
                (typeof(TcbboRecordJson), "Depth"),
                (typeof(TcbboRecordJson), "Sequence"),
                (typeof(Cmbp1RecordJson), "Depth"),
                (typeof(Cmbp1RecordJson), "Sequence"),
                (typeof(DataModel.Dbn.BboRecordDbn), "TsInDelta"),
                (typeof(DataModel.Dbn.TcbboRecordDbn), "Depth"),
                (typeof(DataModel.Dbn.TcbboRecordDbn), "Sequence"),
                (typeof(DataModel.Dbn.Cmbp1RecordDbn), "Depth"),
                (typeof(DataModel.Dbn.Cmbp1RecordDbn), "Sequence"),
            };

            foreach((Type type, string property) in neverSent)
            {
                Assert.IsNotNull(Attribute.GetCustomAttribute(type.GetProperty(property), typeof(ObsoleteAttribute)), type.Name + "." + property);
            }
        }

        // =====================================================================
        // Multi-record JSON-lines stream
        // =====================================================================

        [TestMethod]
        public async Task GetTbboAsync_MultiRecordStream_ReturnsAllRecords()
        {
            string level = "{\"bid_px\":\"419.49\",\"ask_px\":\"419.51\",\"bid_sz\":10,\"ask_sz\":5,\"bid_ct\":1,\"ask_ct\":2}";
            string line1 = "{" + MakeHeader() + ",\"price\":\"419.50\",\"size\":100,\"action\":\"T\",\"side\":\"A\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:00.000000100Z\","
                + "\"ts_in_delta\":50,\"sequence\":1,\"levels\":[" + level + "]}";
            string line2 = "{" + MakeHeader() + ",\"price\":\"419.51\",\"size\":200,\"action\":\"T\",\"side\":\"B\","
                + "\"flags\":0,\"depth\":0,\"ts_recv\":\"2022-05-16T13:30:01.000000100Z\","
                + "\"ts_in_delta\":60,\"sequence\":2,\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(line1 + "\n" + line2);
            TbboRecordJson[] records = await client.GetTbboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(2, records.Length);
        }

        // =====================================================================
        // Metadata: GetRecordCount
        // =====================================================================

        [TestMethod]
        public async Task GetRecordCountAsync_BareLong_ReturnsParsedValue()
        {
            using DatabentoJsonClient client = BuildClient("22010");
            long count = await client.GetRecordCountAsync(AnyDataset, new[] { AnySymbol }, Schema.Trades, AnyStart, AnyEnd);

            Assert.AreEqual(22010L, count);
        }

        // =====================================================================
        // Metadata: GetBillableSize
        // =====================================================================

        [TestMethod]
        public async Task GetBillableSizeAsync_BareLong_ReturnsParsedValue()
        {
            using DatabentoJsonClient client = BuildClient("4842200");
            long bytes = await client.GetBillableSizeAsync(AnyDataset, new[] { AnySymbol }, Schema.Trades, AnyStart, AnyEnd);

            Assert.AreEqual(4842200L, bytes);
        }

        // =====================================================================
        // Metadata: GetCost
        // =====================================================================

        [TestMethod]
        public async Task GetCostAsync_BareDouble_ReturnsParsedValue()
        {
            using DatabentoJsonClient client = BuildClient("3.75");
            double cost = await client.GetCostAsync(AnyDataset, new[] { AnySymbol }, Schema.Trades, AnyStart, AnyEnd);

            Assert.AreEqual(3.75, cost, delta: 0.0001);
        }

        // =====================================================================
        // Metadata: ListConditions
        // =====================================================================

        // A live metadata.get_dataset_condition response: one element per day, oldest first
        private const string TwoDayConditions = "[{\"date\":\"2026-10-06\",\"condition\":\"available\",\"last_modified_date\":\"2026-10-07\"},"
            + "{\"date\":\"2026-10-07\",\"condition\":\"degraded\",\"last_modified_date\":\"2026-10-07\"}]";

        [TestMethod]
        public async Task ListConditionsAsync_ArrayResponse_ReturnsParsedConditions()
        {
            using DatabentoJsonClient client = BuildClient(TwoDayConditions);
            DatasetCondition[] conditions = await client.ListConditionsAsync("OPRA.PILLAR");

            Assert.AreEqual(2, conditions.Length);
            Assert.AreEqual("OPRA.PILLAR", conditions[0].Dataset);
            Assert.AreEqual("2026-10-06", conditions[0].Date);
            Assert.AreEqual("2026-10-07", conditions[1].Date);
        }

        [TestMethod]
        public async Task ListConditionsAsync_DateRange_RequestsGetDatasetConditionWithTheRange()
        {
            // metadata.list_conditions doesn't exist; the per-day list comes from get_dataset_condition
            using DatabentoJsonClient client = BuildClientCapturingRequest("[]", out List<HttpRequestMessage> captured);
            await client.ListConditionsAsync("XNAS.ITCH", "2026-09-30", "2026-10-02");

            Assert.AreEqual(1, captured.Count);
            string uri = captured[0].RequestUri.ToString();
            StringAssert.Contains(uri, "metadata.get_dataset_condition?dataset=XNAS.ITCH");
            StringAssert.Contains(uri, "start_date=2026-09-30");
            StringAssert.Contains(uri, "end_date=2026-10-02");
        }

        // =====================================================================
        // Metadata: GetDatasetCondition
        // =====================================================================

        [TestMethod]
        public async Task GetDatasetConditionAsync_Date_RequestsThatDayOnly()
        {
            // the endpoint ignores "date" and answers every day of the dataset
            using DatabentoJsonClient client = BuildClientCapturingRequest("[]", out List<HttpRequestMessage> captured);
            await client.GetDatasetConditionAsync("XNAS.ITCH", "2022-05-16");

            Assert.AreEqual(1, captured.Count);
            string uri = captured[0].RequestUri.ToString();
            StringAssert.Contains(uri, "start_date=2022-05-16");
            StringAssert.Contains(uri, "end_date=2022-05-16");
            Assert.IsFalse(uri.Contains("&date="), uri);
        }

        [TestMethod]
        public async Task GetDatasetConditionAsync_Date_ReturnsThatDay()
        {
            string json = "[{\"date\":\"2022-05-16\",\"condition\":\"available\",\"last_modified_date\":\"2025-11-27\"}]";

            using DatabentoJsonClient client = BuildClient(json);
            DatasetCondition condition = await client.GetDatasetConditionAsync("XNAS.ITCH", "2022-05-16");

            Assert.AreEqual("2022-05-16", condition.Date);
            Assert.AreEqual("available", condition.Condition);
            Assert.AreEqual("2025-11-27", condition.LastModifiedDate);
            Assert.AreEqual("XNAS.ITCH", condition.Dataset);
        }

        [TestMethod]
        public async Task GetDatasetConditionAsync_NoDate_ReturnsTheMostRecentDay()
        {
            using DatabentoJsonClient client = BuildClient(TwoDayConditions);
            DatasetCondition condition = await client.GetDatasetConditionAsync("XNAS.ITCH");

            Assert.AreEqual("2026-10-07", condition.Date);
            Assert.AreEqual("degraded", condition.Condition);
        }

        [TestMethod]
        public async Task GetDatasetConditionAsync_NoDate_ReturnsTheMostRecentDayWhateverTheOrder()
        {
            string newestFirst = "[{\"date\":\"2026-10-07\",\"condition\":\"degraded\",\"last_modified_date\":\"2026-10-07\"},"
                + "{\"date\":\"2026-10-06\",\"condition\":\"available\",\"last_modified_date\":\"2026-10-07\"}]";

            using DatabentoJsonClient client = BuildClient(newestFirst);
            DatasetCondition condition = await client.GetDatasetConditionAsync("XNAS.ITCH");

            Assert.AreEqual("2026-10-07", condition.Date);
        }

        [TestMethod]
        public async Task GetDatasetConditionAsync_NoDate_RequestsNoRange()
        {
            using DatabentoJsonClient client = BuildClientCapturingRequest("[]", out List<HttpRequestMessage> captured);
            await client.GetDatasetConditionAsync("XNAS.ITCH");

            Assert.AreEqual(1, captured.Count);
            string uri = captured[0].RequestUri.ToString();
            Assert.IsFalse(uri.Contains("start_date"), uri);
            Assert.IsFalse(uri.Contains("end_date"), uri);
        }

        [TestMethod]
        public async Task GetDatasetConditionAsync_NoDays_ReturnsNull()
        {
            using DatabentoJsonClient client = BuildClient("[]");

            Assert.IsNull(await client.GetDatasetConditionAsync("XNAS.ITCH", "2022-05-15"));
        }

        // =====================================================================
        // Metadata: GetDatasetRange
        // =====================================================================

        [TestMethod]
        public async Task GetDatasetRangeAsync_LiveResponse_MapsTheRangeOfEachSchema()
        {
            // a live metadata.get_dataset_range response, shortened to two schemas
            string json = "{\"start\":\"2018-05-01T00:00:00.000000000Z\",\"end\":\"2026-10-07T04:00:00.000000000Z\",\"schema\":{"
                + "\"mbo\":{\"start\":\"2018-05-01T00:00:00.000000000Z\",\"end\":\"2026-10-07T04:00:00.000000000Z\"},"
                + "\"ohlcv-1d\":{\"start\":\"2018-05-01T00:00:00.000000000Z\",\"end\":\"2026-10-06T00:00:00.000000000Z\"}}}";

            using DatabentoJsonClient client = BuildClient(json);
            DateRange range = await client.GetDatasetRangeAsync("XNAS.ITCH");

            Assert.AreEqual(new DateTimeOffset(2018, 5, 1, 0, 0, 0, TimeSpan.Zero), range.Start);
            Assert.AreEqual(2, range.Schemas.Count);
            Assert.AreEqual(new DateTimeOffset(2026, 10, 7, 4, 0, 0, TimeSpan.Zero), range.Schemas[Schema.Mbo].End);
            Assert.AreEqual(new DateTimeOffset(2026, 10, 6, 0, 0, 0, TimeSpan.Zero), range.Schemas[Schema.Ohlcv1Day].End);
        }

        // =====================================================================
        // Metadata: ListUnitPrices
        // =====================================================================

        [TestMethod]
        public async Task ListUnitPricesAsync_LiveResponse_MapsThePricePerSchema()
        {
            // a live metadata.list_unit_prices response (shortened): one map of schema -> price per mode
            string json = "[{\"mode\":\"historical\",\"unit_prices\":{\"mbo\":1.2,\"trades\":6.0,\"ohlcv-1d\":30.0}},"
                + "{\"mode\":\"live\",\"unit_prices\":{\"mbo\":0.6,\"trades\":3.0}}]";

            using DatabentoJsonClient client = BuildClient(json);
            UnitPriceInfo[] prices = await client.ListUnitPricesAsync("XNAS.ITCH");

            Assert.AreEqual(2, prices.Length);
            Assert.AreEqual("historical", prices[0].Mode);
            Assert.AreEqual(3, prices[0].UnitPrices.Count);
            Assert.AreEqual(1.2m, prices[0].UnitPrices["mbo"]);
            Assert.AreEqual(30.0m, prices[0].UnitPrices["ohlcv-1d"]);
            Assert.AreEqual(3.0m, prices[1].UnitPrices[Schema.Trades]);
        }

        // =====================================================================
        // Error: unrecognised 422 is rethrown
        // =====================================================================

        [TestMethod]
        public async Task GetMboAsync_UnrecognisedError422_ThrowsDatabentoHttpException()
        {
            string errorBody = "{\"detail\":{\"case\":\"symbology_invalid_request\",\"message\":\"none resolved\"}}";
            using DatabentoJsonClient client = BuildClientWithStatusCode(HttpStatusCode.UnprocessableEntity, errorBody);

            await Assert.ThrowsExceptionAsync<DatabentoHttpException>(() =>
                client.GetMboAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd));
        }

        // =====================================================================
        // Error: an unparseable record throws instead of being silently dropped
        // =====================================================================

        [TestMethod]
        public async Task GetCbbo1mAsync_UnparseableRecord_ThrowsDatabentoException()
        {
            // A record whose numeric field cannot be parsed is a real error, not "no data". It must
            // surface as an exception rather than being silently skipped, which previously caused the
            // client to return zero rows for responses that actually contained data.
            string json = "{" + MakeHeader(rtype: 193) + ",\"side\":\"N\",\"price\":\"not_a_number\",\"size\":1,\"flags\":200}";

            using DatabentoJsonClient client = BuildClient(json);

            await Assert.ThrowsExceptionAsync<DatabentoException>(() =>
                client.GetCbbo1mAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd));
        }

        // =====================================================================
        // Regression: string-encoded numeric fields parse (offline, runs in CI)
        // =====================================================================

        [TestMethod]
        public async Task GetCbbo1mAsync_StringEncodedPrice_ParsesIntoNumericField()
        {
            // The headline regression (#19): with pretty_px=true the API returns prices as JSON
            // strings ("price":"4.000000000"). They must read into the numeric DTO properties and the
            // record must survive — previously the whole response collapsed to zero records.
            string level = "{\"bid_px\":\"3.700000000\",\"ask_px\":\"3.900000000\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":0,\"ask_pb\":0}";
            string json = "{" + MakeHeader(rtype: 193) + ",\"side\":\"N\",\"price\":\"4.000000000\",\"size\":18,\"flags\":200,"
                + "\"ts_recv\":\"2023-11-08T14:31:00.000000000Z\",\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            CbboRecordJson[] records = await client.GetCbbo1mAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsTrue(records[0].Price.HasValue);
            Assert.AreEqual(4.0, records[0].Price.Value);
            Assert.IsNotNull(records[0].Level1);
            Assert.AreEqual(3.7, records[0].Level1.BidPrice);
        }
        // =====================================================================
        // map_symbols
        // =====================================================================

        [TestMethod]
        public async Task GetCbbo1mAsync_SingleSymbol_DoesNotRequestMapSymbols()
        {
            // The caller already knows the symbol, and map_symbols repeats it on every record rather
            // than sending it once - so asking for it here is pure payload for no information.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetCbbo1mAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "symbols=SPY");
            Assert.IsFalse(captured[0].RequestUri.ToString().Contains("map_symbols"));
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_SeveralSymbols_RequestsMapSymbols()
        {
            // The response interleaves the symbols, so without this each record could only be attributed
            // by resolving instrument ids through a second symbology call.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetCbbo1mAsync(AnyDataset, new[] { "SPY", "QQQ" }, AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "map_symbols=true");
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_AllSymbols_RequestsMapSymbols()
        {
            // One entry, but it stands for the whole dataset - so the response is multi-symbol.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetCbbo1mAsync(AnyDataset, new[] { "ALL_SYMBOLS" }, AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "map_symbols=true");
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_AllSymbolsThroughTheSingleSymbolOverload_RequestsMapSymbols()
        {
            // The single-symbol overload wraps its argument into a one-element list, so ALL_SYMBOLS has to
            // be recognised there too - it is one entry that stands for the whole dataset.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetCbbo1mAsync(AnyDataset, "ALL_SYMBOLS", AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "map_symbols=true");
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_AllSymbolsMixedWithAnother_RequestsMapSymbols()
        {
            // ALL_SYMBOLS is documented as the sole entry, but a list carrying it alongside another symbol
            // is still multi-symbol, so the records must name themselves either way.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetCbbo1mAsync(AnyDataset, new[] { "SPY", "ALL_SYMBOLS" }, AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "map_symbols=true");
        }

        [TestMethod]
        public async Task GetOhlcv1mAsync_SeveralSymbols_RequestsMapSymbols()
        {
            // The rule lives in the shared query builder, so it holds for every timeseries schema.
            using DatabentoJsonClient client = BuildClientCapturingRequest(out List<HttpRequestMessage> captured);
            await client.GetOhlcv1mAsync(AnyDataset, new[] { "SPY", "QQQ" }, AnyStart, AnyEnd);

            Assert.AreEqual(1, captured.Count);
            StringAssert.Contains(captured[0].RequestUri.ToString(), "map_symbols=true");
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_ResponseCarriesSymbol_PopulatesIt()
        {
            string level = "{\"bid_px\":\"3.70\",\"ask_px\":\"3.90\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":0,\"ask_pb\":0}";
            string json = "{" + MakeHeader(rtype: 193) + ",\"side\":\"N\",\"price\":\"4.00\",\"size\":18,\"flags\":200,"
                + "\"ts_recv\":\"2023-11-08T14:31:00.000000000Z\",\"levels\":[" + level + "],\"symbol\":\"QQQ\"}";

            using DatabentoJsonClient client = BuildClient(json);
            CbboRecordJson[] records = await client.GetCbbo1mAsync(AnyDataset, new[] { "SPY", "QQQ" }, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual("QQQ", records[0].Symbol);
        }

        [TestMethod]
        public async Task GetCbbo1mAsync_ResponseWithoutSymbol_LeavesItNull()
        {
            // A single-symbol request never asks for the field, so the record must still parse without it.
            string level = "{\"bid_px\":\"3.70\",\"ask_px\":\"3.90\",\"bid_sz\":185,\"ask_sz\":147,\"bid_pb\":0,\"ask_pb\":0}";
            string json = "{" + MakeHeader(rtype: 193) + ",\"side\":\"N\",\"price\":\"4.00\",\"size\":18,\"flags\":200,"
                + "\"ts_recv\":\"2023-11-08T14:31:00.000000000Z\",\"levels\":[" + level + "]}";

            using DatabentoJsonClient client = BuildClient(json);
            CbboRecordJson[] records = await client.GetCbbo1mAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.IsNull(records[0].Symbol);
        }

        // =====================================================================
        // Symbology
        // =====================================================================

        [TestMethod]
        public async Task ResolveSymbolsAsync_SeveralSymbols_SendsThemInOneCommaJoinedField()
        {
            // symbology.resolve keeps only the last of repeated "symbols" fields, so every symbol has to travel in one field
            List<string> capturedBodies = new List<string>();
            Mock<IHttpTransport> transport = new Mock<IHttpTransport>(MockBehavior.Strict);
            transport
                .Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedBodies.Add(request.Content.ReadAsStringAsync().GetAwaiter().GetResult()))
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":{}}", Encoding.UTF8, "application/json"),
                });
            transport.Setup(t => t.Dispose());

            using DatabentoJsonClient client = new DatabentoJsonClient(new DatabentoOptions { ApiKey = AnyApiKey }, transport.Object);
            await client.ResolveSymbolsAsync(new SymbologyRequest
            {
                Dataset = Datasets.OpraPillar,
                Symbols = new[] { "SPXW  140207C01275000", "SPXW  140207P02050000" },
                StartDate = "2014-02-07",
                EndDate = "2014-02-08",
            });

            Assert.AreEqual(1, capturedBodies.Count);
            string[] symbolFields = Array.FindAll(capturedBodies[0].Split('&'), field => field.StartsWith("symbols=", StringComparison.Ordinal));
            Assert.AreEqual(1, symbolFields.Length);
            Assert.AreEqual("SPXW  140207C01275000,SPXW  140207P02050000", WebUtility.UrlDecode(symbolFields[0].Substring("symbols=".Length)));
        }

        [TestMethod]
        public async Task ResolveSymbolsAsync_IntervalsAsTheApiSendsThem_MapTheirSymbolAndDates()
        {
            // a live symbology.resolve response: each interval is {"d0","d1","s"}, not named after the request's fields
            string json = "{\"result\":{\"SPY   220520P00450000\":[{\"d0\":\"2021-10-27\",\"d1\":\"2022-05-21\",\"s\":\"240846\"}]},"
                + "\"symbols\":[\"SPY   220520P00450000\"],\"stype_in\":\"raw_symbol\",\"stype_out\":\"instrument_id\","
                + "\"start_date\":\"2016-05-20\",\"end_date\":\"2022-05-21\",\"partial\":[\"SPY   220520P00450000\"],\"not_found\":[],"
                + "\"message\":\"Partially resolved\",\"status\":1}";

            using DatabentoJsonClient client = BuildClient(json);
            SymbologyResolution resolution = await client.ResolveSymbolsAsync(new SymbologyRequest
            {
                Dataset = Datasets.OpraPillar,
                Symbols = new[] { "SPY   220520P00450000" },
                StartDate = "2016-05-20",
                EndDate = "2022-05-21",
            });

            MappedSymbol[] intervals = resolution.Result["SPY   220520P00450000"];
            Assert.AreEqual(1, intervals.Length);
            Assert.AreEqual("240846", intervals[0].Symbol);
            Assert.AreEqual("2021-10-27", intervals[0].StartDate);
            Assert.AreEqual("2022-05-21", intervals[0].EndDate);
            Assert.AreEqual("Partially resolved", resolution.Message);
            Assert.AreEqual(1, resolution.Status);
        }
    }
}
