using System;
using System.IO;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// The file a restore writes into the cache root before it touches the cache, and deletes once the cache is whole again. While it is
    /// there, nothing that reads or writes the cache starts, so nothing sees a half-restored cache; a restore that was stopped leaves it,
    /// and running the restore again takes it over.
    /// </summary>
    internal static class CacheRestoreMarker
    {
        /// <summary>
        /// Name of the marker file in the cache root
        /// </summary>
        public const string FileName = "RESTORE_IN_PROGRESS";

        /// <summary>
        /// Throws when a restore of the cache root is in progress, or was stopped before it finished
        /// </summary>
        /// <param name="cacheRoot">Cache root</param>
        /// <param name="refused">What is refused, for the message, e.g. "the compaction"</param>
        /// <exception cref="InvalidOperationException">The marker is there</exception>
        public static void ThrowIfPresent(string cacheRoot, string refused)
        {
            string path = GetPath(cacheRoot);
            if(!File.Exists(path))
            {
                return;
            }

            string restore;
            try
            {
                restore = File.ReadAllText(path).Trim();
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            {
                restore = ex.Message;
            }

            throw new InvalidOperationException(Invariant($"{cacheRoot} is being restored from a backup ({restore}), so {refused} does not start. Let the restore finish, or run it again if it was stopped: {path} is deleted once the cache is whole."));
        }

        /// <summary>
        /// Writes the marker into the cache root
        /// </summary>
        /// <param name="cacheRoot">Cache root</param>
        /// <param name="backupName">Name of the backup being restored</param>
        /// <param name="startUtc">When the restore started</param>
        public static void Write(string cacheRoot, string backupName, DateTime startUtc)
        {
            Directory.CreateDirectory(cacheRoot);
            File.WriteAllText(GetPath(cacheRoot), Invariant($"backup {backupName}, started {startUtc:yyyy-MM-dd HH:mm:ss} UTC on {Environment.MachineName}"));
        }

        private static string GetPath(string cacheRoot)
        {
            return Path.Combine(cacheRoot, FileName);
        }
    }
}
