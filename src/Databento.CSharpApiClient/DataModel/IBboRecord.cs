using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// A venue's best bid and offer, sampled each second or minute with the last trade (<c>bbo-1s</c>, <c>bbo-1m</c>).
    /// Implemented by <see cref="Json.BboRecordJson"/> and <see cref="Dbn.BboRecordDbn"/>.
    /// </summary>
    public interface IBboRecord : IRecord
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

        /// <summary>Gets the venue's message sequence number.</summary>
        uint Sequence { get; }

        /// <summary>Gets the best bid price; <see langword="null"/> when nobody is bidding.</summary>
        double? BidPrice { get; }

        /// <summary>Gets the best ask price; <see langword="null"/> when nobody is offering.</summary>
        double? AskPrice { get; }

        /// <summary>Gets the quantity bid at the best bid.</summary>
        uint BidSize { get; }

        /// <summary>Gets the quantity offered at the best ask.</summary>
        uint AskSize { get; }

        /// <summary>Gets the number of orders at the best bid.</summary>
        uint BidCount { get; }

        /// <summary>Gets the number of orders at the best ask.</summary>
        uint AskCount { get; }
    }
}
