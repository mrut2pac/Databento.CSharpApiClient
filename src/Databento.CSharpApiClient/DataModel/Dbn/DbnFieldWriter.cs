using System;
using System.Globalization;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Converts record values to their DBN fields, the reverse of what the decoders read: undefined values become DBN's
    /// undefined values, and a value the field cannot hold is refused rather than altered.
    /// </summary>
    internal static class DbnFieldWriter
    {
        // DBN's undefined price and timestamp
        private const long UndefinedPrice = long.MaxValue;

        private const ulong UndefinedTimestamp = ulong.MaxValue;

        // the largest price in nanos a long holds, as a double; at and beyond it the conversion would overflow
        private const double PriceNanoLimit = 9.2233720368547758E18;

        // the last 100 ns tick u64 nanoseconds hold short of u64::MAX, DBN's undefined timestamp: some time in 2554
        private const ulong MaxTicks = (ulong.MaxValue - 1) / 100UL;

        public static long Price(double price, string field)
        {
            if(double.IsNaN(price))
            {
                return UndefinedPrice;
            }

            double nanos = price * 1e9;
            if(double.IsInfinity(price) || nanos >= PriceNanoLimit || nanos <= -PriceNanoLimit)
            {
                throw Invalid(field, price, "it is out of the range of a DBN price");
            }

            return Utils.DoubleToNano(price);
        }

        public static ulong Timestamp(DateTime timestamp, string field)
        {
            if(timestamp == DateTime.MinValue)
            {
                return UndefinedTimestamp;
            }

            DateTime utc = timestamp.Kind == DateTimeKind.Local ? timestamp.ToUniversalTime() : timestamp;
            return Nanoseconds(utc.Ticks - DateTime.UnixEpoch.Ticks, timestamp, field);
        }

        public static ulong Timestamp(DateTime? timestamp, string field)
        {
            return timestamp.HasValue ? Timestamp(timestamp.Value, field) : UndefinedTimestamp;
        }

        public static ulong Timestamp(DateTimeOffset timestamp, string field)
        {
            return Nanoseconds((timestamp - DateTimeOffset.UnixEpoch).Ticks, timestamp, field);
        }

        // ticks since the UNIX epoch as DBN's u64 nanoseconds (1 tick = 100 ns), refused rather than wrapped past them
        private static ulong Nanoseconds(long ticks, object timestamp, string field)
        {
            if(ticks < 0)
            {
                throw Invalid(field, timestamp, "it is before the UNIX epoch");
            }

            if((ulong)ticks > MaxTicks)
            {
                throw Invalid(field, timestamp, "it is past the last timestamp DBN holds, in 2554");
            }

            return (ulong)ticks * 100UL;
        }

        public static byte Side(TradeAggressor side, string field)
        {
            if(side != TradeAggressor.Buyer && side != TradeAggressor.Seller && side != TradeAggressor.None)
            {
                throw Invalid(field, (byte)side, "a side is 'A', 'B' or 'N'");
            }

            return (byte)side;
        }

        public static byte Char(char? value, string field)
        {
            return value.HasValue ? Char(value.Value, field) : (byte)0;
        }

        public static byte Char(char value, string field)
        {
            if(value > 0x7F)
            {
                throw Invalid(field, value, "it is not ASCII");
            }

            return (byte)value;
        }

        public static byte Byte(int value, string field)
        {
            if(value < byte.MinValue || value > byte.MaxValue)
            {
                throw Invalid(field, value, "the field is one byte");
            }

            return (byte)value;
        }

        /// <summary>
        /// Writes text as a fixed-width C string into <paramref name="field"/>, which must be zeroed: ASCII only, and at most
        /// one character less than the width, the last byte kept for the terminating NUL, as Databento's encoder requires.
        /// </summary>
        public static void CString(Span<byte> field, string value, string name)
        {
            if(string.IsNullOrEmpty(value))
            {
                return;
            }

            if(value.Length > field.Length - 1)
            {
                throw Invalid(name, value, string.Format(CultureInfo.InvariantCulture, "the field holds at most {0} characters", field.Length - 1));
            }

            for(int i = 0; i < value.Length; i++)
            {
                if(value[i] > 0x7F)
                {
                    throw Invalid(name, value, "it is not ASCII");
                }

                field[i] = (byte)value[i];
            }
        }

        public static ArgumentException Invalid(string field, object value, string reason)
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, "DBN cannot hold {0} {1}: {2}.", field, value, reason));
        }
    }
}
