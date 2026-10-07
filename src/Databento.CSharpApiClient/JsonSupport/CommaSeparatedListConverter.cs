using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// Reads a list the API sends either as one comma-joined string (e.g. <c>"SPY,QQQ"</c>) or as a JSON array of strings,
    /// and writes it back as the comma-joined string.
    /// </summary>
    internal sealed class CommaSeparatedListConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch(reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    string joined = reader.GetString();
                    return string.IsNullOrEmpty(joined) ? Array.Empty<string>() : joined.Split(',');
                case JsonTokenType.StartArray:
                    List<string> items = new List<string>();
                    while(reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        items.Add(reader.GetString());
                    }

                    return items.ToArray();
                default:
                    throw new JsonException("Expected a comma-joined string or an array of strings, got " + reader.TokenType + ".");
            }
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", value));
    }
}
