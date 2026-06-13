using System;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Batch;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;
using Databento.CSharpApiClient.Exceptions;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    public class DatabentoJsonClientTests : IntegrationTestBase
    {
        // =====================================================================
        // Metadata
        // =====================================================================

        [SkippableFact]
        public async Task ListDatasets_ReturnsNonEmptyArrayContainingOpraPillar()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            string[] datasets = await client.ListDatasetsAsync();

            Assert.NotNull(datasets);
            Assert.NotEmpty(datasets);
            Assert.Contains(Datasets.OpraPillar, datasets);
        }

        [SkippableFact]
        public async Task ListSchemas_OpraPillar_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            string[] schemas = await client.ListSchemasAsync(Datasets.OpraPillar);

            Assert.NotNull(schemas);
            Assert.NotEmpty(schemas);
        }

        [SkippableFact]
        public async Task ListPublishers_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            PublisherInfo[] publishers = await client.ListPublishersAsync();

            Assert.NotNull(publishers);
            Assert.NotEmpty(publishers);
        }

        [SkippableFact]
        public async Task ListFields_Trades_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            FieldInfo[] fields = await client.ListFieldsAsync(Schema.Trades);

            Assert.NotNull(fields);
            Assert.NotEmpty(fields);
        }

        [SkippableFact]
        public async Task ListUnitPrices_XnasItch_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            UnitPriceInfo[] prices = await client.ListUnitPricesAsync(Datasets.XnasItch);

            Assert.NotNull(prices);
            Assert.NotEmpty(prices);
        }

        [SkippableFact]
        public async Task GetDatasetCondition_OpraPillar_ReturnsNonNullWithDataset()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DatasetCondition condition = await client.GetDatasetConditionAsync(Datasets.OpraPillar);

            Assert.NotNull(condition);
            Assert.False(string.IsNullOrEmpty(condition.Dataset));
        }

        [SkippableFact]
        public async Task GetDatasetRange_OpraPillar_ReturnsValidRange()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateRange range = await client.GetDatasetRangeAsync(Datasets.OpraPillar);

            Assert.NotNull(range);
            Assert.True(range.Start < range.End);
        }

        // =====================================================================
        // Symbology
        // =====================================================================

        [SkippableFact]
        public async Task ResolveSymbols_SpxwOption_ReturnsResolution()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            SymbologyResolution resolution;
            try
            {
                resolution = await client.ResolveSymbolsAsync(new SymbologyRequest
                {
                    Dataset   = Datasets.OpraPillar,
                    Symbols   = ["SPXW  250908C06475000"],
                    StypeIn   = SymbolTypes.RawSymbol,
                    StypeOut  = SymbolTypes.InstrumentId,
                    StartDate = start.ToString("yyyy-MM-dd"),
                    EndDate   = end.ToString("yyyy-MM-dd"),
                });
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(resolution);
            Assert.NotNull(resolution.Result);
        }

        // =====================================================================
        // Batch
        // =====================================================================

        [SkippableFact]
        public async Task ListBatchJobs_ReturnsArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Lists historical jobs (may be empty on a fresh account — that is acceptable).
            BatchJob[] jobs = await client.ListBatchJobsAsync();

            Assert.NotNull(jobs);
        }

        // =====================================================================
        // Timeseries: CBBO
        // =====================================================================

        [SkippableFact]
        public async Task GetCbbo1s_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = await client.GetCbbo1sAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1m_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = await client.GetCbbo1mAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1m_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = await client.GetCbbo1mAsync(
                    Datasets.OpraPillar,
                    ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                    start,
                    end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1m_SpxwOption_SundayReturnsEmpty()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Sept 7 2025 is a Sunday — options market is closed, no data expected.
            DateTimeOffset start = new DateTimeOffset(2025, 9, 7, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 8, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = await client.GetCbbo1mAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.Empty(records);
        }

        [SkippableFact]
        public async Task GetCbbo1s_MostRecentTradingDate_ThrowsDataStartAfterAvailableEnd()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Historical data for tomorrow is never available — the API pushes back with a
            // descriptive 422 rather than a generic error. This documents that behavior.
            DateTimeOffset tomorrow = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
            DateTimeOffset dayAfter = tomorrow.AddDays(1);

            DatabentoHttpException ex = await Assert.ThrowsAsync<DatabentoHttpException>(() =>
                client.GetCbbo1sAsync(Datasets.OpraPillar, "SPXW  250908C06475000", tomorrow, dayAfter));

            Assert.Equal(422, ex.StatusCode);
            Assert.Equal("data_start_after_available_end", ex.ErrorCase);
        }

        // =====================================================================
        // Timeseries: OHLCV
        // =====================================================================

        [SkippableFact]
        public async Task GetOhlcv1s_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = await client.GetOhlcv1sAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            if(records.Length == 0)
            {
                string raw = this.FetchRawTimeseries(Datasets.XnasItch, Schema.Ohlcv1Sec, "SPY",
                    "2022-05-16T00:00:00Z", "2022-05-17T00:00:00Z");
                throw new Xunit.Sdk.XunitException(
                    "Records empty after deserialization. Raw API response (first 2000 chars):\n"
                    + raw.Substring(0, Math.Min(raw.Length, 2000)));
            }

            Assert.NotEmpty(records);
            Assert.True(records[0].Open > 0);
            Assert.True(records[0].Volume > 0);
        }

        [SkippableFact]
        public async Task GetOhlcv1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = await client.GetOhlcv1mAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Open > 0);
        }

        [SkippableFact]
        public async Task GetOhlcv1h_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = await client.GetOhlcv1hAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Open > 0);
        }

        [SkippableFact]
        public async Task GetOhlcv1d_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 6, 1, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = await client.GetOhlcv1dAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: Trades
        // =====================================================================

        [SkippableFact]
        public async Task GetTrades_Spy_ReturnsRecordsWithPriceAndSize()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordJson[] records;
            try
            {
                records = await client.GetTradesAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Price > 0);
            Assert.True(records[0].Size > 0);
        }

        [SkippableFact]
        public async Task GetTrades_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordJson[] records;
            try
            {
                records = await client.GetTradesAsync(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: MBP-1
        // =====================================================================

        [SkippableFact]
        public async Task GetMbp1_Spy_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordJson[] records;
            try
            {
                records = await client.GetMbp1Async(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.NotNull(records[0].Level1);
            Assert.True(records[0].Level1.BidPrice > 0 || records[0].Level1.AskPrice > 0);
        }

        [SkippableFact]
        public async Task GetMbp1_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordJson[] records;
            try
            {
                records = await client.GetMbp1Async(
                    Datasets.XnasItch,
                    ["SPY", "QQQ"],
                    start,
                    end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: Statistics
        // =====================================================================

        [SkippableFact]
        public async Task GetStatistics_Es_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // ESH4 = E-mini S&P 500, March 2024 expiry (raw symbol, GLBX.MDP3).
            DateTimeOffset start = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2024, 3, 16, 0, 0, 0, TimeSpan.Zero);

            StatisticsRecordJson[] records;
            try
            {
                records = await client.GetStatisticsAsync(Datasets.GlbxMdp3, "ESH4", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: Definitions
        // =====================================================================

        [SkippableFact]
        public async Task GetDefinitions_SpxwOption_ReturnsRecordsWithRawSymbol()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            DefinitionRecordJson[] records;
            try
            {
                records = await client.GetDefinitionsAsync(
                    Datasets.OpraPillar,
                    "SPXW  250908C06475000",
                    start,
                    end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.False(string.IsNullOrEmpty(records[0].RawSymbol));
        }

        [SkippableFact]
        public async Task GetDefinitions_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            DefinitionRecordJson[] records;
            try
            {
                records = await client.GetDefinitionsAsync(
                    Datasets.OpraPillar,
                    ["SPXW  250908C06475000", "SPXW  250908P06475000"],
                    start,
                    end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: MBO
        // =====================================================================

        [SkippableFact]
        public async Task GetMbo_Es_ReturnsRecordsWithOrderId()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // ESH4 on GLBX.MDP3 — CME has full MBO (L3) data.
            DateTimeOffset start = new DateTimeOffset(2024, 3, 15, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2024, 3, 15, 13, 31, 0, TimeSpan.Zero);

            MboRecordJson[] records;
            try
            {
                records = await client.GetMboAsync(Datasets.GlbxMdp3, "ESH4", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].OrderId > 0);
        }

        // =====================================================================
        // Timeseries: MBP-10
        // =====================================================================

        [SkippableFact]
        public async Task GetMbp10_Spy_ReturnsRecordsWithLevels()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 31, 0, TimeSpan.Zero);

            Mbp10RecordJson[] records;
            try
            {
                records = await client.GetMbp10Async(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.NotNull(records[0].Level1);
        }

        // =====================================================================
        // Timeseries: BBO-1s / BBO-1m
        // =====================================================================

        [SkippableFact]
        public async Task GetBbo1s_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            BboRecordJson[] records;
            try
            {
                records = await client.GetBbo1sAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Skip.If(records.Length == 0, "No BBO-1s records returned — schema may not be available on this subscription or date range.");
            Assert.NotNull(records[0].Level1);
            Assert.True(records[0].Level1.BidPrice > 0 || records[0].Level1.AskPrice > 0);
        }

        [SkippableFact]
        public async Task GetBbo1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            BboRecordJson[] records;
            try
            {
                records = await client.GetBbo1mAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Skip.If(records.Length == 0, "No BBO-1m records returned — schema may not be available on this subscription or date range.");
        }

        // =====================================================================
        // Timeseries: TBBO
        // =====================================================================

        [SkippableFact]
        public async Task GetTbbo_Spy_ReturnsRecordsWithBboLevel()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TbboRecordJson[] records;
            try
            {
                records = await client.GetTbboAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Price > 0);
            Assert.NotNull(records[0].Level1);
        }

        // =====================================================================
        // Timeseries: TCBBO
        // =====================================================================

        [SkippableFact]
        public async Task GetTcbbo_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            TcbboRecordJson[] records;
            try
            {
                records = await client.GetTcbboAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.NotNull(records[0].Level1);
        }

        // =====================================================================
        // Timeseries: CMBP-1
        // =====================================================================

        [SkippableFact]
        public async Task GetCmbp1_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            Cmbp1RecordJson[] records;
            try
            {
                records = await client.GetCmbp1Async(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.NotNull(records[0].Level1);
        }

        // =====================================================================
        // Timeseries: Status
        // =====================================================================

        [SkippableFact]
        public async Task GetStatus_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            StatusRecordJson[] records;
            try
            {
                records = await client.GetStatusAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Skip.If(records.Length == 0, "No Status records returned — no trading-halt events for this symbol/date on current subscription.");
            Assert.False(string.IsNullOrEmpty(records[0].IsTrading));
        }

        // =====================================================================
        // Timeseries: Imbalance
        // =====================================================================

        [SkippableFact]
        public async Task GetImbalance_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // NASDAQ publishes closing-auction imbalance messages for SPY around 16:00 ET.
            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            ImbalanceRecordJson[] records;
            try
            {
                records = await client.GetImbalanceAsync(Datasets.XnasItch, "SPY", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        // =====================================================================
        // Timeseries: SymbolMapping
        // =====================================================================

        [SkippableFact]
        public async Task GetSymbolMappings_SpxwOption_ReturnsRecordsWithSymbols()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            SymbolMappingRecordJson[] records;
            try
            {
                records = await client.GetSymbolMappingsAsync(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.False(string.IsNullOrEmpty(records[0].StypeInSymbol));
        }

        // =====================================================================
        // Metadata: GetRecordCount / GetBillableSize / GetCost
        // =====================================================================

        [SkippableFact]
        public async Task GetRecordCount_SpyTrades_ReturnsPositiveCount()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            long count;
            try
            {
                count = await client.GetRecordCountAsync(Datasets.XnasItch, ["SPY"], Schema.Trades, start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.True(count > 0);
        }

        [SkippableFact]
        public async Task GetBillableSize_SpyTrades_ReturnsPositiveBytes()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            long bytes;
            try
            {
                bytes = await client.GetBillableSizeAsync(Datasets.XnasItch, ["SPY"], Schema.Trades, start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.True(bytes > 0);
        }

        [SkippableFact]
        public async Task GetCost_SpyTrades_ReturnsNonNegativeCost()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            double cost;
            try
            {
                cost = await client.GetCostAsync(Datasets.XnasItch, ["SPY"], Schema.Trades, start, end);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.True(cost >= 0.0);
        }

        // =====================================================================
        // Metadata: ListConditions
        // =====================================================================

        [SkippableFact]
        public async Task ListConditions_OpraPillar_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DatasetCondition[] conditions;
            try
            {
                conditions = await client.ListConditionsAsync(Datasets.OpraPillar);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotNull(conditions);
            Assert.NotEmpty(conditions);
            Assert.All(conditions, c => Assert.False(string.IsNullOrEmpty(c.Condition)));
        }
    }
}
