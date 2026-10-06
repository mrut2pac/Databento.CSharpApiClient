using System;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;
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
                Assert.True(records[0].TsEventUtc > default(DateTime), "expected TsEventUtc to be decoded");
                Assert.NotNull(records[0].Level);
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
        public async Task GetStatistics_EsH4_MatchesTheJsonEncoding()
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();

            // GLBX.MDP3 2024 data is served as DBN v3 (80-byte Statistics records); older XNAS.ITCH data still comes back as v1.
            DateTimeOffset start = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2024, 3, 16, 0, 0, 0, TimeSpan.Zero);

            try
            {
                StatisticsRecordDbn[] records = await client.GetStatisticsAsync(Datasets.GlbxMdp3, "ESH4", start, end);
                Assert.NotNull(records);
                Assert.NotEmpty(records);

                // A misread binary layout shifts the fields after quantity while staying plausible, so pin every decoded field to the
                // JSON encoding of the same request, which shares no layout code with the binary decoder.
                using DatabentoJsonClient jsonClient = this.CreateJsonClient();
                StatisticsRecordJson[] jsonRecords = await jsonClient.GetStatisticsAsync(Datasets.GlbxMdp3, "ESH4", start, end);

                Assert.Equal(jsonRecords.Length, records.Length);
                for(int i = 0; i < records.Length; ++i)
                {
                    Assert.Equal(jsonRecords[i].Quantity, records[i].Quantity);
                    Assert.Equal(jsonRecords[i].Sequence, records[i].Sequence);
                    Assert.Equal(jsonRecords[i].StatType, records[i].StatType);
                    Assert.Equal(jsonRecords[i].ChannelId, records[i].ChannelId);
                    Assert.Equal(jsonRecords[i].UpdateAction, records[i].UpdateAction);
                    Assert.Equal(jsonRecords[i].TsRefUtc, records[i].TsRefUtc);
                }
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

        [SkippableFact]
        public async Task GetDefinitions_OpraOptionServedAsDbnV1_MatchesTheJsonEncoding()
        {
            await this.AssertDefinitionsMatchTheJsonEncoding(
                Datasets.OpraPillar,
                "SPXW  220207C04295000",
                new DateTimeOffset(2022, 2, 7, 0, 0, 0, TimeSpan.Zero));
        }

        [SkippableFact]
        public async Task GetDefinitions_XnasEquityServedAsDbnV1_MatchesTheJsonEncoding()
        {
            await this.AssertDefinitionsMatchTheJsonEncoding(
                Datasets.XnasItch,
                "SPY",
                new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero));
        }

        [SkippableFact]
        public async Task GetDefinitions_GlbxOptionServedAsDbnV3_MatchesTheJsonEncoding()
        {
            await this.AssertDefinitionsMatchTheJsonEncoding(
                Datasets.GlbxMdp3,
                "ESH4 P3350",
                new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero));
        }

        // A misread binary layout yields plausible-looking garbage, so every decoded field is pinned to the JSON encoding of the same
        // request, which reads fields by name and shares no layout code with the binary decoder.
        private async Task AssertDefinitionsMatchTheJsonEncoding(string dataset, string symbol, DateTimeOffset day)
        {
            this.SkipIfNoApiKey();
            using DatabentoClient client = this.CreateBinaryClient();
            using DatabentoJsonClient jsonClient = this.CreateJsonClient();

            try
            {
                DefinitionRecordDbn[] records = await client.GetDefinitionsAsync(dataset, symbol, day, day.AddDays(1));
                DefinitionRecordJson[] jsonRecords = await jsonClient.GetDefinitionsAsync(dataset, symbol, day, day.AddDays(1));

                Assert.NotEmpty(records);
                Assert.Equal(jsonRecords.Length, records.Length);
                for(int i = 0; i < records.Length; ++i)
                {
                    Assert.Equal(symbol, records[i].RawSymbol);
                    Assert.Equal(jsonRecords[i].RawSymbol, records[i].RawSymbol);
                    Assert.Equal(jsonRecords[i].TsReceivedUtc, records[i].TsReceivedUtc);
                    Assert.Equal(jsonRecords[i].MinPriceIncrement, records[i].MinPriceIncrement, 9);
                    Assert.Equal(jsonRecords[i].DisplayFactor, records[i].DisplayFactor, 9);
                    Assert.Equal(jsonRecords[i].HighLimitPrice, records[i].HighLimitPrice, 9);
                    Assert.Equal(jsonRecords[i].LowLimitPrice, records[i].LowLimitPrice, 9);
                    Assert.Equal(jsonRecords[i].MaxPriceVariation, records[i].MaxPriceVariation, 9);
                    Assert.Equal(jsonRecords[i].UnitOfMeasureQty, records[i].UnitOfMeasureQty, 9);
                    Assert.Equal(jsonRecords[i].StrikePrice, records[i].StrikePrice, 9);
                    Assert.Equal(jsonRecords[i].InstrumentClass, records[i].InstrumentClass?.ToString());
                    Assert.Equal(jsonRecords[i].Action, records[i].Action?.ToString());
                    Assert.Equal(jsonRecords[i].Exchange, records[i].Exchange);
                    Assert.Equal(jsonRecords[i].Asset, records[i].Asset);
                    Assert.Equal(jsonRecords[i].Cfi, records[i].Cfi);
                    Assert.Equal(jsonRecords[i].SecurityType, records[i].SecurityType);
                    Assert.Equal(jsonRecords[i].Expiration, records[i].Expiration);
                    Assert.Equal(jsonRecords[i].Activation, records[i].Activation);
                }
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
                Assert.True(records[0].TsReceivedUtc > default(DateTime), "expected ts_recv to be decoded");
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
                Assert.True(records[0].TsReceivedUtc > default(DateTime), "expected TsReceivedUtc to be decoded");
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
