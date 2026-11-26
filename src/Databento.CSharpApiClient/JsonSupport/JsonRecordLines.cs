using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Databento.CSharpApiClient.DataModel.Json;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads Databento's JSON records from a stream as the client models them, for <see cref="DatabentoJsonClient"/>'s responses
    /// and <see cref="JsonRecordReader"/>'s data alike.
    /// </summary>
    internal static class JsonRecordLines
    {
        /// <summary>
        /// Gets the options every JSON record, and every other response of the JSON client, is read with. Property names come from
        /// the models' <c>[JsonPropertyName]</c> attributes, and unknown properties are ignored.
        /// </summary>
        internal static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,

            // The Historical API serialises numeric fields as JSON strings when pretty_px=true
            // (e.g. "price":"4.000000000", "bid_px":"3.700000000"); read those into the numeric DTO
            // properties instead of throwing on the string token.
            NumberHandling = JsonNumberHandling.AllowReadingFromString,

            // a timestamp is ISO-8601 with pretty_ts, as the client requests it, and UNIX nanoseconds without, a batch job's default
            Converters = { new RecordTimestampConverter(), new NullableRecordTimestampConverter() },
        };

        /// <summary>
        /// Reads the records of <paramref name="stream"/> one at a time and leaves it open. Newline-delimited JSON, the timeseries and
        /// batch encoding, is read one line at a time; a JSON array document (in UTF-8) one element at a time, as line 1. Blank lines are skipped, and so
        /// are records without a header unless <paramref name="requireHeader"/>. A line that doesn't parse, or a headerless record
        /// when one is required, is thrown as the exception <paramref name="malformedLine"/> makes of the record's type name, the line
        /// number and the failure.
        /// </summary>
        internal static async IAsyncEnumerable<T> ReadAsync<T>(
            Stream stream,
            Func<T, RecordHeaderJson> header,
            bool requireHeader,
            Func<string, long, Exception, Exception> malformedLine,
            [EnumeratorCancellation] CancellationToken ct)
            where T : class
        {
            if(!stream.CanRead)
            {
                // as the StreamReader that read every stream before refused it
                throw new ArgumentException("Stream was not readable.", nameof(stream));
            }

            // A JSON array document starts with '[', after a UTF-8 byte order mark if any; anything else is newline-delimited JSON
            // (the timeseries encoding). The first bytes are read to tell them apart, then read again with the rest.
            byte[] head = new byte[4];
            int headLength = 0;
            while(headLength < head.Length)
            {
                int read = await stream.ReadAsync(head.AsMemory(headLength), ct).ConfigureAwait(false);
                if(read == 0)
                {
                    break;
                }

                headLength += read;
            }

            int first = headLength >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF ? 3 : 0;
            PrefixedStream data = new PrefixedStream(head, headLength, stream);
            if(first < headLength && head[first] == '[')
            {
                // read one record at a time as the array arrives, never the whole document at once
                await foreach(T item in ReadArrayAsync<T>(data, malformedLine, ct).ConfigureAwait(false))
                {
                    ct.ThrowIfCancellationRequested();
                    if(IsRecord(item, header, requireHeader, 1, malformedLine))
                    {
                        yield return item;
                    }
                }

                yield break;
            }

            using(StreamReader sr = new StreamReader(data, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: true))
            {
                string line = await sr.ReadLineAsync(ct).ConfigureAwait(false);
                long lineNumber = 0;
                for(; line != null; line = await sr.ReadLineAsync(ct).ConfigureAwait(false))
                {
                    ct.ThrowIfCancellationRequested();
                    lineNumber++;

                    // A blank separator line (e.g. a trailing newline) is legal in JSONL and carries
                    // no record. Any non-blank line is expected to be a complete record object.
                    if(string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    T item = ReadLine<T>(line, lineNumber, typeof(T).Name, malformedLine);
                    if(IsRecord(item, header, requireHeader, lineNumber, malformedLine))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Reads a JSON array document's elements as they are read; a document that doesn't parse, or isn't one array, is thrown as
        /// the exception <paramref name="malformedLine"/> makes of it, as line 1.
        /// </summary>
        private static async IAsyncEnumerable<T> ReadArrayAsync<T>(Stream stream, Func<string, long, Exception, Exception> malformedLine, [EnumeratorCancellation] CancellationToken ct)
        {
            IAsyncEnumerator<T> items = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, Options, ct).GetAsyncEnumerator(ct);
            try
            {
                while(true)
                {
                    try
                    {
                        if(!await items.MoveNextAsync().ConfigureAwait(false))
                        {
                            yield break;
                        }
                    }
                    catch(Exception ex) when(ex is JsonException or FormatException or OverflowException or NotSupportedException)
                    {
                        throw malformedLine(typeof(T).Name, 1, ex);
                    }

                    yield return items.Current;
                }
            }
            finally
            {
                await items.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static TValue ReadLine<TValue>(string line, long lineNumber, string recordType, Func<string, long, Exception, Exception> malformedLine)
        {
            try
            {
                return JsonSerializer.Deserialize<TValue>(line, Options);
            }
            catch(Exception ex) when(ex is JsonException or FormatException or OverflowException or NotSupportedException)
            {
                // A record that fails to parse is a real error, not "no data": swallowing it once returned zero rows for responses
                // that contained data. A custom converter may throw Format/Overflow rather than JsonException.
                throw malformedLine(recordType, lineNumber, ex);
            }
        }

        // a record has a header; JSON without one (null, or an object of another kind) is skipped, or refused when a record is required
        private static bool IsRecord<T>(T item, Func<T, RecordHeaderJson> header, bool requireHeader, long lineNumber, Func<string, long, Exception, Exception> malformedLine)
            where T : class
        {
            if(item != null && header(item) != null)
            {
                return true;
            }

            if(requireHeader)
            {
                throw malformedLine(typeof(T).Name, lineNumber, new JsonException("The JSON has no record header (\"hd\")."));
            }

            return false;
        }
    }
}
