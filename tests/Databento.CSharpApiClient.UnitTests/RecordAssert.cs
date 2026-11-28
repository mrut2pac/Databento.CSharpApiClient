using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Compares records property by property, descending into their levels, so a round trip is checked on every field the
    /// record class has rather than on a few picked out by hand.
    /// </summary>
    internal static class RecordAssert
    {
        /// <summary>Asserts that every public property of the two records, and of their levels, is equal; NaN equals NaN.</summary>
        public static void AllPropertiesEqual(object expected, object actual, string path = null)
        {
            path ??= expected?.GetType().Name ?? "record";
            if(expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual, path);
                return;
            }

            Type type = expected.GetType();
            Assert.AreEqual(type, actual.GetType(), path);
            if(IsLeaf(type))
            {
                Assert.IsTrue(expected.Equals(actual), $"{path}: expected <{expected}>, actual <{actual}>");
                return;
            }

            if(expected is IList expectedList)
            {
                IList actualList = (IList)actual;
                Assert.AreEqual(expectedList.Count, actualList.Count, path + ".Count");
                for(int i = 0; i < expectedList.Count; i++)
                {
                    AllPropertiesEqual(expectedList[i], actualList[i], $"{path}[{i}]");
                }

                return;
            }

            foreach(PropertyInfo property in Properties(type))
            {
                AllPropertiesEqual(property.GetValue(expected), property.GetValue(actual), path + "." + property.Name);
            }
        }

        /// <summary>
        /// Asserts that no public property of a record is left at the value a new instance has, apart from those named, so a
        /// round trip of the record exercises every field.
        /// </summary>
        public static void NoPropertyAtDefault(object record, params string[] exempt)
        {
            object fresh = Activator.CreateInstance(record.GetType());
            foreach(PropertyInfo property in Properties(record.GetType()).Where(property => !exempt.Contains(property.Name)))
            {
                object value = property.GetValue(record);
                object defaultValue = property.GetValue(fresh);
                Assert.IsFalse(Equals(value, defaultValue), $"{record.GetType().Name}.{property.Name} is left at its default <{defaultValue}>.");
            }
        }

        // the obsolete members are older views of the same bytes, such as ImbalanceRecordDbn.AuctionTimeUtc
        private static PropertyInfo[] Properties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetIndexParameters().Length == 0 && property.GetCustomAttribute<ObsoleteAttribute>() == null)
                .ToArray();
        }

        private static bool IsLeaf(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(decimal);
        }
    }
}
