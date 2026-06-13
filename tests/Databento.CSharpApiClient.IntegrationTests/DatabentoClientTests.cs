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

        // =====================================================================
        // MBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetMbo_Spy_ReturnsRecordsWithPriceAndSize()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                MboRecordDbn[] records = await client.GetMboAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Price > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetMbo_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                MboRecordDbn[] records = await client.GetMboAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // MBP-10 (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetMbp10_Spy_ReturnsRecordsWithLevels()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 31, 0, TimeSpan.Zero);

            try
            {
                Mbp10RecordDbn[] records = await client.GetMbp10Async(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.NotNull(records[0].Levels);
                Assert.Equal(10, records[0].Levels.Length);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetMbp10_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 31, 0, TimeSpan.Zero);

            try
            {
                Mbp10RecordDbn[] records = await client.GetMbp10Async(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // BBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetBbo1s_Spy_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                BboRecordDbn[] records = await client.GetBbo1sAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Level.BidPrice > 0 || records[0].Level.AskPrice > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetBbo1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            try
            {
                BboRecordDbn[] records = await client.GetBbo1mAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetBbo1s_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                BboRecordDbn[] records = await client.GetBbo1sAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // TBBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetTbbo_Spy_ReturnsRecordsWithPriceAndLevel()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                TbboRecordDbn[] records = await client.GetTbboAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Price > 0);
                Assert.NotNull(records[0].Level);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetTbbo_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            try
            {
                TbboRecordDbn[] records = await client.GetTbboAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // TCBBO (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetTcbbo_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            try
            {
                TcbboRecordDbn[] records = await client.GetTcbboAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Level.BidPrice > 0 || records[0].Level.AskPrice > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetTcbbo_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            try
            {
                TcbboRecordDbn[] records = await client.GetTcbboAsync(
                    Datasets.OpraPillar,
                    ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // CMBP-1 (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetCmbp1_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            try
            {
                Cmbp1RecordDbn[] records = await client.GetCmbp1Async(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetCmbp1_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            try
            {
                Cmbp1RecordDbn[] records = await client.GetCmbp1Async(
                    Datasets.OpraPillar,
                    ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // OHLCV (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetOhlcv1s_Spy_ReturnsRecordsWithOhlc()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcv1sAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Open > 0);
                Assert.True(records[0].High >= records[0].Low);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetOhlcv1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcv1mAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Open > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetOhlcv1h_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcv1hAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Open > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetOhlcv1d_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 31, 0, 0, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcv1dAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Open > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetOhlcvEod_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 31, 0, 0, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcvEodAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.True(records[0].Open > 0);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetOhlcv1s_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            try
            {
                OhlcvRecordDbn[] records = await client.GetOhlcv1sAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // Statistics (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetStatistics_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                StatisticsRecordDbn[] records = await client.GetStatisticsAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetStatistics_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                StatisticsRecordDbn[] records = await client.GetStatisticsAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // Definition (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetDefinitions_Spy_ReturnsRecordsWithSymbol()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                DefinitionRecordDbn[] records = await client.GetDefinitionsAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
                Assert.False(string.IsNullOrEmpty(records[0].RawSymbol));
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetDefinitions_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                DefinitionRecordDbn[] records = await client.GetDefinitionsAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // Status (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetStatus_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                StatusRecordDbn[] records = await client.GetStatusAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetStatus_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                StatusRecordDbn[] records = await client.GetStatusAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        // =====================================================================
        // Imbalance (DBN binary)
        // =====================================================================

        [SkippableFact]
        public async Task GetImbalance_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                ImbalanceRecordDbn[] records = await client.GetImbalanceAsync(Datasets.XnasItch, "SPY", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }

        [SkippableFact]
        public async Task GetImbalance_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            try
            {
                ImbalanceRecordDbn[] records = await client.GetImbalanceAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);
            }
            catch(DatabentoHttpException ex) { SkipIfNoLicense(ex); throw; }
        }
    }
}
