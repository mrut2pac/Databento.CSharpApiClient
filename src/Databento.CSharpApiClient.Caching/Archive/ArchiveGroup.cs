using System;
using System.Linq;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Gets the group an archive file holds a symbol under: the parent symbol of an OPRA option (<c>SPXW.OPT</c>) or a CME Globex
    /// contract month (<c>MNQ.FUT</c>), else the symbol itself. The rule is fixed, so every reader and writer of a cache files a
    /// symbol under the same group.
    /// </summary>
    internal static class ArchiveGroup
    {
        private const int OsiSymbolLength = 21;

        private const int OsiRootLength = 6;

        private const string FutureMonthCodes = "FGHJKMNQUVXZ";

        private const string OptionParentSuffix = ".OPT";

        private const string FutureParentSuffix = ".FUT";

        private const string OpraDatasetPrefix = "OPRA.";

        /// <summary>
        /// Gets the group of a raw symbol of a dataset
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c></param>
        /// <param name="rawSymbol">Databento raw symbol, in the dataset's own symbology</param>
        /// <returns>The group, e.g. <c>SPXW.OPT</c> for <c>SPXW  261016C06600000</c>, <c>MNQ.FUT</c> for <c>MNQZ6</c>, <c>SPX</c> for <c>SPX</c></returns>
        public static string Of(string dataset, string rawSymbol)
        {
            ArgumentException.ThrowIfNullOrEmpty(dataset);
            ArgumentException.ThrowIfNullOrEmpty(rawSymbol);

            if(dataset.StartsWith(OpraDatasetPrefix, StringComparison.Ordinal) && TryGetOptionRoot(rawSymbol, out string optionRoot))
            {
                return optionRoot + OptionParentSuffix;
            }

            if(string.Equals(dataset, Datasets.GlbxMdp3, StringComparison.Ordinal) && TryGetFutureRoot(rawSymbol, out string futureRoot))
            {
                return futureRoot + FutureParentSuffix;
            }

            return rawSymbol;
        }

        /// <summary>
        /// Gets the root of a 21-character OSI symbol: <c>SPXW  261016C06600000</c> is <c>SPXW</c>
        /// </summary>
        private static bool TryGetOptionRoot(string rawSymbol, out string root)
        {
            // root padded to 6, then expiration yyMMdd, C or P, and the strike in thousandths as 8 digits
            root = rawSymbol.Length == OsiSymbolLength ? rawSymbol.Substring(0, OsiRootLength).TrimEnd(' ') : string.Empty;
            bool isOsi = root.Length > 0
                && root.All(character => char.IsAsciiLetterOrDigit(character) || character == '.')
                && rawSymbol.Substring(OsiRootLength, 6).All(char.IsAsciiDigit)
                && (rawSymbol[OsiRootLength + 6] == 'C' || rawSymbol[OsiRootLength + 6] == 'P')
                && rawSymbol.Substring(OsiRootLength + 7).All(char.IsAsciiDigit);
            if(!isOsi)
            {
                root = null;
            }

            return isOsi;
        }

        /// <summary>
        /// Gets the root of a contract month's raw symbol, the symbol without its month code and year digit: <c>MNQZ6</c> is <c>MNQ</c>
        /// </summary>
        private static bool TryGetFutureRoot(string rawSymbol, out string root)
        {
            int rootLength = rawSymbol.Length - 2;
            bool isContractMonth = rootLength >= 1
                && char.IsAsciiDigit(rawSymbol[^1])
                && FutureMonthCodes.Contains(rawSymbol[^2], StringComparison.Ordinal)
                && rawSymbol.Take(rootLength).All(char.IsAsciiLetterOrDigit);
            root = isContractMonth ? rawSymbol.Substring(0, rootLength) : null;
            return isContractMonth;
        }
    }
}
