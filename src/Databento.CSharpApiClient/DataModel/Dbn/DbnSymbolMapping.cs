using System.Collections.Generic;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Maps a single input symbol to a list of time-sliced output-symbol intervals,
    /// as stored in the <see cref="DbnMetadata"/> header of a DBN stream.
    /// </summary>
    public sealed class DbnSymbolMapping
    {
        /// <summary>The input (stype_in) symbol string as it appeared in the request.</summary>
        public string RawSymbol { get; set; }

        /// <summary>Ordered, non-overlapping date intervals mapping the raw symbol to its output form.</summary>
        public List<DbnMappingInterval> Intervals { get; set; } = new List<DbnMappingInterval>();
    }
}
