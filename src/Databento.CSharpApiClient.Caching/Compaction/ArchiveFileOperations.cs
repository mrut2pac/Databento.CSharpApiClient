using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// The file operations of a compaction run: deleting a file that may be in use, retried with back-off
    /// </summary>
    internal sealed class ArchiveFileOperations
    {
        /// <summary>
        /// Longest wait between two delete attempts, however many are configured
        /// </summary>
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(1);

        private readonly ArchiveCompactorOptions options;

        private readonly Action<string> deleteFile;

        /// <summary>
        /// Creates a run's file operations
        /// </summary>
        /// <param name="options">The run's settings</param>
        /// <param name="deleteFile">What deletes a file; <c>null</c> for <see cref="File.Delete"/></param>
        public ArchiveFileOperations(ArchiveCompactorOptions options, Action<string> deleteFile)
        {
            this.options = options;
            this.deleteFile = deleteFile ?? File.Delete;
        }

        /// <summary>
        /// Deletes a file, trying again with a doubling wait while it is in use, and asking before every attempt whether it may still be
        /// deleted: a day file saved again during a wait holds data the archive does not
        /// </summary>
        /// <param name="path">File to delete</param>
        /// <param name="mayDelete">Whether the file may still be deleted, asked before every attempt; <c>null</c> when it always may</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Whether the file is gone, still there after the last attempt, or kept because it may no longer be deleted</returns>
        public async Task<DeleteResult> DeleteAsync(string path, Func<bool> mayDelete, CancellationToken cancellationToken)
        {
            TimeSpan delay = this.options.RetireRetryDelay;

            // a first wait configured above the cap is kept, never shortened
            TimeSpan maxDelay = delay > MaxRetryDelay ? delay : MaxRetryDelay;
            for(int attempt = 1; ; ++attempt)
            {
                if(mayDelete != null && !mayDelete())
                {
                    return DeleteResult.Changed;
                }

                try
                {
                    this.deleteFile(path);
                    return DeleteResult.Deleted;
                }
                catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
                {
                    if(attempt >= this.options.RetireAttempts)
                    {
                        return DeleteResult.Left;
                    }
                }

                await Task.Delay(delay, this.options.TimeProvider, cancellationToken).ConfigureAwait(false);
                delay = delay + delay < maxDelay ? delay + delay : maxDelay;
            }
        }

        /// <summary>
        /// Deletes a temporary file once, leaving it when that fails
        /// </summary>
        public static void TryDeleteOnce(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            {
                // a temporary file left behind never matches a version's name, so it does no harm
            }
        }
    }
}
