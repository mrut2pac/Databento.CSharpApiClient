using System;
using System.Collections.Generic;
using System.Linq;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// A day check whose answers a test sets, and which notes every day it summarizes and every group-month it judges
    /// </summary>
    internal sealed class FakeArchiveDayCheck : IArchiveDayCheck
    {
        private readonly object callLock = new object();

        private readonly List<(ArchiveDay Day, IReadOnlyList<IRecord> Records)> summarizedDays = new List<(ArchiveDay Day, IReadOnlyList<IRecord> Records)>();

        private readonly List<ArchiveGroupDays> judgedGroups = new List<ArchiveGroupDays>();

        /// <summary>
        /// Gets or sets which dataset and schema the check judges; all of them by default.
        /// </summary>
        public Func<string, string, bool> Judges { get; set; } = (dataset, schema) => true;

        /// <summary>
        /// Gets or sets what the check keeps of a day; nothing by default.
        /// </summary>
        public Func<ArchiveDay, IReadOnlyList<IRecord>, object> Summary { get; set; } = (day, records) => null;

        /// <summary>
        /// Gets or sets the suspects the check finds in a group-month; none by default.
        /// </summary>
        public Func<ArchiveGroupDays, IEnumerable<ArchiveDaySuspect>> Suspects { get; set; } = days => Enumerable.Empty<ArchiveDaySuspect>();

        /// <summary>
        /// Gets each day summarized so far, with its records, in the order it was.
        /// </summary>
        public IReadOnlyList<(ArchiveDay Day, IReadOnlyList<IRecord> Records)> SummarizedDays
        {
            get
            {
                lock(this.callLock)
                {
                    return this.summarizedDays.ToList();
                }
            }
        }

        /// <summary>
        /// Gets each group-month judged so far, in the order it was.
        /// </summary>
        public IReadOnlyList<ArchiveGroupDays> JudgedGroups
        {
            get
            {
                lock(this.callLock)
                {
                    return this.judgedGroups.ToList();
                }
            }
        }

        public bool AppliesTo(string dataset, string schema)
        {
            return this.Judges(dataset, schema);
        }

        public object Summarize(ArchiveDay day, IReadOnlyList<IRecord> records)
        {
            lock(this.callLock)
            {
                this.summarizedDays.Add((day, records));
            }

            return this.Summary(day, records);
        }

        public IEnumerable<ArchiveDaySuspect> FindSuspects(ArchiveGroupDays days)
        {
            lock(this.callLock)
            {
                this.judgedGroups.Add(days);
            }

            return this.Suspects(days);
        }
    }
}
