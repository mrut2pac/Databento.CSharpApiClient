using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// The consolidated best bid and offer across venues, sampled each second or minute with the last trade
    /// (<c>cbbo-1s</c>, <c>cbbo-1m</c>). Implemented by <see cref="Json.CbboRecordJson"/> and <see cref="Dbn.CbboRecordDbn"/>.
    /// </summary>
    public interface ICbboRecord : IRecord
    {
        /// <summary>Gets the last trade price; <see langword="null"/> when there was no trade.</summary>
        double? Price { get; }

        /// <summary>Gets the last trade quantity.</summary>
        uint Size { get; }

        /// <summary>Gets the aggressor side of the last trade.</summary>
        TradeAggressor Side { get; }

        /// <summary>Gets the message info flags.</summary>
        MessageInfoBits Flags { get; }

        /// <summary>Gets the time Databento received the message, in UTC; <see langword="null"/> when undefined.</summary>
        DateTime? TsReceivedUtc { get; }

        /// <summary>Gets the best bid price; <see langword="null"/> when nobody is bidding.</summary>
        double? BidPrice { get; }

        /// <summary>Gets the best ask price; <see langword="null"/> when nobody is offering.</summary>
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
