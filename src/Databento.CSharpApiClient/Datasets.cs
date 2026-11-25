namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Dataset identifiers for commonly used Databento venues.
    /// Full list via <c>metadata.list_datasets</c>.
    /// </summary>
    public static class Datasets
    {
        // U.S. Equities

        /// <summary>Nasdaq TotalView-ITCH (full order book).</summary>
        public const string XnasItch = "XNAS.ITCH";

        /// <summary>Nasdaq BX TotalView-ITCH.</summary>
        public const string XbosItch = "XBOS.ITCH";

        /// <summary>Nasdaq PSX TotalView-ITCH.</summary>
        public const string XpsxItch = "XPSX.ITCH";

        /// <summary>Cboe BZX PITCH (full order book).</summary>
        public const string BatsPitch = "BATS.PITCH";

        /// <summary>Cboe BYX PITCH.</summary>
        public const string BatyPitch = "BATY.PITCH";

        /// <summary>Cboe EDGX PITCH.</summary>
        public const string EdgxPitch = "EDGX.PITCH";

        /// <summary>Cboe EDGA PITCH.</summary>
        public const string EdgaPitch = "EDGA.PITCH";

        /// <summary>Nasdaq Basic with NLS Plus (top of book and trades).</summary>
        public const string XnasBasic = "XNAS.BASIC";

        /// <summary>NYSE Integrated (full order book).</summary>
        public const string XnysPillar = "XNYS.PILLAR";

        /// <summary>NYSE Arca Integrated.</summary>
        public const string ArcxPillar = "ARCX.PILLAR";

        /// <summary>NYSE American Integrated.</summary>
        public const string XasePillar = "XASE.PILLAR";

        /// <summary>NYSE Texas Integrated.</summary>
        public const string XchiPillar = "XCHI.PILLAR";

        /// <summary>NYSE National trades and BBO.</summary>
        public const string XcisTradesBbo = "XCIS.TRADESBBO";

        /// <summary>IEX TOPS (top of book and trades).</summary>
        public const string IexgTops = "IEXG.TOPS";

        /// <summary>MEMX MEMOIR (full order book).</summary>
        public const string MemxMemoir = "MEMX.MEMOIR";

        /// <summary>MIAX Pearl Equities depth of market.</summary>
        public const string EprlDom = "EPRL.DOM";

        /// <summary>Blue Ocean ATS MEMOIR (full order book).</summary>
        public const string OceaMemoir = "OCEA.MEMOIR";

        // U.S. Equity Options

        /// <summary>OPRA PILLAR — U.S. equity option NBBO, trades, and top-of-book.</summary>
        public const string OpraPillar = "OPRA.PILLAR";

        // U.S. Futures / Commodities

        /// <summary>CME Globex MDP 3.0 — E-mini, energy, metals, and more.</summary>
        public const string GlbxMdp3 = "GLBX.MDP3";

        /// <summary>ICE Futures U.S. iMpact.</summary>
        public const string IfusImpact = "IFUS.IMPACT";

        /// <summary>Cboe Futures Exchange (CFE) PITCH.</summary>
        public const string XcbfPitch = "XCBF.PITCH";

        // European Futures / Fixed Income

        /// <summary>ICE Europe IMPACT.</summary>
        public const string IfeuImpact = "IFEU.IMPACT";

        /// <summary>ICE Endex IMPACT.</summary>
        public const string NdexImpact = "NDEX.IMPACT";

        /// <summary>ICE Europe Financials iMpact.</summary>
        public const string IfllImpact = "IFLL.IMPACT";

        /// <summary>Eurex EOBI.</summary>
        public const string XeurEobi = "XEUR.EOBI";

        /// <summary>European Energy Exchange EOBI.</summary>
        public const string XeeeEobi = "XEEE.EOBI";

        // Databento Equities roll-up products

        /// <summary>Databento Equities Basic (consolidated U.S. equity NBBO and trades).</summary>
        public const string DbeqBasic = "DBEQ.BASIC";

        /// <summary>Databento Equities Plus (extended coverage on top of Basic).</summary>
        public const string DbeqPlus = "DBEQ.PLUS";

        /// <summary>Databento Equities Max (full market depth).</summary>
        public const string DbeqMax = "DBEQ.MAX";

        /// <summary>Databento US Equities Mini (consolidated top of book and trades across U.S. venues).</summary>
        public const string EqusMini = "EQUS.MINI";

        /// <summary>Databento US Equities Summary (consolidated end-of-day summaries).</summary>
        public const string EqusSummary = "EQUS.SUMMARY";

        // Indices

        /// <summary>Databento Core Indices — index values from the Cboe Global Indices Feed (CGIF).</summary>
        public const string DbixCore = "DBIX.CORE";
    }
}
