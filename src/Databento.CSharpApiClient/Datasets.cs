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

        // U.S. Equity Options

        /// <summary>OPRA PILLAR — U.S. equity option NBBO, trades, and top-of-book.</summary>
        public const string OpraPillar = "OPRA.PILLAR";

        // U.S. Futures / Commodities

        /// <summary>CME Globex MDP 3.0 — E-mini, energy, metals, and more.</summary>
        public const string GlbxMdp3 = "GLBX.MDP3";

        // European Futures / Fixed Income

        /// <summary>ICE Europe IMPACT.</summary>
        public const string IfeuImpact = "IFEU.IMPACT";

        /// <summary>ICE Endex IMPACT.</summary>
        public const string NdexImpact = "NDEX.IMPACT";

        // Databento Equities roll-up products

        /// <summary>Databento Equities Basic (consolidated U.S. equity NBBO and trades).</summary>
        public const string DbeqBasic = "DBEQ.BASIC";

        /// <summary>Databento Equities Plus (extended coverage on top of Basic).</summary>
        public const string DbeqPlus = "DBEQ.PLUS";

        /// <summary>Databento Equities Max (full market depth).</summary>
        public const string DbeqMax = "DBEQ.MAX";
    }
}
