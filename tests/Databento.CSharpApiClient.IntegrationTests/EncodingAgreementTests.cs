using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.Exceptions;

using Xunit;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// For every schema, decodes the same request from DBN and from JSON and checks the two agree on every field
    /// (see <see cref="EncodingComparer"/>), so a misread binary layout or a field the DBN decoder skips fails. Where the DBN record
    /// maps to the JSON model (<see cref="DbnToJsonExtensions"/>), the mapped records must serialize exactly as the JSON encoding's.
    /// </summary>
    public class EncodingAgreementTests : IntegrationTestBase
    {
        private const string OpraPut = "SPY   240501P00501000";

        private const string DefinitionLegs = "LegCount,LegIndex,LegInstrumentId,LegRawSymbol,LegInstrumentClass,LegSide,LegPrice,LegDelta,"
            + "LegRatioPriceNumerator,LegRatioPriceDenominator,LegRatioQtyNumerator,LegRatioQtyDenominator,LegUnderlyingId";

        private const string DefinitionV1Only = "TradingReferencePrice,TradingReferenceDate,MdSecurityTradingStatus,SettlementPriceType";

        [SkippableTheory]
        [InlineData("GetMboAsync", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T13:30:02Z", "")]
        [InlineData("GetMbp1Async", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T13:30:02Z", "")]
        [InlineData("GetMbp10Async", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T13:30:01Z", "")]
        [InlineData("GetTbboAsync", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T13:31:00Z", "")]
        [InlineData("GetTradesAsync", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T13:31:00Z", "")]
        [InlineData("GetBbo1sAsync", Datasets.XnasItch, "SPY", "2024-05-01T14:00:00Z", "2024-05-01T14:01:00Z", "")]
        [InlineData("GetOhlcv1mAsync", Datasets.XnasItch, "SPY", "2022-05-16T13:30:00Z", "2022-05-16T14:30:00Z", "")]
        [InlineData("GetOhlcv1dAsync", Datasets.XnasItch, "SPY", "2022-05-02T00:00:00Z", "2022-05-17T00:00:00Z", "")]
        [InlineData("GetStatusAsync", Datasets.XnasItch, "SPY", "2022-05-16T00:00:00Z", "2022-05-17T00:00:00Z", "")]
        [InlineData("GetImbalanceAsync", Datasets.XnasItch, "SPY", "2024-05-01T19:55:00Z", "2024-05-01T20:00:00Z", "")]
        [InlineData("GetImbalanceAsync", "ARCX.PILLAR", "SPY", "2024-05-01T19:55:00Z", "2024-05-01T20:00:00Z", "")]
        [InlineData("GetStatisticsAsync", Datasets.GlbxMdp3, "ESH4", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", "")]
        [InlineData("GetStatisticsAsync", Datasets.OpraPillar, OpraPut, "2024-05-01T00:00:00Z", "2024-05-02T00:00:00Z", "")]
        [InlineData("GetTcbboAsync", Datasets.OpraPillar, OpraPut, "2024-05-01T14:00:00Z", "2024-05-01T14:05:00Z", "")]
        [InlineData("GetCmbp1Async", Datasets.OpraPillar, OpraPut, "2024-05-01T14:00:00Z", "2024-05-01T14:01:00Z", "")]
        [InlineData("GetCbbo1sAsync", Datasets.OpraPillar, "SPXW  250908C06475000", "2025-09-05T14:00:00Z", "2025-09-05T14:10:00Z", "")]
        [InlineData("GetDefinitionsAsync", Datasets.OpraPillar, "SPXW  220207C04295000", "2022-02-07T00:00:00Z", "2022-02-08T00:00:00Z", DefinitionLegs)]
        [InlineData("GetDefinitionsAsync", Datasets.XnasItch, "SPY", "2022-05-16T00:00:00Z", "2022-05-17T00:00:00Z", DefinitionLegs)]
        [InlineData("GetDefinitionsAsync", Datasets.GlbxMdp3, "ESH4 P3350", "2024-03-15T00:00:00Z", "2024-03-16T00:00:00Z", DefinitionV1Only)]
        public async Task DbnAndJsonEncodings_SameRequest_AgreeOnEveryField(string method, string dataset, string symbol, string start, string end, string ignored)
        {
            this.SkipIfNoApiKey();
            using DatabentoClient binaryClient = this.CreateBinaryClient();
            using DatabentoJsonClient jsonClient = this.CreateJsonClient();

            DateTimeOffset from = DateTimeOffset.Parse(start, CultureInfo.InvariantCulture);
            DateTimeOffset to = DateTimeOffset.Parse(end, CultureInfo.InvariantCulture);

            IList dbnRecords;
            IList jsonRecords;
            try
            {
                dbnRecords = await Fetch(binaryClient, method, dataset, symbol, from, to);
                jsonRecords = await Fetch(jsonClient, method, dataset, symbol, from, to);
            }
            catch(DatabentoHttpException ex)
            {
                SkipIfNoLicense(ex);
                throw;
            }

            Assert.NotEmpty(dbnRecords);
            IReadOnlyList<string> problems = EncodingComparer.Compare(dbnRecords, jsonRecords, ignored.Split(',', StringSplitOptions.RemoveEmptyEntries));
            Assert.True(problems.Count == 0, method + " " + dataset + ":" + Environment.NewLine + string.Join(Environment.NewLine, problems));

            // a DBN record that maps to the JSON model must map to the very record the JSON encoding gives, serialized alike; the
            // records with a record interface are the ones that map, so a renamed method can't switch this check off unnoticed
            MethodInfo toJsonRecord = typeof(DbnToJsonExtensions).GetMethod(nameof(DbnToJsonExtensions.ToJsonRecord), new[] { dbnRecords[0].GetType() });
            Assert.True(
                (dbnRecords[0] is IRecord) == (toJsonRecord != null),
                dbnRecords[0].GetType().Name + (toJsonRecord == null ? " has a record interface but no ToJsonRecord()" : " has ToJsonRecord() but no record interface"));
            if(toJsonRecord != null)
            {
                for(int i = 0; i < dbnRecords.Count; ++i)
                {
                    string mapped = JsonSerializer.Serialize(toJsonRecord.Invoke(null, new[] { dbnRecords[i] }), toJsonRecord.ReturnType);
                    string fromJson = JsonSerializer.Serialize(jsonRecords[i], toJsonRecord.ReturnType);
                    Assert.True(mapped == fromJson, $"{method} {dataset} [{i}]:{Environment.NewLine}mapped {mapped}{Environment.NewLine}JSON   {fromJson}");
                }
            }
        }

        // Both clients name the per-schema methods alike: Get*Async(dataset, symbol, startUtc, endUtc, ct)
        private static async Task<IList> Fetch(object client, string method, string dataset, string symbol, DateTimeOffset from, DateTimeOffset to)
        {
            MethodInfo info = client.GetType().GetMethod(
                method,
                new[] { typeof(string), typeof(string), typeof(DateTimeOffset), typeof(DateTimeOffset), typeof(CancellationToken) });
            Assert.True(info != null, client.GetType().Name + " has no " + method + "(dataset, symbol, start, end, ct)");
            Task task = (Task)info.Invoke(client, new object[] { dataset, symbol, from, to, CancellationToken.None });
            await task.ConfigureAwait(false);
            return (IList)task.GetType().GetProperty("Result").GetValue(task);
        }
    }
}
