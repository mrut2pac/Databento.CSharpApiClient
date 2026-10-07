using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Compares records decoded from the DBN binary encoding with the JSON encoding of the same request, property by property.
    /// A misread binary layout yields plausible-looking values, and the JSON encoding reads fields by name and shares no layout code
    /// with the binary decoders, so agreement on every shared property pins the layout. A property only one model has is reported too,
    /// so a field the DBN decoder doesn't read shows up.
    /// </summary>
    internal static class EncodingComparer
    {
        /// <summary>Properties that legitimately exist on one side only, as <c>Model.Property</c> (<c>*.Property</c> for every model).</summary>
        private static readonly HashSet<string> OneSided = new HashSet<string>(StringComparer.Ordinal)
        {
            // JSON-only: the request's own symbol (map_symbols) and ts_out, which the client never requests
            "*.Symbol",
            "*.TsOutUtc",
        };

        /// <summary>DBN property names that differ from the JSON model's name for the same field.</summary>
        private static readonly Dictionary<string, string> JsonNameOf = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ImbalanceRecordDbn.ContBookClrPrice"] = "ContinuousBookClearingPrice",
            ["ImbalanceRecordDbn.AuctInterestClrPrice"] = "AuctionInterestClearingPrice",
            ["ImbalanceRecordDbn.IndMatchPrice"] = "IndicativeMatchPrice",
        };

        /// <summary>
        /// Returns every disagreement between the two encodings of the same records; empty when they agree.
        /// </summary>
        /// <param name="dbnRecords">Records decoded from DBN.</param>
        /// <param name="jsonRecords">The same request's records decoded from JSON.</param>
        /// <param name="ignored">Further properties to skip, by JSON name, e.g. fields one DBN layout doesn't carry.</param>
        public static IReadOnlyList<string> Compare(IList dbnRecords, IList jsonRecords, params string[] ignored)
        {
            List<string> problems = new List<string>();
            if(dbnRecords.Count != jsonRecords.Count)
            {
                problems.Add($"record count: DBN {dbnRecords.Count}, JSON {jsonRecords.Count}");
                return problems;
            }

            for(int i = 0; i < dbnRecords.Count; ++i)
            {
                CompareObjects(dbnRecords[i], jsonRecords[i], $"[{i}]", new HashSet<string>(ignored, StringComparer.Ordinal), problems);
            }

            // one line per kind of disagreement is enough to act on
            return problems.GroupBy(p => p.Substring(p.IndexOf(' ') + 1)).Select(g => g.First()).ToArray();
        }

        private static void CompareObjects(object dbn, object json, string path, HashSet<string> ignored, List<string> problems)
        {
            Dictionary<string, (object Value, Type Type)> dbnValues = Flatten(dbn, isDbn: true);
            Dictionary<string, (object Value, Type Type)> jsonValues = Flatten(json, isDbn: false);
            string dbnModel = dbn.GetType().Name;
            string jsonModel = json.GetType().Name;

            foreach(string name in dbnValues.Keys.Union(jsonValues.Keys).Where(n => !ignored.Contains(n)))
            {
                bool inDbn = dbnValues.TryGetValue(name, out (object Value, Type Type) d);
                bool inJson = jsonValues.TryGetValue(name, out (object Value, Type Type) j);
                if(!inDbn || !inJson)
                {
                    string model = inDbn ? jsonModel : dbnModel;
                    if(!OneSided.Contains("*." + name) && !OneSided.Contains(model + "." + name))
                    {
                        problems.Add($"{path} {(inDbn ? jsonModel : dbnModel)} has no {name}, which {(inDbn ? dbnModel : jsonModel)} has");
                    }

                    continue;
                }

                if(d.Value is IList dbnList && j.Value is IList jsonList)
                {
                    if(dbnList.Count != jsonList.Count)
                    {
                        problems.Add($"{path} {name}: DBN has {dbnList.Count} items, JSON {jsonList.Count}");
                        continue;
                    }

                    for(int k = 0; k < dbnList.Count; ++k)
                    {
                        CompareObjects(dbnList[k], jsonList[k], $"{path}.{name}[{k}]", ignored, problems);
                    }

                    continue;
                }

                if(!SameValue(d.Value, j.Value))
                {
                    problems.Add($"{path} {dbnModel}.{name} = {Show(d.Value)} but {jsonModel}.{name} = {Show(j.Value)}");
                }
            }
        }

        // A record's comparable values by JSON property name: the JSON header is lifted to the top level as DBN has it,
        // and a single book level is lifted the same way, since some models keep it in a one-element array or a Level object
        private static Dictionary<string, (object Value, Type Type)> Flatten(object record, bool isDbn)
        {
            Dictionary<string, (object Value, Type Type)> values = new Dictionary<string, (object Value, Type Type)>(StringComparer.Ordinal);
            foreach(PropertyInfo property in record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(property.GetIndexParameters().Length > 0 || property.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    continue;
                }

                object value = property.GetValue(record);
                string name = isDbn && JsonNameOf.TryGetValue(record.GetType().Name + "." + property.Name, out string alias) ? alias : property.Name;

                bool liftHeader = name == "Header";
                bool liftLevel = name == "Level" && value != null && IsModel(property.PropertyType);
                bool liftLevels = name == "Levels" && value is IList list && list.Count == 1 && !LevelsStayAList(record);
                if(liftHeader || liftLevel || liftLevels)
                {
                    object inner = liftLevels ? ((IList)value)[0] : value;
                    if(inner != null)
                    {
                        foreach(KeyValuePair<string, (object Value, Type Type)> lifted in Flatten(inner, isDbn))
                        {
                            values[lifted.Key] = lifted.Value;
                        }
                    }

                    continue;
                }

                // JSON computed conveniences over Levels (e.g. Level1) are not fields
                if(!isDbn && property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>() == null && property.Name != "Header")
                {
                    continue;
                }

                values[name] = (value, property.PropertyType);
            }

            return values;
        }

        // mbp-10 keeps its ten levels as a list in both models; every other schema has one level, lifted to the top
        private static bool LevelsStayAList(object record) => record.GetType().Name.StartsWith("Mbp10", StringComparison.Ordinal);

        private static bool IsModel(Type type) => type.IsClass && type != typeof(string);

        private static bool SameValue(object dbn, object json)
        {
            if(dbn == null || json == null)
            {
                return IsUnset(dbn) && IsUnset(json);
            }

            if(dbn is double || dbn is float || json is double || json is float)
            {
                double a = Convert.ToDouble(dbn, CultureInfo.InvariantCulture);
                double b = Convert.ToDouble(json, CultureInfo.InvariantCulture);
                return (double.IsNaN(a) && double.IsNaN(b)) || Math.Abs(a - b) <= 1e-9 * Math.Max(1.0, Math.Abs(a));
            }

            if(dbn is DateTime || dbn is DateTimeOffset || json is DateTime || json is DateTimeOffset)
            {
                return ToUtc(dbn) == ToUtc(json);
            }

            // a status flag the JSON encoding sends as "Y", "N" or "~" (not available); the DBN model's bool reads "~" as false
            if(dbn is bool flag && json is string yesNo)
            {
                return flag ? yesNo == "Y" : yesNo == "N" || yesNo == "~";
            }

            // a character code: a char, a one-letter string, or an enum whose value is the character
            if(IsCharLike(dbn) || IsCharLike(json))
            {
                return CharOf(dbn) == CharOf(json);
            }

            if(dbn is Enum || json is Enum || IsNumber(dbn) || IsNumber(json))
            {
                return Convert.ToDecimal(dbn, CultureInfo.InvariantCulture) == Convert.ToDecimal(json, CultureInfo.InvariantCulture);
            }

            return Equals(dbn, json);
        }

        private static bool IsUnset(object value) => value == null || (value is string s && s.Length == 0);

        private static bool IsNumber(object value) => value is byte || value is sbyte || value is short || value is ushort || value is int
            || value is uint || value is long || value is ulong || value is decimal;

        private static bool IsCharLike(object value) => value is char || (value is string s && s.Length == 1);

        private static string CharOf(object value)
        {
            switch(value)
            {
                case char c:
                    return c.ToString();
                case string s:
                    return s;
                case Enum e:
                    return ((char)Convert.ToInt32(e, CultureInfo.InvariantCulture)).ToString();
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private static DateTime? ToUtc(object value) => value switch
        {
            DateTime dt => dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            DateTimeOffset dto => dto.UtcDateTime,
            _ => null,
        };

        private static string Show(object value) => value switch
        {
            null => "null",
            string s => "\"" + s + "\"",
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };
    }
}
