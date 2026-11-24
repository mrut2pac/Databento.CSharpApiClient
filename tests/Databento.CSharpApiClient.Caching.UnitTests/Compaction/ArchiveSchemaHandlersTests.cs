using Databento.CSharpApiClient.Caching.Compaction;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Compaction
{
    [TestClass]
    public class ArchiveSchemaHandlersTests
    {
        [TestMethod]
        [DataRow("cbbo-1s")]
        [DataRow("cbbo-1m")]
        [DataRow("bbo-1s")]
        [DataRow("bbo-1m")]
        [DataRow("tcbbo")]
        [DataRow("trades")]
        [DataRow("ohlcv-1s")]
        [DataRow("ohlcv-1m")]
        [DataRow("ohlcv-1h")]
        [DataRow("ohlcv-1d")]
        [DataRow("ohlcv-eod")]
        [DataRow("statistics")]
        public void TryGet_ArchivedSchema_IsHandledUnderItsDatabentoName(string schema)
        {
            Assert.IsTrue(ArchiveSchemaHandlers.TryGet(schema, out IArchiveSchemaHandler handler));

            Assert.AreEqual(schema, handler.Schema);
        }

        [TestMethod]
        [DataRow("mbo")]
        [DataRow("mbp-1")]
        [DataRow("definition")]
        [DataRow("CBBO-1M")]
        [DataRow("CBBO1-Min")]
        [DataRow("")]
        public void TryGet_SchemaTheArchiveHasNoColumnsFor_ReturnsFalse(string schema)
        {
            Assert.IsFalse(ArchiveSchemaHandlers.TryGet(schema, out IArchiveSchemaHandler handler));
            Assert.IsNull(handler);
        }
    }
}
