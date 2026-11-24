using System;
using System.IO;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// The exclusive lock a run that writes the cache's archive files holds on its root: a compaction, a backup or a restore. It is the
    /// file <c>cache.lock</c> in the root, held open with no sharing and deleted when it is closed, so the lock is released however the
    /// process ends.
    /// </summary>
    internal sealed class CacheRootLock : IDisposable
    {
        /// <summary>
        /// Name of the lock file in the cache root
        /// </summary>
        public const string FileName = "cache.lock";

        private readonly FileStream file;

        private CacheRootLock(FileStream file)
        {
            this.file = file;
        }

        /// <summary>
        /// Takes the lock of a cache root
        /// </summary>
        /// <param name="cacheRoot">Cache root, which exists</param>
        /// <param name="refused">What is refused when the lock is held, for the message, e.g. "the compaction"</param>
        /// <returns>The lock, released when disposed</returns>
        /// <exception cref="InvalidOperationException">The lock cannot be taken, as when another run holds it</exception>
        public static CacheRootLock Take(string cacheRoot, string refused)
        {
            string path = GetPath(cacheRoot);
            try
            {
                return new CacheRootLock(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose));
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException(Invariant($"{path} cannot be taken, so {refused} does not start: another compaction, backup or restore of the cache may hold it. {ex.Message}"), ex);
            }
        }

        /// <summary>
        /// Gets whether a path is a cache root's lock file
        /// </summary>
        public static bool IsLockFile(string path)
        {
            return string.Equals(Path.GetFileName(path), FileName, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            this.file.Dispose();
        }

        private static string GetPath(string cacheRoot)
        {
            return Path.Combine(cacheRoot, FileName);
        }
    }
}
