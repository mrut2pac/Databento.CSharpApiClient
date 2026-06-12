using System.Text.Json;

using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <c>NanoPriceConverter</c> via the public JSON model types —
    /// read formats (pretty number, nano string, null) and the serialize/deserialize round-trip.
    /// </summary>
    [TestClass]
    public class NanoPriceConverterTests
    {
        [TestMethod]
        public void Read_PrettyPriceNumber_ReturnsDisplayScaledValue()
        {
            string json = "{\"bid_px\":4.01,\"ask_px\":4.05,\"bid_sz\":10,\"ask_sz\":12,\"bid_pb\":1,\"ask_pb\":2}";

            CbboLevel1RecordJson level = JsonSerializer.Deserialize<CbboLevel1RecordJson>(json);

            Assert.AreEqual(4.01, level.BidPrice);
            Assert.AreEqual(4.05, level.AskPrice);
        }

        [TestMethod]
        public void Read_NanoIntegerString_ReturnsDisplayScaledValue()
        {
            string json = "{\"bid_px\":\"4010000000\",\"ask_px\":\"4050000000\",\"bid_sz\":10,\"ask_sz\":12,\"bid_pb\":1,\"ask_pb\":2}";

            CbboLevel1RecordJson level = JsonSerializer.Deserialize<CbboLevel1RecordJson>(json);

            Assert.AreEqual(4.01, level.BidPrice);
            Assert.AreEqual(4.05, level.AskPrice);
        }

        [TestMethod]
        public void Read_Null_ReturnsNaN()
        {
            string json = "{\"bid_px\":null,\"ask_px\":4.05,\"bid_sz\":10,\"ask_sz\":12,\"bid_pb\":1,\"ask_pb\":2}";

            CbboLevel1RecordJson level = JsonSerializer.Deserialize<CbboLevel1RecordJson>(json);

            Assert.IsTrue(double.IsNaN(level.BidPrice));
            Assert.AreEqual(4.05, level.AskPrice);
        }

        [TestMethod]
        public void RoundTrip_OhlcvRecord_PreservesDisplayScaledPrices()
        {
            OhlcvRecordJson original = new OhlcvRecordJson
            {
                Header = new RecordHeaderJson { PublisherId = 2, InstrumentId = 12345 },
                Open = 451.23,
                High = 452.50,
                Low = 450.75,
                Close = 452.01,
                Volume = 987654
            };

            string json = JsonSerializer.Serialize(original);
            OhlcvRecordJson roundTripped = JsonSerializer.Deserialize<OhlcvRecordJson>(json);

            Assert.AreEqual(original.Open, roundTripped.Open);
            Assert.AreEqual(original.High, roundTripped.High);
            Assert.AreEqual(original.Low, roundTripped.Low);
            Assert.AreEqual(original.Close, roundTripped.Close);
            Assert.AreEqual(original.Volume, roundTripped.Volume);
        }

        [TestMethod]
        public void RoundTrip_NaNPrice_SerializesAsNullAndReadsBackAsNaN()
        {
            CbboLevel1RecordJson original = new CbboLevel1RecordJson
            {
                BidPrice = double.NaN,
                AskPrice = 4.05,
                BidSize = 10,
                AskSize = 12
            };

            string json = JsonSerializer.Serialize(original);
            StringAssert.Contains(json, "\"bid_px\":null");

            CbboLevel1RecordJson roundTripped = JsonSerializer.Deserialize<CbboLevel1RecordJson>(json);

            Assert.IsTrue(double.IsNaN(roundTripped.BidPrice));
            Assert.AreEqual(4.05, roundTripped.AskPrice);
        }
    }
}
