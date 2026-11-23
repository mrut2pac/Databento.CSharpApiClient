using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads a record timestamp in either form of the JSON encoding: an ISO-8601 string (<c>pretty_ts</c>), read as the serializer
    /// reads one by default, or UNIX nanoseconds, as a string or a number. Undefined nanoseconds (0 or u64::MAX) read as
    /// <see cref="DateTime.MinValue"/>, as the DBN decoder reads them. Writes ISO-8601, as the serializer does by default.
    /// </summary>
    internal sealed class RecordTimestampConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TryReadUnixNs(ref reader, out ulong ns) ? Utils.FromUnixNs(ns).UtcDateTime : reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        /// <summary>
        /// Reads the current token as UNIX nanoseconds when it is a number or a string of digits only; an ISO-8601 string, which
        /// has separators, is not.
        /// </summary>
        internal static bool TryReadUnixNs(ref Utf8JsonReader reader, out ulong ns)
        {
            if(reader.TokenType == JsonTokenType.Number)
            {
                ns = reader.GetUInt64();
                return true;
            }

            ns = 0;
            if(reader.TokenType != JsonTokenType.String || reader.HasValueSequence || reader.ValueIsEscaped)
            {
                return false;
            }

            ReadOnlySpan<byte> text = reader.ValueSpan;
            return Utf8Parser.TryParse(text, out ns, out int consumed) && consumed == text.Length;
        }
    }

    /// <summary>
    /// Reads an optional record timestamp as <see cref="RecordTimestampConverter"/> does, except that undefined nanoseconds read as
    /// <see langword="null"/>, as the pretty encoding sends them.
    /// </summary>
    internal sealed class NullableRecordTimestampConverter : JsonConverter<DateTime?>
    {
        public override bool HandleNull => true;

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return RecordTimestampConverter.TryReadUnixNs(ref reader, out ulong ns)
                ? UndefinedValues.Timestamp(Utils.FromUnixNs(ns).UtcDateTime)
                : reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if(value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
