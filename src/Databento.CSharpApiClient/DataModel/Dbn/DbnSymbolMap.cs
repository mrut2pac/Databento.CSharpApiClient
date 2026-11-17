using System;
using System.Collections.Generic;
using System.Globalization;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Finds the requested symbol a DBN record belongs to from the symbol mappings in the stream's metadata: the input symbol
    /// whose interval maps to the record's instrument ID on the UTC date of the record's index timestamp. Only mappings to
    /// instrument IDs (stype_out instrument_id, what a historical request sends) can be looked up; any other leaves every
    /// record without a symbol. When two input symbols map to the same instrument on a date, the first in the metadata wins.
    /// </summary>
    internal sealed class DbnSymbolMap
    {
        private readonly Dictionary<uint, List<Interval>> intervalsByInstrument = new Dictionary<uint, List<Interval>>();

        public DbnSymbolMap(DbnMetadata metadata)
        {
            if(metadata.STypeOut != DbnSType.InstrumentId)
            {
                return;
            }

            foreach(DbnSymbolMapping mapping in metadata.Mappings)
            {
                foreach(DbnMappingInterval interval in mapping.Intervals)
                {
                    if(!uint.TryParse(interval.Symbol, NumberStyles.None, CultureInfo.InvariantCulture, out uint instrumentId))
                    {
                        continue;
                    }

                    if(!this.intervalsByInstrument.TryGetValue(instrumentId, out List<Interval> intervals))
                    {
                        intervals = new List<Interval>();
                        this.intervalsByInstrument.Add(instrumentId, intervals);
                    }

                    intervals.Add(new Interval(interval.StartDate, interval.EndDate, mapping.RawSymbol));
                }
            }
        }

        /// <summary>
        /// Gets a record's index timestamp: its ts_recv, or its ts_event when ts_recv is undefined.
        /// </summary>
        public static DateTime IndexTime(DateTime tsReceivedUtc, DateTime tsEventUtc)
        {
            return tsReceivedUtc == DateTime.MinValue ? tsEventUtc : tsReceivedUtc;
        }

        /// <summary>
        /// Gets the symbol an instrument maps to on the date of an index timestamp, or <see langword="null"/> if none does.
        /// </summary>
        /// <param name="instrumentId">Record's instrument ID</param>
        /// <param name="indexTimeUtc">Record's index timestamp (<see cref="IndexTime"/>), or the ts_event of a bar, which has no ts_recv</param>
        public string Find(uint instrumentId, DateTime indexTimeUtc)
        {
            if(!this.intervalsByInstrument.TryGetValue(instrumentId, out List<Interval> intervals))
            {
                return null;
            }

            // mappings carry dates as yyyymmdd, with the end date exclusive
            uint date = (uint)(indexTimeUtc.Year * 10000 + indexTimeUtc.Month * 100 + indexTimeUtc.Day);
            foreach(Interval interval in intervals)
            {
                if(interval.StartDate <= date && date < interval.EndDate)
                {
                    return interval.Symbol;
                }
            }

            return null;
        }

        private readonly struct Interval
        {
            public Interval(uint startDate, uint endDate, string symbol)
            {
                this.StartDate = startDate;
                this.EndDate = endDate;
                this.Symbol = symbol;
            }

            public uint StartDate { get; }

            public uint EndDate { get; }

            public string Symbol { get; }
        }
    }
}
