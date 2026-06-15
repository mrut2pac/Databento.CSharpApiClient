using System;

namespace Databento.CSharpApiClient.Exceptions
{
    /// <summary>
    /// Thrown for client-side failures that are not HTTP errors — most notably when a record in an
    /// otherwise-successful response cannot be deserialized. A parse failure is surfaced rather than
    /// swallowed so callers can distinguish a genuine empty result from a response whose records failed
    /// to decode.
    /// </summary>
    public sealed class DatabentoException : Exception
    {
        /// <summary>
        /// Initialises a new instance with the given message.
        /// </summary>
        /// <param name="message">Description of the failure.</param>
        public DatabentoException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance with the given message and underlying cause.
        /// </summary>
        /// <param name="message">Description of the failure.</param>
        /// <param name="innerException">The exception that triggered this failure.</param>
        public DatabentoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
