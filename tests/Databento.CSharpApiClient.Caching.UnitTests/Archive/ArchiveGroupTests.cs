using System;

using Databento.CSharpApiClient.Caching.Archive;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive
{
    [TestClass]
    public class ArchiveGroupTests
    {
        [TestMethod]
        [DataRow("SPXW  261016C06600000", "SPXW.OPT")]
        [DataRow("NDXP  261001C30400000", "NDXP.OPT")]
        [DataRow("SPY   261016P00600000", "SPY.OPT")]
        [DataRow("BRK.B 261016C00500000", "BRK.B.OPT")]
        [DataRow("SPXW1 261016C06600000", "SPXW1.OPT")]
        public void Of_OpraOsiSymbol_IsItsRootAsAnOptionGroup(string rawSymbol, string expectedGroup)
        {
            Assert.AreEqual(expectedGroup, ArchiveGroup.Of("OPRA.PILLAR", rawSymbol));
        }

        [TestMethod]
        public void Of_OsiSymbolOfAnyOpraDataset_IsItsRootAsAnOptionGroup()
        {
            Assert.AreEqual("SPXW.OPT", ArchiveGroup.Of("OPRA.TEST", "SPXW  261016C06600000"));
        }

        [TestMethod]
        [DataRow("SPXW", DisplayName = "the root")]
        [DataRow("$SPXW_261016_C6600", DisplayName = "another symbology")]
        [DataRow("      261016C06600000", DisplayName = "no root")]
        [DataRow("SP XW 261016C06600000", DisplayName = "a space inside the root")]
        [DataRow("SPXW  ABCDEFGHIJKLMNO", DisplayName = "no expiration")]
        [DataRow("SPXW  261016X06600000", DisplayName = "neither a call nor a put")]
        [DataRow("SPXW  2610160C6600000", DisplayName = "the right out of place")]
        [DataRow("SPXW  261016C0660000A", DisplayName = "a strike that is not a number")]
        [DataRow("SPXW  261016C066000000", DisplayName = "one character too many")]
        public void Of_OpraSymbolThatIsNotOsi_IsTheSymbolItself(string rawSymbol)
        {
            Assert.AreEqual(rawSymbol, ArchiveGroup.Of("OPRA.PILLAR", rawSymbol));
        }

        [TestMethod]
        [DataRow("MNQZ6", "MNQ.FUT")]
        [DataRow("ESH7", "ES.FUT")]
        [DataRow("6EM6", "6E.FUT")]
        [DataRow("GCF0", "GC.FUT")]
        public void Of_GlobexContractMonth_IsItsRootAsAFutureGroup(string rawSymbol, string expectedGroup)
        {
            Assert.AreEqual(expectedGroup, ArchiveGroup.Of("GLBX.MDP3", rawSymbol));
        }

        [TestMethod]
        [DataRow("MNQZ6-MNQH7", DisplayName = "a calendar spread")]
        [DataRow("Z6", DisplayName = "no root")]
        [DataRow("MNQ", DisplayName = "the root")]
        [DataRow("MNQA6", DisplayName = "no month code")]
        [DataRow("MNQZ26", DisplayName = "a two-digit year")]
        [DataRow("MNQ.c.0", DisplayName = "a continuous symbol")]
        [DataRow("/MNQ_2612", DisplayName = "another symbology")]
        public void Of_GlobexSymbolThatIsNoContractMonth_IsTheSymbolItself(string rawSymbol)
        {
            Assert.AreEqual(rawSymbol, ArchiveGroup.Of("GLBX.MDP3", rawSymbol));
        }

        [TestMethod]
        [DataRow("XNAS.ITCH", "QQQ")]
        [DataRow("XNAS.ITCH", "BRK.B")]
        [DataRow("DBIX.CORE", "SPX")]
        [DataRow("XNAS.ITCH", "SPXW  261016C06600000")]
        [DataRow("XNAS.ITCH", "MNQZ6")]
        [DataRow("OPRA.PILLAR", "MNQZ6")]
        [DataRow("GLBX.MDP3", "SPXW  261016C06600000")]
        [DataRow("OPRAX.PILLAR", "SPXW  261016C06600000")]
        public void Of_SymbolOfAnotherDatasetsSymbology_IsTheSymbolItself(string dataset, string rawSymbol)
        {
            // an option or contract month is grouped by its root only in the dataset whose symbology it is
            Assert.AreEqual(rawSymbol, ArchiveGroup.Of(dataset, rawSymbol));
        }

        [TestMethod]
        public void Of_NoDatasetOrSymbol_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => ArchiveGroup.Of(string.Empty, "QQQ"));
            Assert.ThrowsException<ArgumentException>(() => ArchiveGroup.Of("XNAS.ITCH", string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => ArchiveGroup.Of(null, "QQQ"));
            Assert.ThrowsException<ArgumentNullException>(() => ArchiveGroup.Of("XNAS.ITCH", null));
        }
    }
}
