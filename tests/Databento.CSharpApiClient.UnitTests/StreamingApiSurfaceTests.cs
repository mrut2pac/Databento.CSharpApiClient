using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Every timeseries method that returns an array has streaming counterparts, so no schema is left array-only, and every
    /// DBN record type the client returns can be read from a stream with <see cref="DbnReader"/>.
    /// </summary>
    [TestClass]
    public class StreamingApiSurfaceTests
    {
        [TestMethod]
        [DataRow(typeof(DatabentoClient))]
        [DataRow(typeof(DatabentoJsonClient))]
        public void EveryTimeseriesGetAsync_HasAsyncAndSyncStreamingCounterparts(Type client)
        {
            List<string> missing = new List<string>();
            foreach(MethodInfo get in TimeseriesGetMethods(client))
            {
                Type record = get.ReturnType.GetGenericArguments()[0].GetElementType();
                Type[] parameters = get.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
                string schema = get.Name.Substring("Get".Length, get.Name.Length - "GetAsync".Length);

                MethodInfo streamAsync = client.GetMethod("Stream" + schema + "Async", parameters);
                if(streamAsync == null || streamAsync.ReturnType != typeof(IAsyncEnumerable<>).MakeGenericType(record))
                {
                    missing.Add("Stream" + schema + "Async(" + string.Join(", ", parameters.Select(type => type.Name)) + ")");
                }

                Type[] syncParameters = parameters.Where(type => type != typeof(CancellationToken)).ToArray();
                MethodInfo stream = client.GetMethod("Stream" + schema, syncParameters);
                if(stream == null || stream.ReturnType != typeof(IEnumerable<>).MakeGenericType(record))
                {
                    missing.Add("Stream" + schema + "(" + string.Join(", ", syncParameters.Select(type => type.Name)) + ")");
                }
            }

            Assert.AreEqual(0, missing.Count, "Missing: " + string.Join("; ", missing));
        }

        [TestMethod]
        public void EveryDbnRecordType_CanBeReadWithDbnReader()
        {
            HashSet<Type> records = TimeseriesGetMethods(typeof(DatabentoClient))
                .Select(get => get.ReturnType.GetGenericArguments()[0].GetElementType())
                .ToHashSet();
            List<string> missing = new List<string>();
            foreach(Type record in records)
            {
                bool readAsync = typeof(DbnReader).GetMethods(BindingFlags.Public | BindingFlags.Static).Any(method =>
                    method.ReturnType == typeof(IAsyncEnumerable<>).MakeGenericType(record)
                    && method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Stream), typeof(CancellationToken) }));
                bool read = typeof(DbnReader).GetMethods(BindingFlags.Public | BindingFlags.Static).Any(method =>
                    method.ReturnType == typeof(IEnumerable<>).MakeGenericType(record)
                    && method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Stream) }));
                if(!readAsync || !read)
                {
                    missing.Add(record.Name);
                }
            }

            Assert.AreEqual(0, missing.Count, "Missing: " + string.Join(", ", missing));
        }

        private static IEnumerable<MethodInfo> TimeseriesGetMethods(Type client)
        {
            return client.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method =>
                method.Name.StartsWith("Get", StringComparison.Ordinal)
                && method.Name.EndsWith("Async", StringComparison.Ordinal)
                && method.GetCustomAttribute<ObsoleteAttribute>() == null
                && method.ReturnType.IsGenericType
                && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                && method.ReturnType.GetGenericArguments()[0].IsArray
                && method.ReturnType.GetGenericArguments()[0].GetElementType().Name.Contains("Record", StringComparison.Ordinal)
                && method.GetParameters().Any(parameter => parameter.Name == "startUtc"));
        }
    }
}
