using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="Utils"/> — timestamp precision, price conversion, and sentinels.
    /// </summary>
    [TestClass]
    public class UtilsTests
    {
        // =====================================================================================
        // FromUnixNs — 100 ns tick precision
        // =====================================================================================

        [TestMethod]
        public void FromUnixNs_WholeMicrosecond_ParsedExactly()
        {
            // 1 microsecond = 1000 ns = 10 ticks; should round-trip without any loss.
            DateTimeOffset expected = new DateTimeOffset(2022, 5, 16, 13, 30, 0, 500, TimeSpan.Zero); // +500ms
            ulong ns = (ulong)((expected - DateTimeOffset.UnixEpoch).Ticks * 100L);

            DateTimeOffset actual = Utils.FromUnixNs(ns);

            Assert.AreEqual(expected, actual, "Whole-microsecond timestamp should round-trip exactly.");
        }

        [TestMethod]
        public void FromUnixNs_100NsTick_PreservedDownToOneTick()
        {
            // 300 ns = 3 ticks; the tick-boundary value must survive.
            DateTimeOffset base_ = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            ulong ns = (ulong)((base_ - DateTimeOffset.UnixEpoch).Ticks * 100L) + 300UL;

            DateTimeOffset actual = Utils.FromUnixNs(ns);

            // 300 ns / 100 = 3 ticks exactly.
            Assert.AreEqual(base_.AddTicks(3), actual);
        }

        [TestMethod]
        public void FromUnixNs_SubTickNs_TruncatedToNearestTick()
        {
            // 150 ns = 1.5 ticks → truncated to 1 tick (100 ns).
            DateTimeOffset base_ = new DateTimeOffset(2022, 5, 16, 0, 0, 0, TimeSpan.Zero);
            ulong baseNs = (ulong)((base_ - DateTimeOffset.UnixEpoch).Ticks * 100L);
            ulong ns = baseNs + 150UL; // +150 ns (+1.5 ticks)

            DateTimeOffset actual = Utils.FromUnixNs(ns);

            // Integer division: 150 / 100 = 1 tick (truncation toward zero).
            Assert.AreEqual(base_.AddTicks(1), actual);
        }

        [TestMethod]
        public void FromUnixNs_Zero_ReturnsMinValue()
        {
            Assert.AreEqual(DateTimeOffset.MinValue, Utils.FromUnixNs(0UL));
        }

        [TestMethod]
        public void FromUnixNs_Sentinel_ReturnsMinValue()
        {
            const ulong Sentinel = 0xFFFF_FFFF_FFFF_FFFFUL;
            Assert.AreEqual(DateTimeOffset.MinValue, Utils.FromUnixNs(Sentinel));
        }

        [TestMethod]
        public void FromUnixNs_UtcKind_ReturnedDateTimeOffsetIsUtc()
        {
            ulong ns = (ulong)((new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero) - DateTimeOffset.UnixEpoch).Ticks * 100L);
            DateTimeOffset result = Utils.FromUnixNs(ns);
            Assert.AreEqual(TimeSpan.Zero, result.Offset, "Offset must be UTC (zero).");
        }

        // =====================================================================================
        // ParseUnixNs
        // =====================================================================================

        [TestMethod]
        public void ParseUnixNs_ValidString_ParsedToUtcDateTime()
        {
            DateTimeOffset expected = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
            ulong ns = (ulong)((expected - DateTimeOffset.UnixEpoch).Ticks * 100L);
            string nsStr = ns.ToString(CultureInfo.InvariantCulture);

            DateTime result = Utils.ParseUnixNs(nsStr);

            Assert.AreEqual(expected.UtcDateTime, result);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
        }

        [TestMethod]
        public void ParseUnixNs_NullOrEmpty_ReturnsMinValue()
        {
            Assert.AreEqual(DateTime.MinValue, Utils.ParseUnixNs(null));
            Assert.AreEqual(DateTime.MinValue, Utils.ParseUnixNs(string.Empty));
            Assert.AreEqual(DateTime.MinValue, Utils.ParseUnixNs("   "));
        }

        [TestMethod]
        public void ParseUnixNs_NonNumeric_ReturnsMinValue()
        {
            Assert.AreEqual(DateTime.MinValue, Utils.ParseUnixNs("not-a-number"));
        }

        // =====================================================================================
        // NanoToDouble / DoubleToNano — price round-trip
        // =====================================================================================

        [TestMethod]
        public void NanoToDouble_MinValue_ReturnsNaN()
        {
            Assert.IsTrue(double.IsNaN(Utils.NanoToDouble(long.MinValue)));
        }

        [TestMethod]
        public void NanoToDouble_MaxValue_ReturnsNaN()
        {
            Assert.IsTrue(double.IsNaN(Utils.NanoToDouble(long.MaxValue)));
        }

        [TestMethod]
        public void DoubleToNano_TypicalPrice_RoundTripsExactly()
        {
            double[] prices = new[] { 0.01, 1.0, 410.25, 6475.50, 0.0001 };

            foreach(double price in prices)
            {
                long nano = Utils.DoubleToNano(price);
                double roundTripped = Utils.NanoToDouble(nano);
                Assert.AreEqual(price, roundTripped, 1e-7, $"Price {price} did not round-trip.");
            }
        }

        [TestMethod]
        public void DoubleToNano_FloatingPointNoise_RoundedCorrectly()
        {
            // A price reconstructed from nano may have accumulated floating-point noise
            // (e.g. 4.0099999999 instead of 4.01). DoubleToNano must round, not truncate.
            double price = Utils.NanoToDouble(4_010_000_000L); // 4.01 nano representation
            long nano = Utils.DoubleToNano(price);
            Assert.AreEqual(4_010_000_000L, nano, "Floating-point noise must be removed by rounding.");
        }

        [TestMethod]
        public void DoubleToNano_ExactHalfway_RoundedAwayFromZero()
        {
            // 0.0000000005 * 1e9 = 0.5 → rounds to 1 (away from zero).
            long nano = Utils.DoubleToNano(0.0000000005);
            Assert.AreEqual(1L, nano, "Midpoint must round away from zero.");
        }
    }
}
