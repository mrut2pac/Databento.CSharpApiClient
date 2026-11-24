using System;

using Databento.CSharpApiClient.Caching.Cache;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Cache
{
    [TestClass]
    public class SymbolEscapingTests
    {
        [TestMethod]
        [DataRow("SPXW  261016C06600000", "SPXW%20%20261016C06600000", DisplayName = "an OSI symbol's padded root")]
        [DataRow("ES/NQ", "ES%2FNQ", DisplayName = "a path separator")]
        [DataRow("A_B", "A%5FB", DisplayName = "the name's separator")]
        [DataRow("50%", "50%25", DisplayName = "the escape character itself")]
        [DataRow("caf\u00e9", "%63%61%66%C3%A9", DisplayName = "a non-ASCII letter, by its UTF-8 bytes")]
        [DataRow("\u20ac1", "%E2%82%AC1", DisplayName = "a three-byte character")]
        [DataRow("BRK.B", "BRK.B", DisplayName = "dots kept")]
        [DataRow("MNQZ6-MNQH7", "MNQZ6-MNQH7", DisplayName = "dashes kept")]
        [DataRow("abcXYZ019", "%61%62%63XYZ019", DisplayName = "lowercase letters escaped, capitals and digits kept")]
        public void Escape_Symbol_ReadsBackExactly(string symbol, string expectedEscaped)
        {
            string escaped = SymbolEscaping.Escape(symbol);

            Assert.AreEqual(expectedEscaped, escaped);
            Assert.IsTrue(SymbolEscaping.TryUnescape(escaped, out string unescaped));
            Assert.AreEqual(symbol, unescaped);
        }

        [TestMethod]
        public void Escape_SymbolsDifferingOnlyInCase_NeverShareANameIgnoringCase()
        {
            // Windows and macOS file systems ignore case, so BRK.B and BRK.b must not land in one file
            Assert.IsFalse(string.Equals(SymbolEscaping.Escape("BRK.B"), SymbolEscaping.Escape("BRK.b"), StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Escape_NoSymbol_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => SymbolEscaping.Escape(string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => SymbolEscaping.Escape(null));
        }

        [TestMethod]
        [DataRow("%41", DisplayName = "a character that needs no escape")]
        [DataRow("%2f", DisplayName = "lowercase hex")]
        [DataRow("caf%c3%a9", DisplayName = "lowercase hex of a non-ASCII letter")]
        [DataRow("A_B", DisplayName = "the name's separator unescaped")]
        [DataRow("A B", DisplayName = "a space unescaped")]
        [DataRow("Ab", DisplayName = "a lowercase letter unescaped")]
        [DataRow("%2", DisplayName = "an escape cut short")]
        [DataRow("A%", DisplayName = "an escape character alone")]
        [DataRow("%ZZ", DisplayName = "an escape of no hex digits")]
        [DataRow("%+1", DisplayName = "an escape of a sign")]
        [DataRow("%C3", DisplayName = "half a UTF-8 character")]
        [DataRow("%FF", DisplayName = "a byte that is no UTF-8")]
        [DataRow("", DisplayName = "empty")]
        [DataRow((string)null, DisplayName = "null")]
        public void TryUnescape_FormEscapeDoesNotWrite_IsRefused(string escaped)
        {
            // only the form Escape writes is read, so no two file names stand for one symbol
            Assert.IsFalse(SymbolEscaping.TryUnescape(escaped, out string symbol));
            Assert.IsNull(symbol);
        }
    }
}
