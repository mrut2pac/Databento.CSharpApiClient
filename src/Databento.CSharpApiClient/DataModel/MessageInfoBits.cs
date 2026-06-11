using System;

namespace Databento.CSharpApiClient.DataModel
{
    [Flags]
    public enum MessageInfoBits : byte
    {
        None = 0,
        Reserved = 1,
        PublisherSpecific = 2,
        MaybeBadBook = 4,
        BadTimestampReceived = 8,
        MBP = 16,
        Snapshot = 32,
        TOB = 64,
        Last = 128,
    }
}
