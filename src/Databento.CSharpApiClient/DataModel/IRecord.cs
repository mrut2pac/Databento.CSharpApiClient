using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// The fields every Databento record carries, read the same way from its JSON and DBN encodings. The JSON and DBN record
    /// classes implement the record interfaces explicitly, so code written against an interface works with either client.
    /// </summary>
    public interface IRecord
    {
        /// <summary>Gets the record type.</summary>
        RType RecordType { get; }

        /// <summary>Gets the publisher that sourced the record.</summary>
        ushort PublisherId { get; }

        /// <summary>Gets Databento's numeric instrument ID.</summary>
        uint InstrumentId { get; }

        /// <summary>Gets the event time at the venue, in UTC; <see langword="null"/> when the venue sent none.</summary>
        DateTime? TsEventUtc { get; }

        /// <summary>
        /// Gets the requested symbol the record belongs to, in the request's input symbology (the raw symbol for a raw-symbol
        /// request). <see langword="null"/> when the response does not map the record: an <c>ALL_SYMBOLS</c> JSON record the
        /// response leaves unmapped, or a DBN record whose instrument the stream's metadata maps to no symbol on its date.
        /// </summary>
        string Symbol { get; }
    }
}
