using System;
using System.Globalization;
using System.Linq;

using Databento.CSharpApiClient.Caching.Cache;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// Says which archive file a version is: the dataset, schema and group it holds, the month and day cutoff it covers, when its full
    /// file was built, and which of the version's files it is (generation 0 the full file, 1 and up a generation of its delta). The file
    /// name is derived from it, so a copied file still says what it holds.
    /// </summary>
    internal sealed class ArchiveFileIdentity
    {
        public ArchiveFileIdentity(string dataset, string schema, string group, int year, int month, string cutoff, DateTime builtUtc, int generation = 0)
        {
            ThrowIfNotNamePart(dataset, nameof(dataset));
            ThrowIfNotNamePart(schema, nameof(schema));
            ThrowIfNotNamePart(cutoff, nameof(cutoff));
            ArgumentException.ThrowIfNullOrEmpty(group);
            if(year < 1 || year > 9999 || month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month), Invariant($"{year}-{month} is not a month."));
            }

            if(builtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(Invariant($"The build time must be UTC, got {builtUtc.Kind}."), nameof(builtUtc));
            }

            if(generation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generation), Invariant($"A generation is 0 for the full file or 1 and up for a delta, got {generation}."));
            }

            this.Dataset = dataset;
            this.Schema = schema;
            this.Group = group;
            this.Year = year;
            this.Month = month;
            this.Cutoff = cutoff;
            this.Generation = generation;

            // the name keeps whole seconds, so the identity does too: one read back from a name equals the one it was built from
            this.BuiltUtc = new DateTime(builtUtc.Ticks - (builtUtc.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the Databento dataset, e.g. <c>OPRA.PILLAR</c>.
        /// </summary>
        public string Dataset { get; }

        /// <summary>
        /// Gets the Databento schema, e.g. <c>cbbo-1m</c>.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the group the file holds, see <see cref="ArchiveGroup"/>.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the year of the month the file covers.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the month the file covers, 1 to 12.
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// Gets the day cutoff the file's days are cut by, e.g. <c>UTC</c>.
        /// </summary>
        public string Cutoff { get; }

        /// <summary>
        /// Gets when the version's full file was built, UTC, to the second; its delta carries the same time.
        /// </summary>
        public DateTime BuiltUtc { get; }

        /// <summary>
        /// Gets the generation: 0 for the full file, 1 and up for its delta, which each compaction rewrites as the next one.
        /// </summary>
        public int Generation { get; }

        /// <summary>
        /// Gets whether this is the full file or its delta, which follows from <see cref="Generation"/>.
        /// </summary>
        public ArchiveFileKind Kind => this.Generation == 0 ? ArchiveFileKind.Full : ArchiveFileKind.Delta;

        /// <summary>
        /// Gets the file name, e.g. <c>OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet</c> for a full file and
        /// <c>...T021500.delta.3.parquet</c> for the third generation of its delta.
        /// </summary>
        public string FileName => ArchiveFileName.GetSearchPrefix(this.Dataset, this.Schema, this.Group, this.Year, this.Month, this.Cutoff)
            + this.BuiltUtc.ToString(ArchiveFileName.BuildTimeFormat, CultureInfo.InvariantCulture)
            + (this.Kind == ArchiveFileKind.Full ? ArchiveFileName.FullSuffix : Invariant($"{ArchiveFileName.DeltaSuffix}{this.Generation}"))
            + ArchiveFileName.Extension;

        /// <summary>
        /// A dataset, schema or cutoff goes into the file name as it is, so it may hold no <c>_</c>, which separates the name's parts
        /// </summary>
        private static void ThrowIfNotNamePart(string text, string paramName)
        {
            ArgumentException.ThrowIfNullOrEmpty(text, paramName);
            if(!text.All(character => char.IsAsciiLetterOrDigit(character) || character == '-' || character == '.'))
            {
                throw new ArgumentException(Invariant($"'{text}' may hold only letters, digits, '-' and '.': '_' separates the file name's parts."), paramName);
            }
        }
    }
}
