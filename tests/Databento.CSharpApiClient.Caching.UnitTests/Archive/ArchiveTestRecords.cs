using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive
{
    /// <summary>
    /// Builds Databento records for the archive tests and compares them the way the archive promises to keep them: every value of the
    /// record interface. Every property of a record is filled by reflection, so a field a client upgrade adds to the interface and the
    /// archive does not store fails the round trip instead of being dropped.
    /// </summary>
    internal static class ArchiveTestRecords
    {
        public static readonly DateTime BuiltUtc = new DateTime(2026, 11, 18, 2, 15, 0, DateTimeKind.Utc);

        public static readonly ArchiveFileIdentity Identity = new ArchiveFileIdentity("OPRA.PILLAR", "cbbo-1m", "SPXW.OPT", 2026, 10, "UTC", BuiltUtc);

        private static readonly DateTime BaseTimeUtc = new DateTime(2026, 10, 16, 13, 30, 0, DateTimeKind.Utc);

        /// <summary>
        /// A record type each record class's schema holds, since the writer refuses any other
        /// </summary>
        private static readonly Dictionary<Type, RType> RecordTypes = new Dictionary<Type, RType>
        {
            [typeof(BboRecordDbn)] = RType.Bbo1S,
            [typeof(CbboRecordDbn)] = RType.Cbbo1M,
            [typeof(TcbboRecordDbn)] = RType.Tcbbo,
            [typeof(TradeRecordDbn)] = RType.Mbp0,
            [typeof(OhlcvRecordDbn)] = RType.Ohlcv1M,
            [typeof(StatisticsRecordDbn)] = RType.Statistics,
        };

        /// <summary>
        /// Creates a record with every property set to a value of its own, its symbol left unset
        /// </summary>
        public static T CreateFullyPopulated<T>(int seed) where T : new()
        {
            T record = new T();
            Populate(record, seed);
            typeof(T).GetProperty(nameof(IRecord.RecordType)).SetValue(record, RecordTypes[typeof(T)]);

            return record;
        }

        /// <summary>
        /// Asserts the records read back carry every interface value of the records written, and the symbol-day's symbol
        /// </summary>
        public static void AssertSameValues<TSource>(TSource[] expected, TSource[] actual, string rawSymbol) where TSource : IRecord
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Length);

            PropertyInfo[] properties = typeof(TSource).GetProperties().Concat(typeof(TSource).GetInterfaces().SelectMany(type => type.GetProperties())).ToArray();
            for(int i = 0; i < expected.Length; ++i)
            {
                foreach(PropertyInfo property in properties)
                {
                    if(property.Name == nameof(IRecord.Symbol))
                    {
                        Assert.AreEqual(rawSymbol, property.GetValue(actual[i]), Invariant($"records[{i}].Symbol"));
                        continue;
                    }

                    Assert.AreEqual(property.GetValue(expected[i]), property.GetValue(actual[i]), Invariant($"records[{i}].{property.Name}"));
                }
            }
        }

        /// <summary>
        /// A raw symbol in the dataset's own symbology of each schema's file, since the file's group is taken from it
        /// </summary>
        public static string RawSymbolOf<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            return TargetOf(schema).RawSymbol;
        }

        /// <summary>
        /// Writes an archive file of the symbol-days into a folder under its version's name
        /// </summary>
        public static string WriteFile<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema, string directory, ArchiveFileIdentity identity, params ArchiveSymbolDay<TSource>[] symbolDays)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, identity.FileName);
            using(FileStream stream = File.Create(path))
            {
                ArchiveFile.WriteAsync(stream, schema, identity, symbolDays).GetAwaiter().GetResult();
            }

            return path;
        }

        /// <summary>
        /// Writes the records as one symbol-day of an archive file in memory and reads them back
        /// </summary>
        public static async Task<TRecord[]> RoundTripAsync<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema, TSource[] records)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            SchemaTarget target = TargetOf(schema);
            ArchiveFileIdentity identity = new ArchiveFileIdentity(target.Dataset, target.Schema, target.Group, 2026, 10, "UTC", BuiltUtc);

            using(MemoryStream stream = new MemoryStream())
            {
                await ArchiveFile.WriteAsync(stream, schema, identity, new[] { new ArchiveSymbolDay<TSource>(target.RawSymbol, new DateTime(2026, 10, 16), records) });

                stream.Position = 0;
                await using(ArchiveFileReader<TSource, TRecord> reader = await ArchiveFile.OpenAsync(stream, schema))
                {
                    return await reader.ReadSymbolDayAsync(target.RawSymbol, new DateTime(2026, 10, 16));
                }
            }
        }

        private static SchemaTarget TargetOf<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            return schema switch
            {
                BboArchiveSchema => new SchemaTarget("GLBX.MDP3", "bbo-1s", "MNQZ6", "MNQ.FUT"),
                OhlcvArchiveSchema => new SchemaTarget("XNAS.ITCH", "ohlcv-1m", "QQQ", "QQQ"),
                TradeArchiveSchema => new SchemaTarget("DBIX.CORE", "trades", "SPX", "SPX"),
                TcbboArchiveSchema => new SchemaTarget("OPRA.PILLAR", "tcbbo", "SPXW  261016C06600000", "SPXW.OPT"),
                StatisticsArchiveSchema => new SchemaTarget("OPRA.PILLAR", "statistics", "SPXW  261016C06600000", "SPXW.OPT"),
                _ => new SchemaTarget("OPRA.PILLAR", "cbbo-1m", "SPXW  261016C06600000", "SPXW.OPT"),
            };
        }

        private static void Populate(object target, int seed)
        {
            int ordinal = 0;

            // an obsolete property is one the vendor never fills, always 0 in real data, and the archive does not store it
            foreach(PropertyInfo property in target.GetType().GetProperties().Where(p => p.CanWrite && p.Name != nameof(IRecord.Symbol) && p.GetCustomAttribute<ObsoleteAttribute>() == null))
            {
                ++ordinal;
                property.SetValue(target, CreateValue(property.PropertyType, (seed * 100) + ordinal));
            }
        }

        private static object CreateValue(Type type, int value)
        {
            Type underlying = Nullable.GetUnderlyingType(type) ?? type;

            if(underlying.IsEnum)
            {
                // a defined value other than the default
                Array values = Enum.GetValues(underlying);
                return values.GetValue(1 + (value % (values.Length - 1)));
            }

            if(underlying == typeof(DateTime))
            {
                // odd ticks, so the 100 ns digit is exercised
                return BaseTimeUtc.AddTicks((value * 10_000_019L) + 7);
            }

            if(underlying == typeof(double))
            {
                return (value * 1.25) + 0.05;
            }

            if(underlying.IsClass)
            {
                object nested = Activator.CreateInstance(underlying);
                Populate(nested, value);

                return nested;
            }

            // the ends of the type's range, so a column narrower than the property changes the value; one short of the end, since the
            // largest value of a quantity is its undefined value
            decimal offset = 1 + (value % 250);
            decimal number = value % 2 == 0
                ? Convert.ToDecimal(underlying.GetField("MaxValue").GetValue(null), CultureInfo.InvariantCulture) - offset
                : Convert.ToDecimal(underlying.GetField("MinValue").GetValue(null), CultureInfo.InvariantCulture) + offset;

            return Convert.ChangeType(number, underlying, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The dataset, schema, raw symbol and group a schema's test file is written under
        /// </summary>
        private sealed record SchemaTarget(string Dataset, string Schema, string RawSymbol, string Group);
    }
}
