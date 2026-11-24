using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Databento.CSharpApiClient.Caching.Cache;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Names the archive files of the cache and finds the current version among a folder's files. A full file is named
    /// <c>&lt;dataset&gt;_&lt;schema&gt;_&lt;group&gt;_&lt;yyyy-MM&gt;.&lt;cutoff&gt;_&lt;yyyyMMddTHHmmss&gt;.full.parquet</c> (the group escaped, see
    /// <see cref="SymbolEscaping"/>) and its delta the same with <c>.delta.&lt;N&gt;</c>, N its generation and the build time its full file's.
    /// The newest full file and the highest generation of its delta are the current version.
    /// </summary>
    internal static class ArchiveFileName
    {
        public const string Extension = ".parquet";

        public const string BuildTimeFormat = "yyyyMMdd'T'HHmmss";

        public const string FullSuffix = ".full";

        public const string DeltaSuffix = ".delta.";

        public const string TemporaryFileExtension = ".tmp";

        /// <summary>
        /// Gets the name a file is written under while it is built, which <see cref="FindCurrentVersion"/> never picks up
        /// </summary>
        /// <param name="fileName">Final file name</param>
        /// <returns>A unique temporary file name</returns>
        public static string GetTemporaryFileName(string fileName)
        {
            return Invariant($"{fileName}.{Guid.NewGuid():N}{TemporaryFileExtension}");
        }

        /// <summary>
        /// Gets the part of the file name every file of an archive file shares, up to the build time
        /// </summary>
        public static string GetSearchPrefix(string dataset, string schema, string group, int year, int month, string cutoff)
        {
            return Invariant($"{dataset}_{schema}_{SymbolEscaping.Escape(group)}_{year:0000}-{month:00}.{cutoff}_");
        }

        /// <summary>
        /// Finds the current version among the files named by a prefix from <see cref="GetSearchPrefix"/>: the full file with the newest
        /// build time in its name, and the highest generation of that build's delta. A delta of an older full file, or of none, is not part
        /// of it.
        /// </summary>
        /// <param name="paths">Files of a month folder, such as its listing; those of other names are passed over</param>
        /// <param name="prefix">File name up to the build time</param>
        /// <returns>The current version, or <c>null</c> when there is no full file</returns>
        public static ArchiveVersion FindCurrentVersion(IEnumerable<string> paths, string prefix)
        {
            List<NamedFile> files = ParseFiles(paths, prefix).ToList();
            if(!files.Any(file => file.Generation == 0))
            {
                return null;
            }

            NamedFile full = files.Where(file => file.Generation == 0).MaxBy(file => file.BuiltUtc);
            List<NamedFile> deltas = files.Where(file => file.Generation > 0 && file.BuiltUtc == full.BuiltUtc).ToList();
            if(deltas.Count == 0)
            {
                return new ArchiveVersion(full.BuiltUtc, full.Path, null, 0);
            }

            NamedFile delta = deltas.MaxBy(file => file.Generation);
            return new ArchiveVersion(full.BuiltUtc, full.Path, delta.Path, delta.Generation);
        }

        /// <summary>
        /// Finds every full file and delta of an archive file among a folder's files, of any build and generation
        /// </summary>
        /// <param name="paths">Files of a month folder, such as its listing; those of other names are passed over</param>
        /// <param name="prefix">File name up to the build time, from <see cref="GetSearchPrefix"/></param>
        /// <returns>The full paths, in the order given</returns>
        public static IReadOnlyList<string> FindFiles(IEnumerable<string> paths, string prefix)
        {
            return ParseFiles(paths, prefix).Select(file => file.Path).ToList();
        }

        /// <summary>
        /// Finds the newest build time any full file or delta of an archive file among a folder's files carries
        /// </summary>
        /// <param name="paths">Files of a month folder, such as its listing; those of other names are passed over</param>
        /// <param name="prefix">File name up to the build time, from <see cref="GetSearchPrefix"/></param>
        /// <returns>The build time, UTC, or <c>null</c> when there is no file</returns>
        public static DateTime? FindLatestBuildTime(IEnumerable<string> paths, string prefix)
        {
            return ParseFiles(paths, prefix).Select(file => (DateTime?)file.BuiltUtc).Max();
        }

        /// <summary>
        /// Reads the group from the name of a full file or delta of a schema's month, or of a temporary file one is written under
        /// </summary>
        /// <param name="fileName">File name, without its folder</param>
        /// <param name="dataset">Dataset of the folder</param>
        /// <param name="schema">Schema of the folder</param>
        /// <param name="year">Year of the month folder</param>
        /// <param name="month">Month of the month folder</param>
        /// <param name="cutoff">Day cutoff of the cache</param>
        /// <param name="group">The group, when the name is one</param>
        /// <returns><c>false</c> when the name is no archive file of that dataset, schema, month and cutoff, such as a day file</returns>
        public static bool TryParseGroup(string fileName, string dataset, string schema, int year, int month, string cutoff, out string group)
        {
            group = null;
            string name = fileName;
            if(name.EndsWith(TemporaryFileExtension, StringComparison.Ordinal) && !TryGetFinalName(fileName, out name))
            {
                return false;
            }

            // neither a dataset, a schema nor an escaped group holds '_', see ArchiveFileIdentity
            string[] parts = name.Split('_');
            if(parts.Length != 5
                || !string.Equals(parts[0], dataset, StringComparison.Ordinal)
                || !string.Equals(parts[1], schema, StringComparison.Ordinal)
                || !SymbolEscaping.TryUnescape(parts[2], out string parsedGroup)
                || !TryParse(name, GetSearchPrefix(dataset, schema, parsedGroup, year, month, cutoff), out _, out _))
            {
                return false;
            }

            group = parsedGroup;
            return true;
        }

        /// <summary>
        /// Gets whether a file name is of a temporary file one of an archive file's full files or deltas is written under, from
        /// <see cref="GetTemporaryFileName"/>
        /// </summary>
        /// <param name="fileName">File name, without its folder</param>
        /// <param name="prefix">File name up to the build time, from <see cref="GetSearchPrefix"/></param>
        public static bool IsTemporaryFileOf(string fileName, string prefix)
        {
            return TryGetFinalName(fileName, out string finalName) && TryParse(finalName, prefix, out _, out _);
        }

        /// <summary>
        /// Reads the final name from the name of a temporary file, <c>&lt;final name&gt;.&lt;guid&gt;.tmp</c>
        /// </summary>
        /// <returns><c>false</c> when the name is not of that form</returns>
        private static bool TryGetFinalName(string temporaryFileName, out string finalName)
        {
            finalName = null;
            if(temporaryFileName.Length <= TemporaryFileExtension.Length || !temporaryFileName.EndsWith(TemporaryFileExtension, StringComparison.Ordinal))
            {
                return false;
            }

            int guidStart = temporaryFileName.LastIndexOf('.', temporaryFileName.Length - TemporaryFileExtension.Length - 1);
            if(guidStart < 0)
            {
                return false;
            }

            finalName = temporaryFileName[..guidStart];
            return true;
        }

        private static IEnumerable<NamedFile> ParseFiles(IEnumerable<string> paths, string prefix)
        {
            foreach(string path in paths)
            {
                if(TryParse(Path.GetFileName(path), prefix, out DateTime builtUtc, out int generation))
                {
                    yield return new NamedFile(path, builtUtc, generation);
                }
            }
        }

        /// <summary>
        /// Reads the build time and generation from a file name. Anything that is not exactly prefix + build time + <c>.full</c> or
        /// <c>.delta.N</c> + extension is not ours, and N must be written as the writer writes it, so no two names stand for one file.
        /// </summary>
        private static bool TryParse(string fileName, string prefix, out DateTime builtUtc, out int generation)
        {
            builtUtc = default;
            generation = 0;
            int buildTimeLength = BuildTimeFormat.Length - 2;
            if(fileName.Length < prefix.Length + buildTimeLength + Extension.Length
                || !fileName.StartsWith(prefix, StringComparison.Ordinal)
                || !fileName.EndsWith(Extension, StringComparison.Ordinal))
            {
                return false;
            }

            ReadOnlySpan<char> buildTime = fileName.AsSpan(prefix.Length, buildTimeLength);
            ReadOnlySpan<char> kind = fileName.AsSpan(prefix.Length + buildTimeLength, fileName.Length - prefix.Length - buildTimeLength - Extension.Length);
            if(!DateTime.TryParseExact(buildTime, BuildTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out builtUtc))
            {
                return false;
            }

            if(kind.SequenceEqual(FullSuffix))
            {
                return true;
            }

            if(!kind.StartsWith(DeltaSuffix, StringComparison.Ordinal))
            {
                return false;
            }

            ReadOnlySpan<char> number = kind.Slice(DeltaSuffix.Length);
            return int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out generation)
                && generation > 0
                && number.SequenceEqual(generation.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// A file of an archive file: its path, its full file's build time and its generation, 0 for the full file
        /// </summary>
        private readonly record struct NamedFile(string Path, DateTime BuiltUtc, int Generation);
    }
}
