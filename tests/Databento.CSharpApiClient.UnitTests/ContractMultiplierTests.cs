using System.Text.Json;

using Databento.CSharpApiClient.DataModel.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="DefinitionRecordJson.ContractMultiplier"/>: an integer field whose
    /// "undefined" value, the largest 32-bit integer, has to read as <see cref="double.NaN"/>.
    /// </summary>
    [TestClass]
    public class ContractMultiplierTests
    {
        [TestMethod]
        public void Read_UndefinedSentinel_ReturnsNaN()
        {
            // what GLBX.MDP3 and OPRA.PILLAR send live when the venue doesn't set it
            DefinitionRecordJson record = JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":2147483647}");

            Assert.IsTrue(double.IsNaN(record.ContractMultiplier));
        }

        [TestMethod]
        public void Read_DefinedNumber_ReturnsItUnscaled()
        {
            DefinitionRecordJson record = JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":100}");

            Assert.AreEqual(100.0, record.ContractMultiplier);
        }

        [TestMethod]
        public void Read_NumberAsString_ReturnsItUnscaled()
        {
            // an integer, never a fixed-point price: a string form must not be read as nanos
            DefinitionRecordJson record = JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":\"100\"}");

            Assert.AreEqual(100.0, record.ContractMultiplier);
        }

        [TestMethod]
        public void Read_UndefinedSentinelAsString_ReturnsNaN()
        {
            DefinitionRecordJson record = JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":\"2147483647\"}");

            Assert.IsTrue(double.IsNaN(record.ContractMultiplier));
        }

        [TestMethod]
        public void Read_ZeroAndNegative_ReturnThemAsIs()
        {
            Assert.AreEqual(0.0, JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":0}").ContractMultiplier);
            Assert.AreEqual(-5.0, JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":-5}").ContractMultiplier);
        }

        [TestMethod]
        public void Read_NotA32BitInteger_Throws()
        {
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":100.5}"));
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":3000000000}"));
        }

        [TestMethod]
        public void Write_ValueA32BitIntegerCannotHold_Throws()
        {
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Serialize(new DefinitionRecordJson { ContractMultiplier = 100.5 }));
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Serialize(new DefinitionRecordJson { ContractMultiplier = double.PositiveInfinity }));
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Serialize(new DefinitionRecordJson { ContractMultiplier = int.MaxValue }));
        }

        [TestMethod]
        public void Read_Null_ReturnsNaN()
        {
            DefinitionRecordJson record = JsonSerializer.Deserialize<DefinitionRecordJson>("{\"contract_multiplier\":null}");

            Assert.IsTrue(double.IsNaN(record.ContractMultiplier));
        }

        [TestMethod]
        public void RoundTrip_UndefinedAndDefined_ReadBackTheSame()
        {
            string undefined = JsonSerializer.Serialize(new DefinitionRecordJson { ContractMultiplier = double.NaN });
            string defined = JsonSerializer.Serialize(new DefinitionRecordJson { ContractMultiplier = 100 });

            StringAssert.Contains(undefined, "\"contract_multiplier\":2147483647");
            Assert.IsTrue(double.IsNaN(JsonSerializer.Deserialize<DefinitionRecordJson>(undefined).ContractMultiplier));
            Assert.AreEqual(100.0, JsonSerializer.Deserialize<DefinitionRecordJson>(defined).ContractMultiplier);
        }
    }
}
