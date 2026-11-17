using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// Reads the values a record class holds for "undefined" as <see langword="null"/>, for the record interfaces.
    /// </summary>
    internal static class UndefinedValues
    {
        public static double? Price(double price)
        {
            return double.IsNaN(price) ? null : price;
        }

        public static double? Price(double? price)
        {
            return price.HasValue ? Price(price.Value) : null;
        }

        public static DateTime? Timestamp(DateTime timestamp)
        {
            return timestamp == DateTime.MinValue ? null : timestamp;
        }

        public static DateTime? Timestamp(DateTime? timestamp)
        {
            return timestamp.HasValue ? Timestamp(timestamp.Value) : null;
        }

        public static long? Quantity(long quantity)
        {
            return quantity == long.MaxValue || quantity == int.MaxValue ? null : quantity;
        }
    }
}
