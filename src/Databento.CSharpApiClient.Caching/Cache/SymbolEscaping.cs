using System;
using System.Globalization;
using System.Text;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// Writes a symbol into a file name and reads it back exactly. ASCII capital letters, digits, <c>.</c> and <c>-</c> stay as they are;
    /// every other character's UTF-8 bytes are written as <c>%XX</c> (OPRA symbols pad their root with spaces). So an escaped symbol never
    /// holds the <c>_</c> that separates the parts of a cache file name, and two symbols that differ only in case never share a name on
    /// a file system that ignores case.
    /// </summary>
    internal static class SymbolEscaping
    {
        /// <summary>
        /// Escapes a symbol for a file name: <c>SPXW  261016C06600000</c> is <c>SPXW%20%20261016C06600000</c>
        /// </summary>
        /// <param name="symbol">Symbol, not empty</param>
        /// <returns>The escaped symbol</returns>
        public static string Escape(string symbol)
        {
            ArgumentException.ThrowIfNullOrEmpty(symbol);

            StringBuilder escaped = new StringBuilder(symbol.Length);
            foreach(byte character in Encoding.UTF8.GetBytes(symbol))
            {
                if(IsKept((char)character))
                {
                    escaped.Append((char)character);
                }
                else
                {
                    escaped.Append('%').Append(character.ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            return escaped.ToString();
        }

        /// <summary>
        /// Reads an escaped symbol back. Only the form <see cref="Escape"/> writes is accepted, so no two names stand for one symbol.
        /// </summary>
        /// <param name="escaped">Escaped symbol</param>
        /// <param name="symbol">The symbol</param>
        /// <returns><c>false</c> when the text is no symbol as <see cref="Escape"/> writes one</returns>
        public static bool TryUnescape(string escaped, out string symbol)
        {
            symbol = null;
            if(string.IsNullOrEmpty(escaped))
            {
                return false;
            }

            byte[] bytes = new byte[escaped.Length];
            int length = 0;
            for(int i = 0; i < escaped.Length; ++i)
            {
                char character = escaped[i];
                if(character == '%' && i + 2 < escaped.Length
                    && byte.TryParse(escaped.AsSpan(i + 1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte value))
                {
                    bytes[length++] = value;
                    i += 2;
                }
                else if(IsKept(character))
                {
                    bytes[length++] = (byte)character;
                }
                else
                {
                    return false;
                }
            }

            string decoded;
            try
            {
                decoded = new UTF8Encoding(false, true).GetString(bytes, 0, length);
            }
            catch(DecoderFallbackException)
            {
                return false;
            }

            if(!string.Equals(Escape(decoded), escaped, StringComparison.Ordinal))
            {
                return false;
            }

            symbol = decoded;
            return true;
        }

        private static bool IsKept(char character)
        {
            return char.IsAsciiLetterUpper(character) || char.IsAsciiDigit(character) || character == '.' || character == '-';
        }
    }
}
