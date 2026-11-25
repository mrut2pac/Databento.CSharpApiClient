using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Databento.CSharpApiClient.DataModel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for the constants that name Databento's own values: <see cref="Datasets"/> and <see cref="StatTypes"/>.
    /// </summary>
    [TestClass]
    public class ApiConstantsTests
    {
        [TestMethod]
        public void Datasets_Codes_AreExactlyTheListedOnes()
        {
            string[] expected =
            {
                "XNAS.ITCH", "XBOS.ITCH", "XPSX.ITCH", "BATS.PITCH", "BATY.PITCH", "EDGX.PITCH", "EDGA.PITCH", "XNAS.BASIC", "XNYS.PILLAR",
                "ARCX.PILLAR", "XASE.PILLAR", "XCHI.PILLAR", "XCIS.TRADESBBO", "IEXG.TOPS", "MEMX.MEMOIR", "EPRL.DOM", "OCEA.MEMOIR",
                "OPRA.PILLAR", "GLBX.MDP3", "IFUS.IMPACT", "XCBF.PITCH", "IFEU.IMPACT", "NDEX.IMPACT", "IFLL.IMPACT", "XEUR.EOBI", "XEEE.EOBI",
                "DBEQ.BASIC", "DBEQ.PLUS", "DBEQ.MAX", "EQUS.MINI", "EQUS.SUMMARY", "DBIX.CORE",
            };

            CollectionAssert.AreEquivalent(expected, ConstantsOf<string>(typeof(Datasets)));
        }

        [TestMethod]
        public void StatTypes_Values_MatchDatabentosNumbering()
        {
            Dictionary<string, ushort> expected = new Dictionary<string, ushort>
            {
                [nameof(StatTypes.OpeningPrice)] = 1,
                [nameof(StatTypes.IndicativeOpeningPrice)] = 2,
                [nameof(StatTypes.SettlementPrice)] = 3,
                [nameof(StatTypes.TradingSessionLowPrice)] = 4,
                [nameof(StatTypes.TradingSessionHighPrice)] = 5,
                [nameof(StatTypes.ClearedVolume)] = 6,
                [nameof(StatTypes.LowestOffer)] = 7,
                [nameof(StatTypes.HighestBid)] = 8,
                [nameof(StatTypes.OpenInterest)] = 9,
                [nameof(StatTypes.FixingPrice)] = 10,
                [nameof(StatTypes.ClosePrice)] = 11,
                [nameof(StatTypes.NetChange)] = 12,
                [nameof(StatTypes.Vwap)] = 13,
                [nameof(StatTypes.Volatility)] = 14,
                [nameof(StatTypes.Delta)] = 15,
                [nameof(StatTypes.UncrossingPrice)] = 16,
                [nameof(StatTypes.UpperPriceLimit)] = 17,
                [nameof(StatTypes.LowerPriceLimit)] = 18,
                [nameof(StatTypes.BlockVolume)] = 19,
                [nameof(StatTypes.IndicativeClosePrice)] = 20,
                [nameof(StatTypes.MwcbLevel1)] = 21,
                [nameof(StatTypes.MwcbLevel2)] = 22,
                [nameof(StatTypes.MwcbLevel3)] = 23,
                [nameof(StatTypes.AuctionCollarReferencePrice)] = 24,
                [nameof(StatTypes.AuctionCollarUpperPrice)] = 25,
                [nameof(StatTypes.AuctionCollarLowerPrice)] = 26,
                [nameof(StatTypes.VenueSpecificVolume1)] = 10001,
                [nameof(StatTypes.VenueSpecificPrice1)] = 10002,
            };
            Dictionary<string, ushort> actual = typeof(StatTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral)
                .ToDictionary(field => field.Name, field => (ushort)field.GetRawConstantValue());

            CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
        }

        private static List<T> ConstantsOf<T>(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(T))
                .Select(field => (T)field.GetRawConstantValue())
                .ToList();
        }
    }
}
