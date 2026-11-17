using System;

namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// A statistic a venue publishes, such as open interest or a settlement price (<c>statistics</c>). Implemented by
    /// <see cref="Json.StatisticsRecordJson"/> and <see cref="Dbn.StatisticsRecordDbn"/>.
    /// </summary>
    public interface IStatisticsRecord : IRecord
    {
        /// <summary>Gets the time Databento received the message, in UTC; <see langword="null"/> when undefined.</summary>
        DateTime? TsReceivedUtc { get; }

        /// <summary>Gets the reference time the statistic applies to, in UTC; <see langword="null"/> when it has none.</summary>
        DateTime? TsRefUtc { get; }

        /// <summary>Gets the statistic's price; <see langword="null"/> for a statistic without one, such as open interest.</summary>
        double? Price { get; }

        /// <summary>
        /// Gets the statistic's quantity, such as the open interest; <see langword="null"/> for a statistic without one. A
        /// quantity of exactly <see cref="int.MaxValue"/> also reads as <see langword="null"/>, since it is the undefined value of
        /// the 32-bit quantity in DBN v1 data.
        /// </summary>
        long? Quantity { get; }

        /// <summary>Gets the venue's message sequence number.</summary>
        uint Sequence { get; }

        /// <summary>Gets the nanoseconds between the venue sending the message and Databento receiving it.</summary>
        int TsInDelta { get; }

        /// <summary>Gets the statistic type, as Databento numbers them (e.g. 9 for open interest).</summary>
        ushort StatType { get; }

        /// <summary>Gets the channel the statistic was published on.</summary>
        ushort ChannelId { get; }

        /// <summary>Gets the update action: 1 adds the statistic, 2 deletes it.</summary>
        byte UpdateAction { get; }

        /// <summary>Gets the statistic's flags.</summary>
        MessageInfoBits StatFlags { get; }
    }
}
