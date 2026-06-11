using System;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient
{
    internal class Utils
    {
        // no timestamp
        private const ulong UndefinedTimestamp = 0xFFFF_FFFF_FFFF_FFFFUL;

        public static DateTime ParseUnixNs(string nsRaw)
        {
            if(string.IsNullOrWhiteSpace(nsRaw))
            {
                return DateTime.MinValue;
            }

            ulong ns;
            if(!ulong.TryParse(nsRaw, out ns))
            {
                return DateTime.MinValue;
            }

            return FromUnixNs(ns).UtcDateTime;
        }

        public static DateTimeOffset FromUnixNs(ulong ns)
        {
            if(ns == 0UL || ns == UndefinedTimestamp)
            {
                return DateTimeOffset.MinValue;
            }

            // 1 tick = 100 ns, so converting via ticks preserves precision down to 100 ns
            // (DateTime's resolution) instead of truncating to whole milliseconds. The remaining
            // sub-100ns nanosecond detail cannot be represented by DateTime and is dropped.
            long ticks = (long)(ns / 100UL);
            return DateTimeOffset.UnixEpoch.AddTicks(ticks);
        }

        public static double ParseNanoPrice(string nanoRaw)
        {
            if(string.IsNullOrWhiteSpace(nanoRaw))
            {
                return double.NaN;
            }

            long nanoPrice;
            if(!long.TryParse(nanoRaw, out nanoPrice))
            {
                return double.NaN;
            }

            return NanoToDouble(nanoPrice);
        }

        public static double NanoToDouble(long nano)
        {
            if(nano == long.MinValue || nano == long.MaxValue)
            {
                return double.NaN;
            }

            return nano / 1e9;
        }

        public static long DoubleToNano(double price)
        {
            // Round to nearest rather than truncating toward zero, so a value reconstructed from a
            // prior NanoToDouble (e.g. 4.0099999999) maps back to the intended fixed-point nano value.
            return (long)Math.Round(price * 1e9, MidpointRounding.AwayFromZero);
        }

        public static TradeAggressor ParseTradeAggressor(string s)
        {
            if(string.IsNullOrEmpty(s))
            {
                return TradeAggressor.None;
            }

            if(s[0] == 'A')
            {
                return TradeAggressor.Seller;
            }
            else if(s[0] == 'B')
            {
                return TradeAggressor.Buyer;
            }
            else
            {
                return TradeAggressor.None;
            }
        }

        public static MessageInfoBits ParseFlags(string s)
        {
            if(string.IsNullOrEmpty(s) || !byte.TryParse(s, out byte value))
            {
                return MessageInfoBits.None;
            }

            return (MessageInfoBits)value;
        }
    }
}
