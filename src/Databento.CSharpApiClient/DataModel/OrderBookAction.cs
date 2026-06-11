namespace Databento.CSharpApiClient.DataModel
{
    /// <summary>
    /// Order-book action codes as published in DBN messages.
    /// The underlying byte value equals the ASCII character sent by the venue.
    /// </summary>
    public enum OrderBookAction : byte
    {
#pragma warning disable IDE0055
        /// <summary>Unknown or unrecognised action.</summary>
        Unknown =    0,

        /// <summary>A new order was added to the book ('A').</summary>
        Add =        (byte)'A',

        /// <summary>An existing order was modified ('M').</summary>
        Modify =     (byte)'M',

        /// <summary>An order was removed/cancelled ('D').</summary>
        Delete =     (byte)'D',

        /// <summary>The order book was cleared/reset ('R').</summary>
        Reset =      (byte)'R',

        /// <summary>An order was updated in place ('U').</summary>
        Update =     (byte)'U',

        /// <summary>An order was fully or partially filled ('F').</summary>
        Fill =       (byte)'F',

        /// <summary>A trade occurred ('T').</summary>
        Trade =      (byte)'T',
    }
}
