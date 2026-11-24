// Ignore Spelling: Cbbo Ohlcv

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Databento.CSharpApiClient.Caching.Archive;
using Databento.CSharpApiClient.Caching.Archive.Schemas;
using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parquet;
using Parquet.Schema;

namespace Databento.CSharpApiClient.Caching.UnitTests.Archive
{
    [TestClass]
    public class ArchiveFileTests
    {
        private const string Call = "SPXW  261016C06600000";

        private const string Put = "SPXW  261016P06600000";

        private static readonly DateTime October15 = new DateTime(2026, 10, 15);

        private static readonly DateTime October16 = new DateTime(2026, 10, 16);

        [TestMethod]
        public async Task ReadSymbolDay_ManyDaysAcrossSmallRowGroups_EachDayComesBackWhole()
        {
            // row groups of 3 rows, so days start mid-group and span several groups
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Put, October16)] = CreateRecords(4, 10),
                [(Call, October16)] = CreateRecords(5, 20),
                [(Call, October15)] = CreateRecords(2, 30),
            };

            using(MemoryStream stream = await WriteAsync(days, rowGroupSize: 3))
            {
                await using(ParquetReader parquetReader = await ParquetReader.CreateAsync(stream, null, leaveStreamOpen: true))
                {
                    Assert.AreEqual(4, parquetReader.RowGroupCount);
                }

                stream.Position = 0;
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    foreach(KeyValuePair<(string Symbol, DateTime Date), CbboRecordDbn[]> day in days)
                    {
                        ArchiveTestRecords.AssertSameValues<ICbboRecord>(day.Value, await reader.ReadSymbolDayAsync(day.Key.Symbol, day.Key.Date), day.Key.Symbol);
                    }

                    Assert.AreEqual(11, reader.Index.RowCount);
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_DaysSharingARowGroupReadInIndexOrder_DecodeItOnce()
        {
            // 100 days of one record each fill 100 rows of the first row group of 4,096
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = Enumerable.Range(0, 100).ToDictionary(strike => (Strike(6600 + strike), October16), strike => CreateRecords(1, strike + 1));

            using(MemoryStream stream = await WriteAsync(days))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    foreach(ArchiveIndexEntry entry in reader.Index.Entries)
                    {
                        ArchiveTestRecords.AssertSameValues<ICbboRecord>(days[(entry.Symbol, October16)], await reader.ReadSymbolDayAsync(entry.Symbol, October16), entry.Symbol);
                    }

                    Assert.AreEqual(1, reader.RowGroupsDecoded);
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_DaysReadInReverseIndexOrder_EachComesBackWhole()
        {
            // row groups of 3 rows, so a day may start in one row group and end in the next
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = Enumerable.Range(0, 10).ToDictionary(strike => (Strike(6600 + strike), October16), strike => CreateRecords(1 + (strike % 3), 10 * (strike + 1)));

            using(MemoryStream stream = await WriteAsync(days, rowGroupSize: 3))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    foreach(ArchiveIndexEntry entry in reader.Index.Entries.Reverse())
                    {
                        ArchiveTestRecords.AssertSameValues<ICbboRecord>(days[(entry.Symbol, October16)], await reader.ReadSymbolDayAsync(entry.Symbol, October16), entry.Symbol);
                    }

                    // and once more in index order, after the reverse pass left the first row group decoded
                    foreach(ArchiveIndexEntry entry in reader.Index.Entries)
                    {
                        ArchiveTestRecords.AssertSameValues<ICbboRecord>(days[(entry.Symbol, October16)], await reader.ReadSymbolDayAsync(entry.Symbol, October16), entry.Symbol);
                    }
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_ConcurrentReadsOfOneReader_EachGetsItsOwnDay()
        {
            // a reader of the cache keeps one reader open per file and serves requests from several threads through it
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Put, October15)] = CreateRecords(7, 10),
                [(Put, October16)] = CreateRecords(4, 20),
                [(Call, October15)] = CreateRecords(6, 30),
                [(Call, October16)] = CreateRecords(5, 40),
            };
            KeyValuePair<(string Symbol, DateTime Date), CbboRecordDbn[]>[] dayList = days.Select(day => new KeyValuePair<(string Symbol, DateTime Date), CbboRecordDbn[]>(day.Key, day.Value)).ToArray();

            using(MemoryStream stream = await WriteAsync(days, rowGroupSize: 3))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    await Task.WhenAll(Enumerable.Range(0, 64).Select(i => Task.Run(async () =>
                    {
                        KeyValuePair<(string Symbol, DateTime Date), CbboRecordDbn[]> day = dayList[i % dayList.Length];
                        ArchiveTestRecords.AssertSameValues<ICbboRecord>(day.Value, await reader.ReadSymbolDayAsync(day.Key.Symbol, day.Key.Date), day.Key.Symbol);
                    })));
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_AfterDispose_Throws()
        {
            using(MemoryStream stream = await WriteAsync(new Dictionary<(string, DateTime), CbboRecordDbn[]> { [(Call, October16)] = CreateRecords(2, 10) }))
            {
                ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema());
                await reader.DisposeAsync();
                await reader.DisposeAsync();

                await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => reader.ReadSymbolDayAsync(Call, October16));
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_IndexPointingAtAnotherDaysRows_Throws()
        {
            // the rows hold the put first, while the index, sorted by symbol, puts the call there
            ArchiveIndex index = new ArchiveIndex(ArchiveTestRecords.Identity, new[]
            {
                new ArchiveIndexEntry(Call, 16, 3),
                new ArchiveIndexEntry(Put, 16, 2),
            });
            DateOnly date = DateOnly.FromDateTime(October16);

            using(MemoryStream stream = await WriteRawAsync(index, new[] { Put, Put, Call, Call, Call }, Enumerable.Repeat(date, 5).ToArray(), CreateRecords(5, 10)))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    await Assert.ThrowsExceptionAsync<InvalidDataException>(() => reader.ReadSymbolDayAsync(Call, October16));
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_IndexPointingAtRowsOfAnotherDate_Throws()
        {
            // the row's date column is checked as well as its symbol
            ArchiveIndex index = new ArchiveIndex(ArchiveTestRecords.Identity, new[] { new ArchiveIndexEntry(Call, 16, 2) });
            DateOnly date = DateOnly.FromDateTime(October15);

            using(MemoryStream stream = await WriteRawAsync(index, new[] { Call, Call }, new[] { date, date }, CreateRecords(2, 10)))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    await Assert.ThrowsExceptionAsync<InvalidDataException>(() => reader.ReadSymbolDayAsync(Call, October16));
                }
            }
        }

        [TestMethod]
        public async Task Open_RowCountDifferentFromTheIndex_IsRefused()
        {
            ArchiveIndex index = new ArchiveIndex(ArchiveTestRecords.Identity, new[] { new ArchiveIndexEntry(Call, 16, 3) });
            DateOnly date = DateOnly.FromDateTime(October16);

            using(MemoryStream stream = await WriteRawAsync(index, new[] { Call, Call }, new[] { date, date }, CreateRecords(2, 10)))
            {
                await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveFile.OpenAsync(stream, Schema()));
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_EmptyDay_ComesBackEmptyNotMissing()
        {
            // an empty day is final: answering "not cached" would make the reader pay the vendor for it again
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, October15)] = Array.Empty<CbboRecordDbn>(),
                [(Call, October16)] = CreateRecords(2, 10),
            };

            using(MemoryStream stream = await WriteAsync(days))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    CbboRecordDbn[] empty = await reader.ReadSymbolDayAsync(Call, October15);

                    Assert.IsNotNull(empty);
                    Assert.AreEqual(0, empty.Length);
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_OnlyEmptyDays_FileStillAnswers()
        {
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, October15)] = Array.Empty<CbboRecordDbn>(),
            };

            using(MemoryStream stream = await WriteAsync(days))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    Assert.AreEqual(0, (await reader.ReadSymbolDayAsync(Call, October15)).Length);
                    Assert.AreEqual(0L, reader.Index.RowCount);
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_DayNotListed_IsNotCached()
        {
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, October16)] = CreateRecords(2, 10),
            };

            using(MemoryStream stream = await WriteAsync(days))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    Assert.IsNull(await reader.ReadSymbolDayAsync(Call, October15));
                    Assert.IsNull(await reader.ReadSymbolDayAsync(Put, October16));
                }
            }
        }

        [TestMethod]
        public async Task ReadSymbolDay_DateOutsideTheFilesMonth_Throws()
        {
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, October16)] = CreateRecords(2, 10),
            };

            using(MemoryStream stream = await WriteAsync(days))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    await Assert.ThrowsExceptionAsync<ArgumentException>(() => reader.ReadSymbolDayAsync(Call, new DateTime(2026, 11, 16)));
                }
            }
        }

        [TestMethod]
        public async Task Write_DayOutsideTheFilesMonth_Throws()
        {
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, new DateTime(2026, 11, 16))] = CreateRecords(2, 10),
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => WriteAsync(days));
        }

        [TestMethod]
        [DataRow("SPX   261016C06600000", DisplayName = "an option of another root")]
        [DataRow("SPXW", DisplayName = "the root itself")]
        [DataRow("$SPXW_261016_C6600", DisplayName = "a symbol of another symbology")]
        public async Task Write_SymbolThatIsNotOfTheFilesGroup_Throws(string rawSymbol)
        {
            // the group comes from Databento's raw symbol, so a symbol of another group is refused rather than filed under a wrong one
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(rawSymbol, October16)] = CreateRecords(2, 10),
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => WriteAsync(days));
        }

        [TestMethod]
        public async Task Write_SymbolDay_RowsCarryTheRawSymbolAndDate()
        {
            using(MemoryStream stream = await WriteAsync(new Dictionary<(string, DateTime), CbboRecordDbn[]> { [(Call, October16)] = CreateRecords(2, 10) }))
            {
                await using(ParquetReader reader = await ParquetReader.CreateAsync(stream, new ParquetOptions { UseDateOnlyTypeForDates = true }))
                {
                    Assert.AreEqual("raw_symbol", reader.Schema.DataFields[0].Name);
                    Assert.AreEqual("date", reader.Schema.DataFields[1].Name);

                    ParquetRowGroupReader rowGroup = reader.OpenRowGroupReader(0);
                    string[] symbols = new string[rowGroup.RowCount];
                    await rowGroup.ReadAsync(Schema().RawSymbolField, symbols);
                    DateOnly[] dates = new DateOnly[rowGroup.RowCount];
                    await rowGroup.ReadAsync<DateOnly>(Schema().DateField, dates);

                    CollectionAssert.AreEqual(new[] { Call, Call }, symbols);
                    CollectionAssert.AreEqual(new[] { new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 16) }, dates);
                }
            }
        }

        [TestMethod]
        public async Task Write_SameSymbolDayTwice_Throws()
        {
            ArchiveSymbolDay<ICbboRecord>[] symbolDays = new[]
            {
                new ArchiveSymbolDay<ICbboRecord>(Call, October16, CreateRecords(2, 10)),
                new ArchiveSymbolDay<ICbboRecord>(Call, October16, CreateRecords(1, 20)),
            };

            using(MemoryStream stream = new MemoryStream())
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => ArchiveFile.WriteAsync(stream, Schema(), ArchiveTestRecords.Identity, symbolDays));
            }
        }

        [TestMethod]
        public async Task WriteInOrder_DaysStreamedInIndexOrder_ReadBackWithTheIndexBuiltAsTheyCame()
        {
            ArchiveSymbolDay<ICbboRecord>[] symbolDays = new[]
            {
                new ArchiveSymbolDay<ICbboRecord>(Call, October15, CreateRecords(3, 10)),
                new ArchiveSymbolDay<ICbboRecord>(Call, October16, Array.Empty<ICbboRecord>()),
                new ArchiveSymbolDay<ICbboRecord>(Put, October16, CreateRecords(2, 20)),
            };

            using(MemoryStream stream = new MemoryStream())
            {
                ArchiveIndex written = await ArchiveFile.WriteInOrderAsync(stream, Schema(), ArchiveTestRecords.Identity, ToAsyncEnumerable(symbolDays), rowGroupSize: 2);

                stream.Position = 0;
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    Assert.AreEqual(written.ToJson(), reader.Index.ToJson(), "the index goes into the footer once the rows are written");
                    foreach(ArchiveSymbolDay<ICbboRecord> symbolDay in symbolDays)
                    {
                        ArchiveTestRecords.AssertSameValues(symbolDay.Records, await reader.ReadSymbolDayAsync(symbolDay.Symbol, symbolDay.Date), symbolDay.Symbol);
                    }
                }
            }
        }

        [TestMethod]
        public async Task WriteInOrder_DayOutOfIndexOrder_Throws()
        {
            ArchiveSymbolDay<ICbboRecord>[] symbolDays = new[]
            {
                new ArchiveSymbolDay<ICbboRecord>(Call, October16, CreateRecords(1, 10)),
                new ArchiveSymbolDay<ICbboRecord>(Call, October15, CreateRecords(1, 20)),
            };

            using(MemoryStream stream = new MemoryStream())
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => ArchiveFile.WriteInOrderAsync(stream, Schema(), ArchiveTestRecords.Identity, ToAsyncEnumerable(symbolDays)));
            }
        }

        [TestMethod]
        public async Task Write_NullSymbolDay_Throws()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => ArchiveFile.WriteAsync(stream, Schema(), ArchiveTestRecords.Identity, new ArchiveSymbolDay<ICbboRecord>[] { null }));
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task Write_RowGroupSizeBelowOne_Throws(int rowGroupSize)
        {
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => WriteAsync(new Dictionary<(string, DateTime), CbboRecordDbn[]> { [(Call, October16)] = CreateRecords(2, 10) }, rowGroupSize));
        }

        [TestMethod]
        public void SymbolDay_NullRecord_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new ArchiveSymbolDay<ICbboRecord>(Call, October16, new CbboRecordDbn[] { CreateRecords(1, 10)[0], null }));
        }

        [TestMethod]
        public void SymbolDay_RecordOfAnotherSymbol_Throws()
        {
            CbboRecordDbn record = CreateRecords(1, 10)[0];
            record.Symbol = Put;

            Assert.ThrowsException<ArgumentException>(() => new ArchiveSymbolDay<ICbboRecord>(Call, October16, new[] { record }));
        }

        [TestMethod]
        public async Task Write_RecordsNamingTheirOwnSymbol_AreAccepted()
        {
            CbboRecordDbn[] records = CreateRecords(2, 10);
            foreach(CbboRecordDbn record in records)
            {
                record.Symbol = Call;
            }

            using(MemoryStream stream = await WriteAsync(new Dictionary<(string, DateTime), CbboRecordDbn[]> { [(Call, October16)] = records }))
            {
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    ArchiveTestRecords.AssertSameValues<ICbboRecord>(records, await reader.ReadSymbolDayAsync(Call, October16), Call);
                }
            }
        }

        [TestMethod]
        public async Task Write_SymbolDays_ReturnsTheIndexItStored()
        {
            Dictionary<(string, DateTime), CbboRecordDbn[]> days = new Dictionary<(string, DateTime), CbboRecordDbn[]>
            {
                [(Call, October16)] = CreateRecords(2, 10),
                [(Call, October15)] = Array.Empty<CbboRecordDbn>(),
            };

            using(MemoryStream stream = new MemoryStream())
            {
                ArchiveIndex written = await ArchiveFile.WriteAsync(stream, Schema(), ArchiveTestRecords.Identity, ToSymbolDays(days));

                stream.Position = 0;
                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(stream, Schema()))
                {
                    Assert.AreEqual(written.ToJson(), reader.Index.ToJson());
                }
            }
        }

        [TestMethod]
        public async Task Open_FileOfAnotherSchema_IsRefused()
        {
            OhlcvRecordDbn[] bars = new[] { ArchiveTestRecords.CreateFullyPopulated<OhlcvRecordDbn>(1) };
            ArchiveFileIdentity identity = new ArchiveFileIdentity("XNAS.ITCH", "ohlcv-1m", "QQQ", 2026, 10, "UTC", ArchiveTestRecords.BuiltUtc);

            using(MemoryStream stream = new MemoryStream())
            {
                await ArchiveFile.WriteAsync(stream, new OhlcvArchiveSchema(RType.Ohlcv1M), identity, new[] { new ArchiveSymbolDay<IOhlcvRecord>("QQQ", October16, bars) });

                stream.Position = 0;
                await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveFile.OpenAsync(stream, Schema()));
            }
        }

        [TestMethod]
        public async Task Open_NotAParquetFile_IsRefused()
        {
            using(MemoryStream stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }))
            {
                await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveFile.OpenAsync(stream, Schema()));
            }
        }

        [TestMethod]
        public async Task Open_ParquetFileWithoutAnIndex_IsRefused()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                DataField<int> field = new DataField<int>("n");
                await using(ParquetWriter writer = await ParquetWriter.CreateAsync(new ParquetSchema(field), stream, null, false))
                {
                    using(ParquetRowGroupWriter rowGroup = writer.CreateRowGroup())
                    {
                        await rowGroup.WriteAsync<int>(field, new int[] { 1 });
                    }
                }

                stream.Position = 0;
                await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveFile.OpenAsync(stream, Schema()));
            }
        }

        [TestMethod]
        public async Task OpenFromPath_FileCanBeDeletedWhileOpen()
        {
            // the compactor retires an old version while a reader of the cache may still be reading it
            string directory = CreateTemporaryDirectory();
            try
            {
                string path = await WriteFileAsync(directory, "OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet");

                await using(ArchiveFileReader<ICbboRecord, CbboRecordDbn> reader = await ArchiveFile.OpenAsync(path, Schema()))
                {
                    File.Delete(path);

                    Assert.AreEqual(2, (await reader.ReadSymbolDayAsync(Call, October16)).Length);
                }
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [TestMethod]
        [DataRow("OPRA.PILLAR_cbbo-1s_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "another schema")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261119T021500.full.parquet", DisplayName = "another build")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPX.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "another group")]
        [DataRow("OPRA.PILLAR_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.delta.1.parquet", DisplayName = "another kind")]
        [DataRow("XNAS.ITCH_cbbo-1m_SPXW.OPT_2026-10.UTC_20261118T021500.full.parquet", DisplayName = "another dataset")]
        public async Task OpenFromPath_NameOfAnotherVersion_IsRefused(string fileName)
        {
            // a file restored or copied under another name must not serve as that name's data, e.g. 1-minute quotes as 1-second ones
            string directory = CreateTemporaryDirectory();
            try
            {
                string path = await WriteFileAsync(directory, fileName);

                await Assert.ThrowsExceptionAsync<InvalidDataException>(() => ArchiveFile.OpenAsync(path, Schema()));

                // an exclusive open fails while any handle the refused open left behind is still open
                new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose();
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        private static CbboArchiveSchema Schema()
        {
            return new CbboArchiveSchema(RType.Cbbo1M);
        }

        private static string Strike(int strike)
        {
            return Invariant($"SPXW  261016C{strike * 1000:00000000}");
        }

        private static string CreateTemporaryDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ArchiveFileTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            return directory;
        }

        private static async Task<string> WriteFileAsync(string directory, string fileName)
        {
            string path = Path.Combine(directory, fileName);
            using(MemoryStream stream = await WriteAsync(new Dictionary<(string, DateTime), CbboRecordDbn[]> { [(Call, October16)] = CreateRecords(2, 10) }))
            {
                File.WriteAllBytes(path, stream.ToArray());
            }

            return path;
        }

        /// <summary>
        /// Writes rows and an index as given, without the writer's checks, to stand in for a damaged file
        /// </summary>
        private static async Task<MemoryStream> WriteRawAsync(ArchiveIndex index, string[] rawSymbols, DateOnly[] dates, CbboRecordDbn[] records)
        {
            CbboArchiveSchema schema = Schema();
            MemoryStream stream = new MemoryStream();
            await using(ParquetWriter writer = await ParquetWriter.CreateAsync(schema.ParquetSchema, stream, null, false))
            {
                writer.CustomMetadata = new Dictionary<string, string>
                {
                    [ArchiveIndex.MetadataKey] = index.ToJson(),
                };
                await ArchiveFile.WriteRowGroupAsync(writer, schema, rawSymbols, dates, records);
            }

            stream.Position = 0;

            return stream;
        }

        private static CbboRecordDbn[] CreateRecords(int count, int seed)
        {
            return Enumerable.Range(seed, count).Select(ArchiveTestRecords.CreateFullyPopulated<CbboRecordDbn>).ToArray();
        }

        private static ArchiveSymbolDay<ICbboRecord>[] ToSymbolDays(Dictionary<(string, DateTime), CbboRecordDbn[]> days)
        {
            return days.Select(day => new ArchiveSymbolDay<ICbboRecord>(day.Key.Item1, day.Key.Item2, day.Value)).ToArray();
        }

        private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach(T item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        private static async Task<MemoryStream> WriteAsync(Dictionary<(string, DateTime), CbboRecordDbn[]> days, int rowGroupSize = ArchiveFile.DefaultRowGroupSize)
        {
            MemoryStream stream = new MemoryStream();
            await ArchiveFile.WriteAsync(stream, Schema(), ArchiveTestRecords.Identity, ToSymbolDays(days), rowGroupSize);
            stream.Position = 0;

            return stream;
        }
    }
}
