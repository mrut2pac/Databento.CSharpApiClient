using System;
using System.Globalization;
using System.IO;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// Names the day files of the cache, one per symbol and UTC day: <c>&lt;symbol&gt;_&lt;yyyy-MM-dd&gt;.&lt;cutoff&gt;.json.gz</c>, the symbol
    /// escaped (see <see cref="SymbolEscaping"/>), in the folder <c>&lt;root&gt;\&lt;dataset&gt;\&lt;schema&gt;\&lt;yyyy&gt;\&lt;MM&gt;</c> of its day.
    /// </summary>
    internal static class DayFileName
    {
        /// <summary>
        /// Extension of a day file: Databento's JSON records, gzip-compressed
        /// </summary>
        public const string Extension = ".json.gz";

        private const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Gets a day file's name
        /// </summary>
        /// <param name="symbol">Databento raw symbol</param>
        /// <param name="date">UTC day</param>
        /// <param name="cutoff">Day cutoff tag of the cache, see <see cref="CacheMetadata.DayCutoff"/></param>
        /// <returns>The file name, e.g. <c>SPXW%20%20261016C06600000_2026-10-16.UTC.json.gz</c></returns>
        public static string Format(string symbol, DateTime date, string cutoff)
        {
            ArgumentException.ThrowIfNullOrEmpty(cutoff);

            return SymbolEscaping.Escape(symbol) + "_" + date.ToString(DateFormat, CultureInfo.InvariantCulture) + "." + cutoff + Extension;
        }

        /// <summary>
        /// Reads a day file's name. A name of another cutoff, or written in any form <see cref="Format"/> does not write, is not one.
        /// </summary>
        /// <param name="fileName">File name, without its folder</param>
        /// <param name="cutoff">Day cutoff tag of the cache</param>
        /// <param name="symbol">The raw symbol</param>
        /// <param name="date">The UTC day</param>
        /// <returns><c>true</c> when the name is a day file's of this cutoff</returns>
        public static bool TryParse(string fileName, string cutoff, out string symbol, out DateTime date)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentException.ThrowIfNullOrEmpty(cutoff);

            symbol = null;
            date = default;
            string suffix = "." + cutoff + Extension;
            if(!fileName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return false;
            }

            string baseName = fileName.Substring(0, fileName.Length - suffix.Length);
            int separator = baseName.LastIndexOf('_');
            if(separator < 1
                || !DateTime.TryParseExact(baseName.AsSpan(separator + 1), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                || !SymbolEscaping.TryUnescape(baseName.Substring(0, separator), out symbol))
            {
                symbol = null;
                date = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the folder of a schema's files for a month: <c>&lt;root&gt;\&lt;dataset&gt;\&lt;schema&gt;\&lt;yyyy&gt;\&lt;MM&gt;</c>
        /// </summary>
        /// <param name="cacheRoot">Cache root</param>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c></param>
        /// <param name="schema">Databento schema, e.g. <c>cbbo-1m</c></param>
        /// <param name="year">Year</param>
        /// <param name="month">Month, 1 to 12</param>
        /// <returns>The folder's path</returns>
        public static string GetMonthDirectory(string cacheRoot, string dataset, string schema, int year, int month)
        {
            return Path.Combine(cacheRoot, dataset, schema, year.ToString("0000", CultureInfo.InvariantCulture), month.ToString("00", CultureInfo.InvariantCulture));
        }
    }
}
