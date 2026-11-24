using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Caching.Compaction
{
    /// <summary>
    /// How a compaction run reads the day files of one Databento schema and writes its archive files
    /// </summary>
    internal interface IArchiveSchemaHandler
    {
        /// <summary>
        /// Gets the Databento schema, e.g. <c>cbbo-1m</c>.
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// Builds the next version of one archive file of the schema
        /// </summary>
        /// <param name="request">The file and its day files</param>
        /// <param name="run">What the run's groups share</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CompactAsync(ArchiveGroupRequest request, CompactionRun run, CancellationToken cancellationToken);
    }
}
