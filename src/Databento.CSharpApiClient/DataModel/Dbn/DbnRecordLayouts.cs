namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// The layout of every DBN schema the client decodes and encodes.
    /// </summary>
    internal static class DbnRecordLayouts
    {
        public static readonly DbnRecordLayout<CbboRecordDbn> Cbbo = new DbnRecordLayout<CbboRecordDbn>(
            "CBBO",
            type => type == RType.Cbbo1S || type == RType.Cbbo1M,
            CbboRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, DbnSymbolMap.IndexTime(record.TsReceivedUtc, record.TsEventUtc)));

        public static readonly DbnRecordLayout<TradeRecordDbn> Trades = new DbnRecordLayout<TradeRecordDbn>(
            "Trades",
            type => type == RType.Mbp0,
            TradeRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, DbnSymbolMap.IndexTime(record.TsReceivedUtc, record.TsEventUtc)));

        public static readonly DbnRecordLayout<Mbp1RecordDbn> Mbp1 = new DbnRecordLayout<Mbp1RecordDbn>(
            "MBP-1",
            type => type == RType.Mbp1,
            Mbp1RecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        public static readonly DbnRecordLayout<MboRecordDbn> Mbo = new DbnRecordLayout<MboRecordDbn>(
            "MBO",
            type => type == RType.Mbo,
            MboRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        public static readonly DbnRecordLayout<Mbp10RecordDbn> Mbp10 = new DbnRecordLayout<Mbp10RecordDbn>(
            "MBP-10",
            type => type == RType.Mbp10,
            Mbp10RecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        // the tbbo schema sends its records with the MBP-1 record type and layout
        public static readonly DbnRecordLayout<TbboRecordDbn> Tbbo = new DbnRecordLayout<TbboRecordDbn>(
            "TBBO",
            type => type == RType.Mbp1,
            TbboRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        public static readonly DbnRecordLayout<BboRecordDbn> Bbo = new DbnRecordLayout<BboRecordDbn>(
            "BBO",
            type => type == RType.Bbo1S || type == RType.Bbo1M,
            BboRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, DbnSymbolMap.IndexTime(record.TsReceivedUtc, record.TsEventUtc)));

        public static readonly DbnRecordLayout<TcbboRecordDbn> Tcbbo = new DbnRecordLayout<TcbboRecordDbn>(
            "TCBBO",
            type => type == RType.Tcbbo,
            TcbboRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, DbnSymbolMap.IndexTime(record.TsReceivedUtc, record.TsEventUtc)));

        public static readonly DbnRecordLayout<Cmbp1RecordDbn> Cmbp1 = new DbnRecordLayout<Cmbp1RecordDbn>(
            "CMBP-1",
            type => type == RType.Cmbp1,
            Cmbp1RecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        // a bar has no ts_recv, so its symbol is looked up on the date of its ts_event
        public static readonly DbnRecordLayout<OhlcvRecordDbn> Ohlcv = new DbnRecordLayout<OhlcvRecordDbn>(
            "OHLCV",
            type => type == RType.Ohlcv1S || type == RType.Ohlcv1M || type == RType.Ohlcv1H || type == RType.Ohlcv1D || type == RType.OhlcvEod,
            OhlcvRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, record.TsEventUtc));

        public static readonly DbnRecordLayout<StatisticsRecordDbn> Statistics = new DbnRecordLayout<StatisticsRecordDbn>(
            "Statistics",
            type => type == RType.Statistics,
            StatisticsRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer, version),
            (record, map) => record.Symbol = map.Find(record.InstrumentId, DbnSymbolMap.IndexTime(record.TsReceivedUtc, record.TsEventUtc)));

        public static readonly DbnRecordLayout<DefinitionRecordDbn> Definitions = new DbnRecordLayout<DefinitionRecordDbn>(
            "Definition",
            type => type == RType.InstrumentDef,
            DefinitionRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer, version));

        public static readonly DbnRecordLayout<StatusRecordDbn> Status = new DbnRecordLayout<StatusRecordDbn>(
            "Status",
            type => type == RType.Status,
            StatusRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));

        public static readonly DbnRecordLayout<ImbalanceRecordDbn> Imbalance = new DbnRecordLayout<ImbalanceRecordDbn>(
            "Imbalance",
            type => type == RType.Imbalance,
            ImbalanceRecordDbn.ReadFromBytes,
            (record, writer, version) => record.WriteToBytes(writer));
    }
}
