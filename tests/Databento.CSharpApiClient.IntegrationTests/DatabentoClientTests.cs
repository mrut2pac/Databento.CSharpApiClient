using System;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    public class DatabentoClientTests : IntegrationTestBase
    {
        // =====================================================================
        // CBBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetCbbo1s_SpxwOption_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = await client.GetCbbo1sAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].BidPrice > 0 || records[0].AskPrice > 0);
        }

        [SkippableFact]
        public async Task GetCbbo1m_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = await client.GetCbbo1mAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1s_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = await client.GetCbbo1sAsync(
                Datasets.OpraPillar,
                ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1m_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = await client.GetCbbo1mAsync(
                Datasets.OpraPillar,
                ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1s_MostRecentTradingDate_ThrowsDataStartAfterAvailableEnd()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset tomorrow = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
            DateTimeOffset dayAfter = tomorrow.AddDays(1);

            DatabentoHttpException ex = await Assert.ThrowsAsync<DatabentoHttpException>(() =>
                client.GetCbbo1sAsync(Datasets.OpraPillar, "SPXW  250908C06475000", tomorrow, dayAfter));

            Assert.Equal(422, ex.StatusCode);
            Assert.Equal("data_start_after_available_end", ex.ErrorCase);
        }

        // =====================================================================
        // Trades (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetTrades_Spy_ReturnsRecordsWithPriceAndSize()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordDbn[] records = await client.GetTradesAsync(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Price > 0);
            Assert.True(records[0].Size > 0);
        }

        [SkippableFact]
        public async Task GetTrades_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordDbn[] records = await client.GetTradesAsync(
                Datasets.XnasItch,
                ["SPY", "QQQ"],
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // MBP-1 (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetMbp1_Spy_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordDbn[] records = await client.GetMbp1Async(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].BidPrice > 0 || records[0].AskPrice > 0);
        }

        [SkippableFact]
        public async Task GetMbp1_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordDbn[] records = await client.GetMbp1Async(
                Datasets.XnasItch,
                ["SPY", "QQQ"],
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }
    }
}
