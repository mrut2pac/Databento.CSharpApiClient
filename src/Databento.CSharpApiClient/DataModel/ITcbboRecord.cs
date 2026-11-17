using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// A trade with the consolidated best bid and offer just before it (<c>tcbbo</c>). Implemented by
    /// <see cref="Json.TcbboRecordJson"/> and <see cref="Dbn.TcbboRecordDbn"/>.
    /// </summary>
    public interface ITcbboRecord : IRecord
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

        /// <summary>Gets the time Databento received the message, in UTC; <see langword="null"/> when undefined.</summary>
        DateTime? TsReceivedUtc { get; }

        /// <summary>Gets the nanoseconds between the venue sending the message and Databento receiving it.</summary>
        int TsInDelta { get; }

        /// <summary>Gets the best bid price before the trade; <see langword="null"/> when nobody was bidding.</summary>
        double? BidPrice { get; }

        /// <summary>Gets the best ask price before the trade; <see langword="null"/> when nobody was offering.</summary>
        double? AskPrice { get; }

        /// <summary>Gets the quantity bid at the best bid.</summary>
        uint BidSize { get; }

        /// <summary>Gets the quantity offered at the best ask.</summary>
        uint AskSize { get; }

        /// <summary>Gets the publisher of the best bid.</summary>
        ushort BidPublisherId { get; }

        /// <summary>Gets the publisher of the best ask.</summary>
        ushort AskPublisherId { get; }
    }
}
