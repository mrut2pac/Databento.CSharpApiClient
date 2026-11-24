using System.Collections.Generic;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.Caching
{
    /// <summary>
    /// A check the compactor runs on a group-month's day-file days before it publishes their archive file, such as a record count far
    /// below the same symbol's other days. A day it finds suspect is reported and archived all the same; downloading the day again
    /// replaces it. One instance serves every group of a run, groups compacted at once included, so it keeps no state between calls.
    /// </summary>
    public interface IArchiveDayCheck
    {
        /// <summary>
        /// Gets whether the check judges the days of a Databento schema of a dataset; the others are never shown to it
        /// </summary>
        /// <param name="dataset">Databento dataset, e.g. <c>OPRA.PILLAR</c></param>
        /// <param name="schema">Databento schema, e.g. <c>cbbo-1m</c></param>
        /// <returns><c>true</c> when the check judges them</returns>
        bool AppliesTo(string dataset, string schema);

        /// <summary>
        /// Summarizes a day as the compactor writes it into the new file, for <see cref="FindSuspects"/> to judge by, so a check that needs
        /// more than record counts does not read the file again. Called once for each day the new file holds.
        /// </summary>
        /// <param name="day">The day, with its record count</param>
        /// <param name="records">The day's records, in Databento's order, as the schema's record interface (<see cref="ICbboRecord"/>, ...);
        /// read-only, and a check must not change a record either, since they are what the file is written from</param>
        /// <returns>What the check keeps of the day, which <see cref="ArchiveGroupDays.SummaryOf"/> gives back; <c>null</c> for nothing</returns>
        object Summarize(ArchiveDay day, IReadOnlyList<IRecord> records);

        /// <summary>
        /// Finds the suspect days among the day-file days of a group-month
        /// </summary>
        /// <param name="days">Every day the group-month serves once the file is published, which of them come from day files, and this
        /// check's summaries of the days the new file holds</param>
        /// <returns>The suspect day-file days, each at most once; empty when none is</returns>
        IEnumerable<ArchiveDaySuspect> FindSuspects(ArchiveGroupDays days);
    }
}
