using System;

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
        public void ListDatasets_ReturnsNonEmptyArrayContainingOpraPillar()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            string[] datasets = client.ListDatasets();

            Assert.NotNull(datasets);
            Assert.NotEmpty(datasets);
            Assert.Contains(Datasets.OpraPillar, datasets);
        }

        [SkippableFact]
        public void ListSchemas_OpraPillar_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            string[] schemas = client.ListSchemas(Datasets.OpraPillar);

            Assert.NotNull(schemas);
            Assert.NotEmpty(schemas);
        }

        [SkippableFact]
        public void ListPublishers_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            PublisherInfo[] publishers = client.ListPublishers();

            Assert.NotNull(publishers);
            Assert.NotEmpty(publishers);
        }

        [SkippableFact]
        public void ListFields_Trades_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            FieldInfo[] fields = client.ListFields(Schema.Trades);

            Assert.NotNull(fields);
            Assert.NotEmpty(fields);
        }

        [SkippableFact]
        public void ListUnitPrices_XnasItch_ReturnsNonEmptyArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            UnitPriceInfo[] prices = client.ListUnitPrices(Datasets.XnasItch);

            Assert.NotNull(prices);
            Assert.NotEmpty(prices);
        }

        [SkippableFact]
        public void GetDatasetCondition_OpraPillar_ReturnsNonNullWithDataset()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DatasetCondition condition = client.GetDatasetCondition(Datasets.OpraPillar);

            Assert.NotNull(condition);
            Assert.False(string.IsNullOrEmpty(condition.Dataset));
        }

        [SkippableFact]
        public void GetDatasetRange_OpraPillar_ReturnsValidRange()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateRange range = client.GetDatasetRange(Datasets.OpraPillar);

            Assert.NotNull(range);
            Assert.True(range.Start < range.End);
        }

        // =====================================================================
        // Symbology
        // =====================================================================

        [SkippableFact]
        public void ResolveSymbols_SpxwOption_ReturnsResolution()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            SymbologyResolution resolution;
            try
            {
                resolution = client.ResolveSymbols(new SymbologyRequest
                {
                    Dataset   = Datasets.OpraPillar,
                    Symbols   = new[] { "SPXW  250908C06475000" },
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
        public void ListBatchJobs_ReturnsArray()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Lists historical jobs (may be empty on a fresh account — that is acceptable).
            BatchJob[] jobs = client.ListBatchJobs();

            Assert.NotNull(jobs);
        }

        // =====================================================================
        // Timeseries: CBBO
        // =====================================================================

        [SkippableFact]
        public void GetCbbo1s_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = client.GetCbbo1s(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
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
        public void GetCbbo1m_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = client.GetCbbo1m(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
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
        public void GetCbbo1m_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = client.GetCbbo1m(
                    Datasets.OpraPillar,
                    new[] { "SPXW  250908C06475000", "SPXW  250908P06475000" },
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
        public void GetCbbo1m_SpxwOption_SundayReturnsEmpty()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Sept 7 2025 is a Sunday — options market is closed, no data expected.
            DateTimeOffset start = new DateTimeOffset(2025, 9, 7, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 8, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records;
            try
            {
                records = client.GetCbbo1m(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);
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
        public void GetCbbo1s_MostRecentTradingDate_ThrowsDataStartAfterAvailableEnd()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // Historical data for tomorrow is never available — the API pushes back with a
            // descriptive 422 rather than a generic error. This documents that behavior.
            DateTimeOffset tomorrow = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
            DateTimeOffset dayAfter = tomorrow.AddDays(1);

            DatabentoHttpException ex = Assert.Throws<DatabentoHttpException>(() =>
                client.GetCbbo1s(Datasets.OpraPillar, "SPXW  250908C06475000", tomorrow, dayAfter));

            Assert.Equal(422, ex.StatusCode);
            Assert.Equal("data_start_after_available_end", ex.ErrorCase);
        }

        // =====================================================================
        // Timeseries: OHLCV
        // =====================================================================

        [SkippableFact]
        public void GetOhlcv1s_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = client.GetOhlcv1s(Datasets.XnasItch, "SPY", start, end);
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
        public void GetOhlcv1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = client.GetOhlcv1m(Datasets.XnasItch, "SPY", start, end);
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
        public void GetOhlcv1h_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = client.GetOhlcv1h(Datasets.XnasItch, "SPY", start, end);
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
        public void GetOhlcv1d_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 6, 1, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records;
            try
            {
                records = client.GetOhlcv1d(Datasets.XnasItch, "SPY", start, end);
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
        public void GetTrades_Spy_ReturnsRecordsWithPriceAndSize()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordJson[] records;
            try
            {
                records = client.GetTrades(Datasets.XnasItch, "SPY", start, end);
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
        public void GetTrades_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 14, 30, 0, TimeSpan.Zero);

            TradeRecordJson[] records;
            try
            {
                records = client.GetTrades(
                    Datasets.XnasItch,
                    new[] { "SPY", "QQQ" },
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
        public void GetMbp1_Spy_ReturnsRecordsWithBidOrAsk()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordJson[] records;
            try
            {
                records = client.GetMbp1(Datasets.XnasItch, "SPY", start, end);
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
        public void GetMbp1_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 16, 13, 35, 0, TimeSpan.Zero);

            Mbp1RecordJson[] records;
            try
            {
                records = client.GetMbp1(
                    Datasets.XnasItch,
                    new[] { "SPY", "QQQ" },
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
        public void GetStatistics_Es_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            // ESH4 = E-mini S&P 500, March 2024 expiry (raw symbol, GLBX.MDP3).
            DateTimeOffset start = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2024, 3, 16, 0, 0, 0, TimeSpan.Zero);

            StatisticsRecordJson[] records;
            try
            {
                records = client.GetStatistics(Datasets.GlbxMdp3, "ESH4", start, end);
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
        public void GetDefinitions_SpxwOption_ReturnsRecordsWithRawSymbol()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            DefinitionRecordJson[] records;
            try
            {
                records = client.GetDefinitions(
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
        public void GetDefinitions_MultiSymbol_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            DefinitionRecordJson[] records;
            try
            {
                records = client.GetDefinitions(
                    Datasets.OpraPillar,
                    new[] { "SPXW  250908C06475000", "SPXW  250908P06475000" },
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
    }
}
