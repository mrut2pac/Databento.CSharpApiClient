using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// A trade (<c>trades</c>). Implemented by <see cref="Json.TradeRecordJson"/> and <see cref="Dbn.TradeRecordDbn"/>.
    /// </summary>
    public interface ITradeRecord : IRecord
    {
        /// <summary>Gets the trade price; <see langword="null"/> when the venue sent none.</summary>
        double? Price { get; }

        /// <summary>Gets the trade quantity.</summary>
        uint Size { get; }

        /// <summary>Gets the order book action, a trade.</summary>
        OrderBookAction Action { get; }

        /// <summary>Gets the aggressor side of the trade.</summary>
        TradeAggressor Side { get; }

        /// <summary>Gets the message info flags.</summary>
        MessageInfoBits Flags { get; }

        /// <summary>Gets the book level the trade happened at.</summary>
        byte Depth { get; }

        /// <summary>Gets the time Databento received the message, in UTC; <see langword="null"/> when undefined.</summary>
        DateTime? TsReceivedUtc { get; }

        /// <summary>Gets the nanoseconds between the venue sending the message and Databento receiving it.</summary>
        int TsInDelta { get; }

        /// <summary>Gets the venue's message sequence number.</summary>
        uint Sequence { get; }
    }
}
