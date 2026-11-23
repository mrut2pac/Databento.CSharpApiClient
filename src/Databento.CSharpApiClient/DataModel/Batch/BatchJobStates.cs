namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// The states of a batch job, as <see cref="BatchJob.State"/> names them and
    /// <see cref="DatabentoJsonClient.ListBatchJobsAsync(System.Collections.Generic.IReadOnlyList{string}, System.DateTimeOffset?, System.Threading.CancellationToken)"/>
    /// filters on them.
    /// </summary>
    public static class BatchJobStates
    {
        /// <summary>Waiting to be processed.</summary>
        public const string Queued = "queued";

        /// <summary>Being processed.</summary>
        public const string Processing = "processing";

        /// <summary>Processed; its files can be downloaded until <see cref="BatchJob.TsExpiration"/>.</summary>
        public const string Done = "done";

        /// <summary>Its files were deleted.</summary>
        public const string Expired = "expired";
    }
}
