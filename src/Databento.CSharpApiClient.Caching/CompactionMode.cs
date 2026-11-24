namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// What a compaction writes for a group-month that already has a full file; one that has none gets a full file of its day files in every mode
    /// </summary>
    public enum CompactionMode
    {
        /// <summary>
        /// A rebuild every time: a new full file of the full file, its delta and the day files, also for a group with a delta and no day files
        /// </summary>
        Full,

        /// <summary>
        /// The next delta generation every time: the previous delta's symbol-days overlaid by the day files, so the delta only grows
        /// </summary>
        Delta,

        /// <summary>
        /// As <see cref="Delta"/>, but a rebuild once the next generation would reach the maximum, or the delta outgrew its share of the full file
        /// </summary>
        Auto,
    }
}
