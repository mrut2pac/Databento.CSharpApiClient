// Ignore Spelling: Cbbo

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.Caching.Cache;
using Databento.CSharpApiClient.Caching.UnitTests.Archive;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.Caching.UnitTests
{
    /// <summary>
    /// A cache root with its metadata, in a folder of its own under the temporary folder, for the compactor tests: saves day files as the
    /// cache does, writes archive files, and reads back what the archive serves for a symbol-day. The cache's OPRA 1-minute CBBO folder
    /// of October 2026 is the one most tests use.
    /// </summary>
    internal sealed class CacheFixture : IDisposable
    {
        public const string Opra = "OPRA.PILLAR";

        public const string Cbbo = "cbbo-1m";

        public CacheFixture()
        {
            this.Folder = Path.Combine(Path.GetTempPath(), "CachingUnitTests_" + Guid.NewGuid().ToString("N"));
            this.Root = Path.Combine(this.Folder, "Cache");
            this.DryRunFolder = Path.Combine(this.Folder, "DryRun");
            CacheMetadata.EnsureCreated(this.Root);
        }

        /// <summary>
        /// Gets the folder that holds the cache root and the dry run folder, and nothing else.
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// Gets the cache root.
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// Gets the folder a dry run writes into, beside the cache root; not created.
        /// </summary>
        public string DryRunFolder { get; }

        /// <summary>
        /// Gets the OPRA 1-minute CBBO folder of October 2026.
        /// </summary>
        public string CbboDirectory => this.MonthDirectory(Opra, Cbbo, 2026, 10);

        /// <summary>
        /// Creates a CBBO record of a day, a minute after 13:30 UTC per <paramref name="minute"/>, quoted a half point around its price
        /// </summary>
        public static CbboRecordJson CbboRecord(DateTime date, double price, int minute)
        {
            DateTime time = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc).AddHours(13.5).AddMinutes(minute);
            return new CbboRecordJson
            {
                Header = new RecordHeaderJson
                {
                    RecordType = RType.Cbbo1M,
                    PublisherId = 30,
                    InstrumentId = 1234567,
                    TsEventUtc = time,
                },
                Price = price,
                Size = minute + 1,
                Side = TradeAggressor.None,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = time.AddSeconds(1),
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = price,
                        AskPrice = price + 0.5,
                        BidSize = 10,
                        AskSize = 20,
                    },
                },
            };
        }

        /// <summary>
        /// Gets a fully populated CBBO symbol-day of a symbol on October 16, as an archive file holds it
        /// </summary>
        public static ArchiveSymbolDay<ICbboRecord> CbboDay(string rawSymbol, int records)
        {
            return new ArchiveSymbolDay<ICbboRecord>(rawSymbol, new DateTime(2026, 10, 16), CreateCbboRecords(records));
        }

        /// <summary>
        /// Gets fully populated CBBO records, each different from the next
        /// </summary>
        public static CbboRecordDbn[] CreateCbboRecords(int count)
        {
            return Enumerable.Range(1, count).Select(ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>).ToArray();
        }

        public ArchiveCompactorOptions CreateOptions(TimeProvider clock)
        {
            return new ArchiveCompactorOptions
            {
                CacheRoot = this.Root,
                CompactionMode = CompactionMode.Delta,
                DryRunFolder = this.DryRunFolder,
                RetireAttempts = 2,
                RetireRetryDelay = TimeSpan.Zero,
                TimeProvider = clock,
            };
        }

        public string MonthDirectory(string dataset, string schema, int year, int month)
        {
            return DayFileName.GetMonthDirectory(this.Root, dataset, schema, year, month);
        }

        /// <summary>
        /// Saves a day file as the cache saves a download, replacing any there
        /// </summary>
        /// <returns>The day file's path</returns>
        public string SaveDayFile<TJson>(string dataset, string schema, string rawSymbol, DateTime date, params TJson[] records)
        {
            string path = Path.Combine(this.MonthDirectory(dataset, schema, date.Year, date.Month), DayFileName.Format(rawSymbol, date, CacheMetadata.UtcCutoff));
            Assert.IsTrue(DayFile.WriteAsync(path, records, overwrite: true, CancellationToken.None).GetAwaiter().GetResult());

            return path;
        }

        /// <summary>
        /// Saves a CBBO day file of one record a minute at the prices given; none for an empty day
        /// </summary>
        /// <returns>The day file's path</returns>
        public string SaveCbbo(string rawSymbol, DateTime date, params double[] prices)
        {
            return this.SaveDayFile(Opra, Cbbo, rawSymbol, date, prices.Select((price, minute) => CbboRecord(date, price, minute)).ToArray());
        }

        /// <summary>
        /// Saves a CBBO day file of one record at 6600.5, changed by <paramref name="change"/>
        /// </summary>
        public string SaveChangedCbbo(string rawSymbol, DateTime date, Action<CbboRecordJson> change)
        {
            CbboRecordJson record = CbboRecord(date, 6600.5, 0);
            change(record);

            return this.SaveDayFile(Opra, Cbbo, rawSymbol, date, record);
        }

        /// <summary>
        /// Gets the bytes of a day file that holds no JSON array, by what it holds instead: "an empty file", "an empty gzip payload",
        /// "whitespace only" or "JSON lines" (a CBBO record on a line of its own)
        /// </summary>
        public static byte[] NoArrayDayFile(string holding)
        {
            return holding switch
            {
                "an empty file" => Array.Empty<byte>(),
                "an empty gzip payload" => Gzip(string.Empty),
                "whitespace only" => Gzip(" \r\n\t \n"),
                "text before an array" => Gzip("x[]"),
                "JSON lines" => Gzip(JsonSerializer.Serialize(CbboRecord(new DateTime(2026, 10, 16), 6600.5, 0)) + "\n"),
                _ => throw new ArgumentOutOfRangeException(nameof(holding), holding, "No such day file."),
            };
        }

        /// <summary>
        /// Writes a CBBO day file that is no gzip
        /// </summary>
        public string WriteUnreadableCbbo(string rawSymbol, DateTime date)
        {
            return this.WriteCbboBytes(rawSymbol, date, new byte[] { 1, 2, 3 });
        }

        /// <summary>
        /// Writes a CBBO day file of the bytes given, such as one that holds no JSON array
        /// </summary>
        /// <returns>The day file's path</returns>
        public string WriteCbboBytes(string rawSymbol, DateTime date, byte[] bytes)
        {
            Directory.CreateDirectory(this.CbboDirectory);
            string path = Path.Combine(this.CbboDirectory, DayFileName.Format(rawSymbol, date, CacheMetadata.UtcCutoff));
            File.WriteAllBytes(path, bytes);

            return path;
        }

        /// <summary>
        /// Writes a full file (generation 0) or delta of the SPXW options' CBBO archive of October 2026
        /// </summary>
        /// <returns>The file's path</returns>
        public string WriteCbboFile(DateTime builtUtc, int generation, params ArchiveSymbolDay<ICbboRecord>[] symbolDays)
        {
            return ArchiveTestRecords.WriteFile(new CbboArchiveSchema(RType.Cbbo1M), this.CbboDirectory, new ArchiveFileIdentity(Opra, Cbbo, "SPXW.OPT", 2026, 10, "UTC", builtUtc, generation), symbolDays);
        }

        /// <summary>
        /// Reads what the archive serves for a symbol-day: its current version's delta when that lists the day, else its full file
        /// </summary>
        /// <returns>The day's records, or <c>null</c> when the archive does not hold the day</returns>
        public async Task<TRecord[]> ReadServedAsync<TSource, TRecord>(ArchiveRecordSchema<TSource, TRecord> schema, string dataset, string schemaName, string rawSymbol, DateTime date)
            where TSource : class, IRecord
            where TRecord : class, TSource
        {
            string directory = this.MonthDirectory(dataset, schemaName, date.Year, date.Month);
            string[] files = Directory.Exists(directory) ? Directory.GetFiles(directory) : Array.Empty<string>();
            ArchiveVersion version = ArchiveFileName.FindCurrentVersion(files, ArchiveFileName.GetSearchPrefix(dataset, schemaName, ArchiveGroup.Of(dataset, rawSymbol), date.Year, date.Month, CacheMetadata.UtcCutoff));
            if(version == null)
            {
                return null;
            }

            if(version.DeltaPath != null)
            {
                await using(ArchiveFileReader<TSource, TRecord> delta = await ArchiveFile.OpenAsync(version.DeltaPath, schema))
                {
                    if(delta.Index.TryLocate(rawSymbol, date.Day, out _))
                    {
                        return await delta.ReadSymbolDayAsync(rawSymbol, date);
                    }
                }
            }

            await using(ArchiveFileReader<TSource, TRecord> full = await ArchiveFile.OpenAsync(version.FullPath, schema))
            {
                return await full.ReadSymbolDayAsync(rawSymbol, date);
            }
        }

        /// <summary>
        /// Reads what the archive serves for a symbol-day of the OPRA 1-minute CBBO
        /// </summary>
        public Task<CbboRecordDbn[]> ReadServedCbboAsync(string rawSymbol, DateTime date)
        {
            return this.ReadServedAsync(new CbboArchiveSchema(RType.Cbbo1M), Opra, Cbbo, rawSymbol, date);
        }

        /// <summary>
        /// Gets every file under the cache root, in ordinal order
        /// </summary>
        public string[] CacheFiles()
        {
            return Directory.GetFiles(this.Root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        }

        /// <summary>
        /// Gets the names of the archive files in a folder, in ordinal order; none when the folder is missing
        /// </summary>
        public string[] ArchiveFileNames(string directory)
        {
            return Directory.Exists(directory)
                ? Directory.GetFiles(directory, "*" + ArchiveFileName.Extension).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray()
                : Array.Empty<string>();
        }

        /// <summary>
        /// Gets the names of the archive files in the OPRA 1-minute CBBO folder of October 2026
        /// </summary>
        public string[] CbboArchiveFileNames()
        {
            return this.ArchiveFileNames(this.CbboDirectory);
        }

        /// <summary>
        /// Gets the paths of the day files in a folder; none when the folder is missing
        /// </summary>
        public string[] DayFiles(string directory)
        {
            return Directory.Exists(directory) ? Directory.GetFiles(directory, "*" + DayFileName.Extension) : Array.Empty<string>();
        }

        public void Dispose()
        {
            if(Directory.Exists(this.Folder))
            {
                Directory.Delete(this.Folder, recursive: true);
            }
        }

        private static byte[] Gzip(string text)
        {
            using(MemoryStream compressed = new MemoryStream())
            {
                using(GZipStream gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    gzip.Write(bytes, 0, bytes.Length);
                }

                return compressed.ToArray();
            }
        }
    }
}
