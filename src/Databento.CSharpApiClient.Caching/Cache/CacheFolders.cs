using System;
using System.IO;
using System.Linq;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// Checks the folders a run works in before it touches any of them
    /// </summary>
    internal static class CacheFolders
    {
        /// <summary>
        /// Throws unless a dry run's folder is given and neither holds nor is inside any of the folders: a reader of the cache would pick up
        /// what a dry run wrote into it, and a dry run replacing its own output would delete a folder it held
        /// </summary>
        /// <param name="dryRunFolder">The dry run's folder</param>
        /// <param name="folders">The folders the run works in, such as the cache root; an empty one is skipped</param>
        /// <exception cref="ArgumentException">The dry run folder is missing, holds one of the folders or is inside one</exception>
        public static void ThrowIfDryRunFolderInvalid(string dryRunFolder, params string[] folders)
        {
            ArgumentException.ThrowIfNullOrEmpty(dryRunFolder);
            if(folders.Any(folder => !string.IsNullOrEmpty(folder) && (IsInside(dryRunFolder, folder) || IsInside(folder, dryRunFolder))))
            {
                throw new ArgumentException(Invariant($"DryRunFolder {dryRunFolder} must neither hold nor be inside {string.Join(", ", folders.Where(folder => !string.IsNullOrEmpty(folder)))}."), nameof(dryRunFolder));
            }
        }

        /// <summary>
        /// Gets whether <paramref name="path"/> is <paramref name="folder"/> or inside it
        /// </summary>
        public static bool IsInside(string path, string folder)
        {
            string fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)) + Path.DirectorySeparatorChar;
            string fullFolder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder)) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(fullFolder, StringComparison.OrdinalIgnoreCase);
        }
    }
}
