using System;
using System.Linq;
using System.Text.Json;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Dbn;
using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// The record interfaces read the same values from a JSON record and a DBN record that carry the same data, and read an
    /// undefined price, quantity or timestamp as null whatever the concrete record holds for it.
    /// </summary>
    [TestClass]
    public class RecordInterfaceTests
    {
        private static readonly DateTime TsEvent = new DateTime(2026, 10, 16, 14, 30, 0, DateTimeKind.Utc);
        private static readonly DateTime TsReceived = TsEvent.AddTicks(1234);
        private static readonly DateTime TsRef = TsEvent.Date.AddDays(-1);

        [TestMethod]
        public void Bbo_JsonAndDbn_ReadTheSameValues()
        {
            BboRecordJson json = new BboRecordJson
            {
                Header = Header(RType.Bbo1S),
                Price = 21000.25,
                Size = 3,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                Sequence = 77,
                Levels = new[]
                {
                    new Mbp1LevelJson
                    {
                        BidPrice = 21000.0,
                        AskPrice = 21000.5,
                        BidSize = 10,
                        AskSize = 12,
                        BidCount = 4,
                        AskCount = 5,
                    },
                },
                Symbol = "MNQZ6",
            };
            BboRecordDbn dbn = new BboRecordDbn
            {
                RecordType = RType.Bbo1S,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                Price = 21000.25,
                Size = 3,
                Side = TradeAggressor.Buyer,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                Sequence = 77,
                Level = new Mbp1LevelDbn
                {
                    BidPrice = 21000.0,
                    AskPrice = 21000.5,
                    BidSize = 10,
                    AskSize = 12,
                    BidCount = 4,
                    AskCount = 5,
                },
                Symbol = "MNQZ6",
            };

            foreach(IBboRecord record in new IBboRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Bbo1S, "MNQZ6");
                Assert.AreEqual(21000.25, record.Price);
                Assert.AreEqual(3u, record.Size);
                Assert.AreEqual(TradeAggressor.Buyer, record.Side);
                Assert.AreEqual(MessageInfoBits.Last, record.Flags);
                Assert.AreEqual(TsReceived, record.TsReceivedUtc);
                Assert.AreEqual(77u, record.Sequence);
                Assert.AreEqual(21000.0, record.BidPrice);
                Assert.AreEqual(21000.5, record.AskPrice);
                Assert.AreEqual(10u, record.BidSize);
                Assert.AreEqual(12u, record.AskSize);
                Assert.AreEqual(4u, record.BidCount);
                Assert.AreEqual(5u, record.AskCount);
            }
        }

        [TestMethod]
        public void Bbo_UndefinedPricesAndNoLevel_ReadAsNull()
        {
            BboRecordJson json = new BboRecordJson
            {
                Header = Header(RType.Bbo1S),
                Price = null,
                Levels = new[]
                {
                    new Mbp1LevelJson
                    {
                        BidPrice = double.NaN,
                        AskPrice = double.NaN,
                    },
                },
            };
            BboRecordDbn dbn = new BboRecordDbn
            {
                Price = double.NaN,
                Level = new Mbp1LevelDbn
                {
                    BidPrice = double.NaN,
                    AskPrice = double.NaN,
                },
            };

            foreach(IBboRecord record in new IBboRecord[] { json, dbn })
            {
                Assert.IsNull(record.Price);
                Assert.IsNull(record.BidPrice);
                Assert.IsNull(record.AskPrice);
            }

            IBboRecord withoutLevels = new BboRecordJson { Header = Header(RType.Bbo1S) };
            IBboRecord withoutLevel = new BboRecordDbn();
            foreach(IBboRecord record in new[] { withoutLevels, withoutLevel })
            {
                Assert.IsNull(record.BidPrice);
                Assert.IsNull(record.AskPrice);
                Assert.AreEqual(0u, record.BidSize);
                Assert.AreEqual(0u, record.AskCount);
            }
        }

        [TestMethod]
        public void Cbbo_JsonAndDbn_ReadTheSameValues()
        {
            CbboRecordJson json = new CbboRecordJson
            {
                Header = Header(RType.Cbbo1S),
                Price = 4.05,
                Size = 2,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = 4.0,
                        AskPrice = 4.1,
                        BidSize = 20,
                        AskSize = 30,
                        BidPublisherId = 21,
                        AskPublisherId = 22,
                    },
                },
                Symbol = "SPXW  261016C06600000",
            };
            CbboRecordDbn dbn = new CbboRecordDbn
            {
                RecordType = RType.Cbbo1S,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                Price = 4.05,
                Size = 2,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                BidPrice = 4.0,
                AskPrice = 4.1,
                BidSize = 20,
                AskSize = 30,
                BidPublisherId = 21,
                AskPublisherId = 22,
                Symbol = "SPXW  261016C06600000",
            };

            foreach(ICbboRecord record in new ICbboRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Cbbo1S, "SPXW  261016C06600000");
                Assert.AreEqual(4.05, record.Price);
                Assert.AreEqual(2u, record.Size);
                Assert.AreEqual(TradeAggressor.Seller, record.Side);
                Assert.AreEqual(MessageInfoBits.Last, record.Flags);
                Assert.AreEqual(TsReceived, record.TsReceivedUtc);
                Assert.AreEqual(4.0, record.BidPrice);
                Assert.AreEqual(4.1, record.AskPrice);
                Assert.AreEqual(20u, record.BidSize);
                Assert.AreEqual(30u, record.AskSize);
                Assert.AreEqual((ushort)21, record.BidPublisherId);
                Assert.AreEqual((ushort)22, record.AskPublisherId);
            }
        }

        [TestMethod]
        public void Cbbo_UndefinedPrices_ReadAsNull()
        {
            CbboRecordJson json = new CbboRecordJson
            {
                Header = Header(RType.Cbbo1S),
                Price = null,
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = double.NaN,
                        AskPrice = double.NaN,
                    },
                },
            };
            CbboRecordDbn dbn = new CbboRecordDbn
            {
                Price = double.NaN,
                BidPrice = double.NaN,
                AskPrice = double.NaN,
            };

            foreach(ICbboRecord record in new ICbboRecord[] { json, dbn })
            {
                Assert.IsNull(record.Price);
                Assert.IsNull(record.BidPrice);
                Assert.IsNull(record.AskPrice);
            }
        }

        [TestMethod]
        public void Tcbbo_JsonAndDbn_ReadTheSameValues()
        {
            TcbboRecordJson json = new TcbboRecordJson
            {
                Header = Header(RType.Tcbbo),
                Price = 4.05,
                Size = 2,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                TsInDelta = 5150,
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = 4.0,
                        AskPrice = 4.1,
                        BidSize = 20,
                        AskSize = 30,
                        BidPublisherId = 21,
                        AskPublisherId = 22,
                    },
                },
                Symbol = "SPXW  261016C06600000",
            };
            TcbboRecordDbn dbn = new TcbboRecordDbn
            {
                RecordType = RType.Tcbbo,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                Price = 4.05,
                Size = 2,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.Seller,
                Flags = MessageInfoBits.Last,
                TsReceivedUtc = TsReceived,
                TsInDelta = 5150,
                Level = new CbboLevelDbn
                {
                    BidPrice = 4.0,
                    AskPrice = 4.1,
                    BidSize = 20,
                    AskSize = 30,
                    BidPublisherId = 21,
                    AskPublisherId = 22,
                },
                Symbol = "SPXW  261016C06600000",
            };

            foreach(ITcbboRecord record in new ITcbboRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Tcbbo, "SPXW  261016C06600000");
                Assert.AreEqual(4.05, record.Price);
                Assert.AreEqual(2u, record.Size);
                Assert.AreEqual(OrderBookAction.Trade, record.Action);
                Assert.AreEqual(TradeAggressor.Seller, record.Side);
                Assert.AreEqual(MessageInfoBits.Last, record.Flags);
                Assert.AreEqual(TsReceived, record.TsReceivedUtc);
                Assert.AreEqual(5150, record.TsInDelta);
                Assert.AreEqual(4.0, record.BidPrice);
                Assert.AreEqual(4.1, record.AskPrice);
                Assert.AreEqual(20u, record.BidSize);
                Assert.AreEqual(30u, record.AskSize);
                Assert.AreEqual((ushort)21, record.BidPublisherId);
                Assert.AreEqual((ushort)22, record.AskPublisherId);
            }
        }

        [TestMethod]
        public void Tcbbo_UndefinedLevelPrices_ReadAsNull()
        {
            TcbboRecordJson json = new TcbboRecordJson
            {
                Header = Header(RType.Tcbbo),
                Levels = new[]
                {
                    new CbboLevel1RecordJson
                    {
                        BidPrice = double.NaN,
                        AskPrice = double.NaN,
                    },
                },
            };
            TcbboRecordDbn dbn = new TcbboRecordDbn
            {
                Level = new CbboLevelDbn
                {
                    BidPrice = double.NaN,
                    AskPrice = double.NaN,
                },
            };

            foreach(ITcbboRecord record in new ITcbboRecord[] { json, dbn })
            {
                Assert.IsNull(record.BidPrice);
                Assert.IsNull(record.AskPrice);
            }
        }

        [TestMethod]
        public void Trade_JsonAndDbn_ReadTheSameValues()
        {
            TradeRecordJson json = new TradeRecordJson
            {
                Header = Header(RType.Mbp0),
                Price = 6600.25,
                Size = 0,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.None,
                Flags = MessageInfoBits.Last,
                Depth = 0,
                TsReceivedUtc = TsReceived,
                TsInDelta = 950,
                Sequence = 123456,
                Symbol = "SPX",
            };
            TradeRecordDbn dbn = new TradeRecordDbn
            {
                RecordType = RType.Mbp0,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                Price = 6600.25,
                Size = 0,
                Action = OrderBookAction.Trade,
                Side = TradeAggressor.None,
                Flags = MessageInfoBits.Last,
                Depth = 0,
                TsReceivedUtc = TsReceived,
                TsInDelta = 950,
                Sequence = 123456,
                Symbol = "SPX",
            };

            foreach(ITradeRecord record in new ITradeRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Mbp0, "SPX");
                Assert.AreEqual(6600.25, record.Price);
                Assert.AreEqual(0u, record.Size);
                Assert.AreEqual(OrderBookAction.Trade, record.Action);
                Assert.AreEqual(TradeAggressor.None, record.Side);
                Assert.AreEqual(MessageInfoBits.Last, record.Flags);
                Assert.AreEqual((byte)0, record.Depth);
                Assert.AreEqual(TsReceived, record.TsReceivedUtc);
                Assert.AreEqual(950, record.TsInDelta);
                Assert.AreEqual(123456u, record.Sequence);
            }
        }

        [TestMethod]
        public void Trade_DepthAboveAByte_IsRefused()
        {
            // DBN carries depth in one byte, so a JSON depth past it cannot come from Databento
            ITradeRecord record = new TradeRecordJson
            {
                Header = Header(RType.Mbp0),
                Depth = 256,
            };

            Assert.ThrowsException<OverflowException>(() => record.Depth);
        }

        [TestMethod]
        public void Ohlcv_JsonAndDbn_ReadTheSameValues()
        {
            OhlcvRecordJson json = new OhlcvRecordJson
            {
                Header = Header(RType.Ohlcv1M),
                Open = 600.1,
                High = 601.2,
                Low = 599.3,
                Close = 600.4,
                Volume = 125000,
                Symbol = "QQQ",
            };
            OhlcvRecordDbn dbn = new OhlcvRecordDbn
            {
                RecordType = RType.Ohlcv1M,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                Open = 600.1,
                High = 601.2,
                Low = 599.3,
                Close = 600.4,
                Volume = 125000,
                Symbol = "QQQ",
            };

            foreach(IOhlcvRecord record in new IOhlcvRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Ohlcv1M, "QQQ");
                Assert.AreEqual(600.1, record.Open);
                Assert.AreEqual(601.2, record.High);
                Assert.AreEqual(599.3, record.Low);
                Assert.AreEqual(600.4, record.Close);
                Assert.AreEqual(125000ul, record.Volume);
            }
        }

        [TestMethod]
        public void Ohlcv_NegativeJsonVolume_IsRefused()
        {
            IOhlcvRecord record = new OhlcvRecordJson
            {
                Header = Header(RType.Ohlcv1M),
                Volume = -1,
            };

            Assert.ThrowsException<OverflowException>(() => record.Volume);
        }

        [TestMethod]
        public void Statistics_JsonAndDbn_ReadTheSameValues()
        {
            StatisticsRecordJson json = new StatisticsRecordJson
            {
                Header = Header(RType.Statistics),
                TsReceivedUtc = TsReceived,
                TsRefUtc = TsRef,
                Price = 12.5,
                Quantity = 4200,
                Sequence = 9,
                TsInDelta = 310,
                StatType = 9,
                ChannelId = 3,
                UpdateAction = 1,
                StatFlags = MessageInfoBits.Last,
                Symbol = "SPXW  261016C06600000",
            };
            StatisticsRecordDbn dbn = new StatisticsRecordDbn
            {
                RecordType = RType.Statistics,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
                TsReceivedUtc = TsReceived,
                TsRefUtc = TsRef,
                Price = 12.5,
                Quantity = 4200,
                Sequence = 9,
                TsInDelta = 310,
                StatType = 9,
                ChannelId = 3,
                UpdateAction = 1,
                StatFlags = MessageInfoBits.Last,
                Symbol = "SPXW  261016C06600000",
            };

            foreach(IStatisticsRecord record in new IStatisticsRecord[] { json, dbn })
            {
                AssertHeader(record, RType.Statistics, "SPXW  261016C06600000");
                Assert.AreEqual(TsReceived, record.TsReceivedUtc);
                Assert.AreEqual(TsRef, record.TsRefUtc);
                Assert.AreEqual(12.5, record.Price);
                Assert.AreEqual(4200L, record.Quantity);
                Assert.AreEqual(9u, record.Sequence);
                Assert.AreEqual(310, record.TsInDelta);
                Assert.AreEqual((ushort)9, record.StatType);
                Assert.AreEqual((ushort)3, record.ChannelId);
                Assert.AreEqual((byte)1, record.UpdateAction);
                Assert.AreEqual(MessageInfoBits.Last, record.StatFlags);
            }
        }

        [TestMethod]
        [DataRow(long.MaxValue)]
        [DataRow((long)int.MaxValue)]
        public void Statistics_UndefinedPriceAndQuantity_ReadAsNull(long undefinedQuantity)
        {
            // open interest carries no price, and a price statistic no quantity; DBN v1 sends a 32-bit undefined quantity
            StatisticsRecordJson json = new StatisticsRecordJson
            {
                Header = Header(RType.Statistics),
                Price = double.NaN,
                Quantity = undefinedQuantity,
            };
            StatisticsRecordDbn dbn = new StatisticsRecordDbn
            {
                Price = double.NaN,
                Quantity = undefinedQuantity,
            };

            foreach(IStatisticsRecord record in new IStatisticsRecord[] { json, dbn })
            {
                Assert.IsNull(record.Price);
                Assert.IsNull(record.Quantity);
            }
        }

        [TestMethod]
        public void Header_UndefinedEventTime_ReadsAsNull()
        {
            // DBN decodes an undefined timestamp as DateTime.MinValue, JSON as null
            IRecord[] records = new IRecord[]
            {
                new BboRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new CbboRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new TcbboRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new TradeRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new OhlcvRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new StatisticsRecordJson { Header = new RecordHeaderJson { TsEventUtc = null } },
                new BboRecordDbn { TsEventUtc = DateTime.MinValue },
                new CbboRecordDbn { TsEventUtc = DateTime.MinValue },
                new TcbboRecordDbn { TsEventUtc = DateTime.MinValue },
                new TradeRecordDbn { TsEventUtc = DateTime.MinValue },
                new OhlcvRecordDbn { TsEventUtc = DateTime.MinValue },
                new StatisticsRecordDbn { TsEventUtc = DateTime.MinValue },
            };

            foreach(IRecord record in records)
            {
                Assert.IsNull(record.TsEventUtc, record.GetType().Name);
            }
        }

        [TestMethod]
        public void UndefinedReceiveTime_ReadsAsNull()
        {
            // both encodings decode an undefined ts_recv as DateTime.MinValue
            Assert.IsNull(((IBboRecord)new BboRecordJson()).TsReceivedUtc);
            Assert.IsNull(((ICbboRecord)new CbboRecordJson()).TsReceivedUtc);
            Assert.IsNull(((ITcbboRecord)new TcbboRecordJson()).TsReceivedUtc);
            Assert.IsNull(((ITradeRecord)new TradeRecordJson()).TsReceivedUtc);
            Assert.IsNull(((IStatisticsRecord)new StatisticsRecordJson()).TsReceivedUtc);
            Assert.IsNull(((IBboRecord)new BboRecordDbn()).TsReceivedUtc);
            Assert.IsNull(((ICbboRecord)new CbboRecordDbn()).TsReceivedUtc);
            Assert.IsNull(((ITcbboRecord)new TcbboRecordDbn()).TsReceivedUtc);
            Assert.IsNull(((ITradeRecord)new TradeRecordDbn()).TsReceivedUtc);
            Assert.IsNull(((IStatisticsRecord)new StatisticsRecordDbn()).TsReceivedUtc);
        }

        [TestMethod]
        public void JsonRecord_SerializesOnlyItsOwnProperties()
        {
            // the interface members are explicit, so serializing a JSON record writes what it wrote before them
            string json = JsonSerializer.Serialize(new TradeRecordJson { Header = Header(RType.Mbp0), Symbol = "SPX" });

            using(JsonDocument document = JsonDocument.Parse(json))
            {
                string[] keys = document.RootElement.EnumerateObject().Select(property => property.Name).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { "hd", "price", "size", "action", "side", "flags", "depth", "ts_recv", "ts_in_delta", "sequence", "ts_out", "symbol" },
                    keys);
            }
        }

        [TestMethod]
        public void Header_MissingInJson_ReadsAsDefaults()
        {
            IRecord record = new OhlcvRecordJson();

            Assert.AreEqual(default(RType), record.RecordType);
            Assert.AreEqual((ushort)0, record.PublisherId);
            Assert.AreEqual(0u, record.InstrumentId);
            Assert.IsNull(record.TsEventUtc);
        }

        private static RecordHeaderJson Header(RType recordType)
        {
            return new RecordHeaderJson
            {
                RecordType = recordType,
                PublisherId = 1,
                InstrumentId = 42,
                TsEventUtc = TsEvent,
            };
        }

        private static void AssertHeader(IRecord record, RType recordType, string symbol)
        {
            Assert.AreEqual(recordType, record.RecordType);
            Assert.AreEqual((ushort)1, record.PublisherId);
            Assert.AreEqual(42u, record.InstrumentId);
            Assert.AreEqual(TsEvent, record.TsEventUtc);
            Assert.AreEqual(symbol, record.Symbol);
        }
    }
}
