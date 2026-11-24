using System.Collections.Generic;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// One archive file to build: a group's day files in one month folder of a dataset's schema, to fold into its current version
    /// </summary>
    internal sealed class ArchiveGroupRequest
    {
        public ArchiveGroupRequest(string directory, string relativeDirectory, string dataset, string schema, string group, int year, int month, IReadOnlyList<FoundDayFile> dayFiles, IReadOnlyList<string> archiveFiles)
        {
            this.Directory = directory;
            this.RelativeDirectory = relativeDirectory;
            this.Dataset = dataset;
            this.Schema = schema;
            this.Group = group;
            this.Year = year;
            this.Month = month;
            this.DayFiles = dayFiles;
            this.ArchiveFiles = archiveFiles;
        }

        /// <summary>
        /// Gets the month folder, full path.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Gets the month folder relative to the cache root, e.g. <c>OPRA.PILLAR\cbbo-1m\2026\10</c>, which the dry-run folder mirrors.
        /// </summary>
        public string RelativeDirectory { get; }

        /// <summary>
        /// Gets the Databento dataset, e.g. <c>OPRA.PILLAR</c>.
        /// </summary>
        public string Dataset { get; }

        /// <summary>
        /// Gets the Databento schema, e.g. <c>cbbo-1m</c>.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the group, e.g. <c>SPXW.OPT</c>.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the year of the month.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the month.
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// Gets the group's day files as the run found them, at most one per symbol-day.
        /// </summary>
        public IReadOnlyList<FoundDayFile> DayFiles { get; }

        /// <summary>
        /// Gets the full paths of the group's full files, deltas and temporary files, as the listing of the month folder found them. Only
        /// a run holding the cache root's lock writes archive files, so they are still what the folder holds.
        /// </summary>
        public IReadOnlyList<string> ArchiveFiles { get; }

        /// <summary>
        /// Gets how the report names the file, e.g. <c>OPRA.PILLAR\cbbo-1m\2026\10 SPXW.OPT</c>.
        /// </summary>
        public string DisplayName => Invariant($"{this.RelativeDirectory} {this.Group}");
    }
}
