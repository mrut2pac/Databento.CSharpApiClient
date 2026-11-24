using System;
using System.Collections.Generic;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// The files of an archive file's current version: the full file of the newest build and the highest generation of that build's delta.
    /// An older generation, such as one a killed run left behind, is never part of the version.
    /// </summary>
    internal sealed class ArchiveVersion
    {
        public ArchiveVersion(DateTime builtUtc, string fullPath, string deltaPath, int deltaGeneration)
        {
            this.BuiltUtc = builtUtc;
            this.FullPath = fullPath;
            this.DeltaPath = deltaPath;
            this.DeltaGeneration = deltaGeneration;
            this.Paths = deltaPath == null ? new[] { fullPath } : new[] { fullPath, deltaPath };
        }

        /// <summary>
        /// Gets when the full file was built, UTC.
        /// </summary>
        public DateTime BuiltUtc { get; }

        /// <summary>
        /// Gets the full path of the full file.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the full path of the current delta, the highest generation of the full file's build, or <c>null</c> when the full file has none.
        /// </summary>
        public string DeltaPath { get; }

        /// <summary>
        /// Gets the generation of the current delta, 0 when the full file has none.
        /// </summary>
        public int DeltaGeneration { get; }

        /// <summary>
        /// Gets the full paths of the version's files: the full file, then the current delta if there is one, which wins a day both list.
        /// </summary>
        public IReadOnlyList<string> Paths { get; }
    }
}
