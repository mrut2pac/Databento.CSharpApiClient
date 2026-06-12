// Ignore Spelling: Databento Mbo Mbp Bbo Tbbo Tcbbo Cmbp Json Cbbo Ohlcv

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
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
            string json = "{" + MakeHeader(rtype: 18) + ",\"ts_recv\":\"2022-05-16T13:30:00.000000000Z\","
                + "\"action\":\"H\",\"reason\":\"T1\",\"trading_event\":\"0\","
                + "\"is_trading\":\"N\",\"is_quoting\":\"N\",\"is_short_sell_restricted\":\"U\"}";

            using DatabentoJsonClient client = BuildClient(json);
            StatusRecordJson[] records = await client.GetStatusAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual("H", records[0].Action);
            Assert.AreEqual("T1", records[0].Reason);
            Assert.AreEqual("N", records[0].IsTrading);
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

        // =====================================================================
        // SymbolMapping
        // =====================================================================

        [TestMethod]
        public async Task GetSymbolMappingsAsync_ValidJsonLine_ReturnsParsedRecord()
        {
            string json = "{" + MakeHeader(rtype: 90) + ",\"ts_recv\":\"2022-05-16T00:00:00.000000000Z\","
                + "\"stype_in_symbol\":\"SPY\",\"stype_out_symbol\":\"10005003\","
                + "\"start_date\":\"2022-01-01\",\"end_date\":\"2023-01-01\"}";

            using DatabentoJsonClient client = BuildClient(json);
            SymbolMappingRecordJson[] records = await client.GetSymbolMappingsAsync(AnyDataset, AnySymbol, AnyStart, AnyEnd);

            Assert.AreEqual(1, records.Length);
            Assert.AreEqual("SPY", records[0].StypeInSymbol);
            Assert.AreEqual("10005003", records[0].StypeOutSymbol);
            Assert.IsNotNull(records[0].StartDate);
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

        [TestMethod]
        public async Task ListConditionsAsync_ArrayResponse_ReturnsParsedConditions()
        {
            string json = "[{\"date\":\"2024-01-15\",\"condition\":\"good\",\"last_modified_date\":\"2024-01-16\"},"
                + "{\"date\":\"2024-01-16\",\"condition\":\"good\",\"last_modified_date\":\"2024-01-17\"}]";

            using DatabentoJsonClient client = BuildClient(json);
            DatasetCondition[] conditions = await client.ListConditionsAsync("OPRA.PILLAR");

            Assert.AreEqual(2, conditions.Length);
            Assert.AreEqual("OPRA.PILLAR", conditions[0].Dataset);
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
    }
}
