using System.Collections.Generic;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    public sealed class DbnSymbolMapping
    {
        public string RawSymbol { get; set; }               // char[symbol_cstr_len], stype_in
        public List<DbnMappingInterval> Intervals { get; set; } = new List<DbnMappingInterval>();
    }
}
