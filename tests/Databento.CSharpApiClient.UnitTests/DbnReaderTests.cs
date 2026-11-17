using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Databento.CSharpApiClient.DataModel.Dbn;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// <see cref="DbnReader"/> streams the records of DBN data the caller already has, such as a downloaded batch file,
    /// with each record's symbol taken from the data's own metadata.
    /// </summary>
    [TestClass]
    public class DbnReaderTests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2022, 5, 16, 13, 30, 0, TimeSpan.Zero);
        private static readonly DateOnly Day = DateOnly.FromDateTime(Start.UtcDateTime);

        [TestMethod]
        public async Task ReadTcbboAsync_FileOnDisk_YieldsItsRecordsWithTheirSymbols()
        {
            byte[] bytes = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800), Tcbbo(801)),
                Mapping("SPXW  261016C06600000", "800"),
                Mapping("SPXW  261016P06600000", "801"));
            string path = Path.Combine(Path.GetTempPath(), "dbn-reader-test-" + Guid.NewGuid().ToString("N") + ".dbn");
            File.WriteAllBytes(path, bytes);
            try
            {
                List<TcbboRecordDbn> records = new List<TcbboRecordDbn>();
                using(FileStream file = File.OpenRead(path))
                {
                    await foreach(TcbboRecordDbn record in DbnReader.ReadTcbboAsync(file))
                    {
                        records.Add(record);
                    }
                }

                CollectionAssert.AreEqual(
                    new[] { "SPXW  261016C06600000", "SPXW  261016P06600000" },
                    records.Select(record => record.Symbol).ToArray());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void ReadTcbbo_Sync_YieldsTheRecords()
        {
            using MemoryStream stream = new MemoryStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800), Tcbbo(800)));

            Assert.AreEqual(2, DbnReader.ReadTcbbo(stream).Count());
        }

        [TestMethod]
        public async Task ReadTcbboAsync_LeavesTheCallersStreamOpen()
        {
            FakeContentStream stream = new FakeContentStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800)));

            await foreach(TcbboRecordDbn record in DbnReader.ReadTcbboAsync(stream))
            {
            }

            Assert.IsFalse(stream.IsDisposed);
        }

        [TestMethod]
        public async Task ReadTradesAsync_DataOfAnotherSchema_ThrowsInvalidData()
        {
            using MemoryStream stream = new MemoryStream(DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800)));

            await Assert.ThrowsExceptionAsync<InvalidDataException>(async () =>
            {
                await foreach(TradeRecordDbn record in DbnReader.ReadTradesAsync(stream))
                {
                }
            });
        }

        [TestMethod]
        public async Task ReadOhlcvAsync_EveryBarInterval_IsRead()
        {
            // one reader serves every ohlcv schema, as one decoder serves the client's five ohlcv methods
            foreach(byte rtype in new byte[] { 0x20, 0x21, 0x22, 0x23 })
            {
                using MemoryStream stream = new MemoryStream(DbnBinaryBuilder.BuildOhlcvStream(rtype, new OhlcvSeed { InstrumentId = 800, TsEvent = Start, Open = 1, High = 2, Low = 0.5, Close = 1.5, Volume = 10 }));
                int count = 0;
                await foreach(OhlcvRecordDbn record in DbnReader.ReadOhlcvAsync(stream))
                {
                    ++count;
                }

                Assert.AreEqual(1, count, rtype.ToString("X2"));
            }
        }

        [TestMethod]
        public void ReadTcbbo_NotDbnData_ThrowsInvalidDataBeforeReadingFurther()
        {
            // a zstd frame starts 28 B5 2F FD; read as DBN, its bytes 4-7 would claim a metadata block of up to 2 GB
            byte[] zstd = new byte[] { 0x28, 0xB5, 0x2F, 0xFD, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00 };

            InvalidDataException exception = Assert.ThrowsException<InvalidDataException>(() => DbnReader.ReadTcbbo(new MemoryStream(zstd)).ToList());
            StringAssert.Contains(exception.Message, "does not start with");
        }

        [TestMethod]
        [DataRow(0, DisplayName = "empty")]
        [DataRow(60, DisplayName = "cut inside the metadata")]
        [DataRow(128 + 5, DisplayName = "cut inside a record header")]
        [DataRow(128 + 40, DisplayName = "cut inside a record body")]
        public void ReadTcbbo_DataCutShort_ThrowsInvalidData(int keptBytes)
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800));

            Assert.ThrowsException<InvalidDataException>(() => DbnReader.ReadTcbbo(new MemoryStream(bytes, 0, keptBytes)).ToList());
        }

        [TestMethod]
        public void ReadTcbbo_RecordLengthBelowAHeader_ThrowsInvalidData()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800));
            bytes[128] = 3; // 12 bytes, less than the 16-byte header

            Assert.ThrowsException<InvalidDataException>(() => DbnReader.ReadTcbbo(new MemoryStream(bytes)).ToList());
        }

        [TestMethod]
        public void ReadTcbbo_MetadataOverrunningItsLength_ThrowsInvalidData()
        {
            byte[] bytes = DbnBinaryBuilder.BuildTcbboStream(Tcbbo(800));
            BitConverter.GetBytes(60u).CopyTo(bytes, 4); // the block really is 120 bytes

            Assert.ThrowsException<InvalidDataException>(() => DbnReader.ReadTcbbo(new MemoryStream(bytes)).ToList());
        }

        [TestMethod]
        public async Task ReadOhlcvAsync_TakesTheSymbolOnTheBarsEventDate()
        {
            byte[] bytes = DbnBinaryBuilder.WithSymbolMappings(
                DbnBinaryBuilder.BuildOhlcvStream(0x21, new OhlcvSeed { InstrumentId = 800, TsEvent = Start.AddDays(1), Open = 1, High = 2, Low = 0.5, Close = 1.5, Volume = 10 }),
                Mapping("QQQ", "800"),
                new SymbolMappingSeed
                {
                    RawSymbol = "SPY",
                    Intervals = new[]
                    {
                        new SymbolMappingIntervalSeed
                        {
                            StartDate = Day.AddDays(1),
                            EndDate = Day.AddDays(2),
                            Symbol = "800",
                        },
                    },
                });
            using MemoryStream stream = new MemoryStream(bytes);

            List<OhlcvRecordDbn> records = new List<OhlcvRecordDbn>();
            await foreach(OhlcvRecordDbn record in DbnReader.ReadOhlcvAsync(stream))
            {
                records.Add(record);
            }

            Assert.AreEqual("SPY", records.Single().Symbol);
        }

        [TestMethod]
        public void ReadTcbbo_NullStream_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => DbnReader.ReadTcbbo(null).ToList());
        }

        private static TcbboSeed Tcbbo(uint instrumentId)
        {
            return new TcbboSeed
            {
                InstrumentId = instrumentId,
                TsEvent = Start,
                TsReceived = Start,
                Price = 4.05,
                Size = 1,
            };
        }

        private static SymbolMappingSeed Mapping(string rawSymbol, string instrumentId)
        {
            return new SymbolMappingSeed
            {
                RawSymbol = rawSymbol,
                Intervals = new[]
                {
                    new SymbolMappingIntervalSeed
                    {
                        StartDate = Day,
                        EndDate = Day.AddDays(1),
                        Symbol = instrumentId,
                    },
                },
            };
        }
    }
}
