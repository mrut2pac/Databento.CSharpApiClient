using System;
using System.Collections.Generic;
using System.Linq;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// Keeps every report it is told, in order, on the caller's thread; <see cref="Progress{T}"/> would post them elsewhere
    /// </summary>
    /// <typeparam name="T">What is reported</typeparam>
    internal sealed class FakeProgress<T> : IProgress<T>
    {
        private readonly object reportLock = new object();

        private readonly List<T> reports = new List<T>();

        /// <summary>
        /// Gets the reports so far.
        /// </summary>
        public IReadOnlyList<T> Reports
        {
            get
            {
                lock(this.reportLock)
                {
                    return this.reports.ToList();
                }
            }
        }

        /// <summary>
        /// Gets or sets what each report throws once it is kept; <c>null</c>, the default, for nothing.
        /// </summary>
        public Exception Failure { get; set; }

        public void Report(T value)
        {
            lock(this.reportLock)
            {
                this.reports.Add(value);
            }

            if(this.Failure != null)
            {
                throw this.Failure;
            }
        }
    }
}
