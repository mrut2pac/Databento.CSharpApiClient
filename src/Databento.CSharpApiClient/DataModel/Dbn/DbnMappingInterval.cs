namespace Databento.CSharpApiClient.DataModel.Dbn
{
    public sealed class DbnMappingInterval
    {
        public uint StartDate { get; set; }   // YYYYMMDD
        public uint EndDate { get; set; }     // YYYYMMDD
        public string Symbol { get; set; }    // char[symbol_cstr_len], stype_out
    }
}
