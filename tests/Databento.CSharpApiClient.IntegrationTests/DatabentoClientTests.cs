using System;

using Databento.CSharpApiClient.DataModel.Dbn;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    public class DatabentoClientTests : IntegrationTestBase
    {
        // =====================================================================
        // CBBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public void GetCbbo1s_SpxwOption_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = client.GetCbbo1s(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].BidPrice > 0 || records[0].AskPrice > 0);
        }

        [SkippableFact]
        public void GetCbbo1m_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = client.GetCbbo1m(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public void GetCbbo1s_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordDbn[] records = client.GetCbbo1s(
                Datasets.OpraPillar,
                new[] { "SPXW  250908C06475000", "SPXW  250908P06475000" },
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Trades (DBN binary)
        // =====================================================================

        [SkippableFact]
        public void GetTrades_Spy_ReturnsRecordsWithPriceAndSize()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordDbn[] records = client.GetTrades(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Price > 0);
            Assert.True(records[0].Size > 0);
        }

        [SkippableFact]
        public void GetTrades_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordDbn[] records = client.GetTrades(
                Datasets.XnasItch,
                new[] { "SPY", "QQQ" },
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // MBP-1 (DBN binary)
        // =====================================================================

        [SkippableFact]
        public void GetMbp1_Spy_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordDbn[] records = client.GetMbp1(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].BidPrice > 0 || records[0].AskPrice > 0);
        }
    }
}
