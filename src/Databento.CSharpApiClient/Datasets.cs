namespace Databento.CSharpApiClient
{
    /// <summary>
    /// Dataset identifiers for commonly used Databento venues.
    /// Full list via <c>metadata.list_datasets</c>.
    /// </summary>
    public static class Datasets
    {
        // U.S. Equities
        public const string XnasItch = "XNAS.ITCH";     // Nasdaq TotalView-ITCH
        public const string XbosItch = "XBOS.ITCH";     // Nasdaq BX TotalView-ITCH
        public const string XpsxItch = "XPSX.ITCH";     // Nasdaq PSX TotalView-ITCH
        public const string BatsPitch = "BATS.PITCH";   // Cboe BZX PITCH
        public const string BatyPitch = "BATY.PITCH";   // Cboe BYX PITCH
        public const string EdgxPitch = "EDGX.PITCH";   // Cboe EDGX PITCH
        public const string EdgaPitch = "EDGA.PITCH";   // Cboe EDGA PITCH

        // U.S. Equity Options
        public const string OpraPillar = "OPRA.PILLAR"; // OPRA (U.S. equity option NBBO, trades, ToB)

        // U.S. Futures / Commodities
        public const string GlbxMdp3 = "GLBX.MDP3";   // CME Globex MDP 3.0 (E-mini, energy, metals …)

        // European Futures / Fixed Income
        public const string IfeuImpact = "IFEU.IMPACT"; // ICE Europe IMPACT
        public const string NdexImpact = "NDEX.IMPACT"; // ICE Endex IMPACT

        // Databento Equities roll-up products
        public const string DbeqBasic = "DBEQ.BASIC";   // Databento Equities Basic
        public const string DbeqPlus = "DBEQ.PLUS";     // Databento Equities Plus
        public const string DbeqMax = "DBEQ.MAX";       // Databento Equities Max
    }
}
