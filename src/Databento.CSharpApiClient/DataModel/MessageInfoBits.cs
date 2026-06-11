using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// Bit-flags that convey metadata about a DBN message.
    /// Multiple flags may be set simultaneously.
    /// </summary>
    [Flags]
    public enum MessageInfoBits : byte
    {
        /// <summary>No flags set.</summary>
        None = 0,

        /// <summary>Reserved for future use.</summary>
        Reserved = 1,

        /// <summary>Publisher-specific supplemental flag.</summary>
        PublisherSpecific = 2,

        /// <summary>The order book state may be unreliable (e.g. following a gap).</summary>
        MaybeBadBook = 4,

        /// <summary>A bad timestamp was received from the venue for this message.</summary>
        BadTimestampReceived = 8,

        /// <summary>The record was generated from a market-by-price feed.</summary>
        MBP = 16,

        /// <summary>The record is a snapshot (initial book state), not an incremental update.</summary>
        Snapshot = 32,

        /// <summary>The record represents the top-of-book level only.</summary>
        TOB = 64,

        /// <summary>This is the last message in a group (e.g. the completing trade in a fill).</summary>
        Last = 128,
    }
}
