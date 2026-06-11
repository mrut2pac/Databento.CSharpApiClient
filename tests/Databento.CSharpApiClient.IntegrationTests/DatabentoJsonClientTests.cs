using System;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;

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
        public void GetDatasetCondition_OpraPillar_ReturnsNonNullWithDataset()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DatasetCondition condition = client.GetDatasetCondition(Datasets.OpraPillar);

            Assert.NotNull(condition);
            Assert.False(string.IsNullOrEmpty(condition.Dataset));
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

            SymbologyResolution resolution = client.ResolveSymbols(new SymbologyRequest
            {
                Dataset   = Datasets.OpraPillar,
                Symbols   = new[] { "SPXW  250908C06475000" },
                StypeIn   = SymbolTypes.RawSymbol,
                StypeOut  = SymbolTypes.InstrumentId,
                StartDate = start.ToString("yyyy-MM-dd"),
                EndDate   = end.ToString("yyyy-MM-dd"),
            });

            Assert.NotNull(resolution);
            Assert.NotNull(resolution.Result);
        }

        // =====================================================================
        // Timeseries: CBBO
        // =====================================================================

        [SkippableFact]
        public void GetCbbo1m_SpxwOption_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2025, 9, 6, 0, 0, 0, TimeSpan.Zero);

            CbboRecordJson[] records = client.GetCbbo1m(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

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

            CbboRecordJson[] records = client.GetCbbo1m(Datasets.OpraPillar, "SPXW  250908C06475000", start, end);

            Assert.NotNull(records);
            Assert.Empty(records);
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

            OhlcvRecordJson[] records = client.GetOhlcv1s(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public void GetOhlcv1m_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 5, 17, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records = client.GetOhlcv1m(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
        }

        [SkippableFact]
        public void GetOhlcv1d_Spy_ReturnsRecords()
        {
            this.SkipIfNoApiKey();
            using DatabentoJsonClient client = this.CreateJsonClient();

            DateTimeOffset start = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset end   = new DateTimeOffset(2022, 6, 1, 0, 0, 0, TimeSpan.Zero);

            OhlcvRecordJson[] records = client.GetOhlcv1d(Datasets.XnasItch, "SPY", start, end);

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

            TradeRecordJson[] records = client.GetTrades(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.True(records[0].Price > 0);
            Assert.True(records[0].Size > 0);
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

            Mbp1RecordJson[] records = client.GetMbp1(Datasets.XnasItch, "SPY", start, end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.NotNull(records[0].Level1);
            Assert.True(records[0].Level1.BidPrice > 0 || records[0].Level1.AskPrice > 0);
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

            DefinitionRecordJson[] records = client.GetDefinitions(
                Datasets.OpraPillar,
                "SPXW  250908C06475000",
                start,
                end);

            Assert.NotNull(records);
            Assert.NotEmpty(records);
            Assert.False(string.IsNullOrEmpty(records[0].RawSymbol));
        }
    }
}
