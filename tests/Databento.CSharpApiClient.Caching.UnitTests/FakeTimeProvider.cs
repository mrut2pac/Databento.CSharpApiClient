using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// A clock that stands still until a test moves it. A timer it creates fires at once, on the thread pool, after moving the clock by
    /// its due time and noting it in <see cref="Waits"/>, so a retry wait takes no real time and a test sees how long it was.
    /// </summary>
    internal sealed class FakeTimeProvider : TimeProvider
    {
        private readonly object timeLock = new object();

        private readonly List<TimeSpan> waits = new List<TimeSpan>();

        private DateTimeOffset utcNow;

        public FakeTimeProvider(DateTime utcNow)
        {
            this.utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
        }

        /// <summary>
        /// Gets the timestamp frequency: a timestamp is the clock's ticks.
        /// </summary>
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        /// <summary>
        /// Gets the due times of the timers created so far, in the order they were started.
        /// </summary>
        public IReadOnlyList<TimeSpan> Waits
        {
            get
            {
                lock(this.timeLock)
                {
                    return this.waits.ToList();
                }
            }
        }

        public override DateTimeOffset GetUtcNow()
        {
            lock(this.timeLock)
            {
                return this.utcNow;
            }
        }

        /// <summary>
        /// Gets or sets what <see cref="GetTimestamp"/> throws, so a test can fail a run once it is under way; <c>null</c>, the default, for nothing.
        /// </summary>
        public Exception TimestampFailure { get; set; }

        public override long GetTimestamp()
        {
            if(this.TimestampFailure != null)
            {
                throw this.TimestampFailure;
            }

            lock(this.timeLock)
            {
                return this.utcNow.UtcTicks;
            }
        }

        public void SetUtcNow(DateTime utcNow)
        {
            lock(this.timeLock)
            {
                this.utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
            }
        }

        public void Advance(TimeSpan time)
        {
            lock(this.timeLock)
            {
                this.utcNow += time;
            }
        }

        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            FakeTimer timer = new FakeTimer(this, callback, state);
            timer.Change(dueTime, period);
            return timer;
        }

        private void Fire(TimeSpan dueTime, TimerCallback callback, object state)
        {
            lock(this.timeLock)
            {
                this.waits.Add(dueTime);
                this.utcNow += dueTime;
            }

            ThreadPool.QueueUserWorkItem(_ => callback(state));
        }

        /// <summary>
        /// A timer that fires once, at once, whatever its period
        /// </summary>
        private sealed class FakeTimer : ITimer
        {
            private readonly FakeTimeProvider clock;

            private readonly TimerCallback callback;

            private readonly object state;

            private bool disposed;

            public FakeTimer(FakeTimeProvider clock, TimerCallback callback, object state)
            {
                this.clock = clock;
                this.callback = callback;
                this.state = state;
            }

            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                if(!this.disposed && dueTime != Timeout.InfiniteTimeSpan)
                {
                    this.clock.Fire(dueTime, this.callback, this.state);
                }

                return !this.disposed;
            }

            public void Dispose()
            {
                this.disposed = true;
            }

            public ValueTask DisposeAsync()
            {
                this.disposed = true;
                return ValueTask.CompletedTask;
            }
        }
    }
}
