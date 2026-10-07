using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Databento.CSharpApiClient.DataModel;
using Databento.CSharpApiClient.DataModel.Batch;
using Databento.CSharpApiClient.DataModel.Json;
using Databento.CSharpApiClient.DataModel.Metadata;
using Databento.CSharpApiClient.DataModel.Symbology;

namespace Databento.CSharpApiClient.IntegrationTests
{
    /// <summary>
    /// Compares a live response with the model the client reads it into, key by key and in both directions.
    /// A key the API sends that no property maps, and a mapped property that no object carries, both read as
    /// silent nulls at runtime; this turns them into test failures.
    /// </summary>
    internal static class ResponseModelGuard
    {
        /// <summary>
        /// Mapped keys that legitimately don't arrive, as <c>Model.json_key</c> (or <c>*.json_key</c> for every model), with the reason.
        /// </summary>
        private static readonly Dictionary<string, string> OptionalKeys = new Dictionary<string, string>
        {
            ["*.ts_out"] = "sent only when ts_out is requested, which the client never does",
            ["DatasetCondition.dataset"] = "not sent; the client fills it in from the request",
            ["DefinitionRecordJson.trading_reference_price"] = DbnV1Only,
            ["DefinitionRecordJson.trading_reference_date"] = DbnV1Only,
            ["DefinitionRecordJson.md_security_trading_status"] = DbnV1Only,
            ["DefinitionRecordJson.settl_price_type"] = DbnV1Only,
            ["DefinitionRecordJson.leg_count"] = DbnV3Only,
            ["DefinitionRecordJson.leg_index"] = DbnV3Only,
            ["DefinitionRecordJson.leg_instrument_id"] = DbnV3Only,
            ["DefinitionRecordJson.leg_raw_symbol"] = DbnV3Only,
            ["DefinitionRecordJson.leg_instrument_class"] = DbnV3Only,
            ["DefinitionRecordJson.leg_side"] = DbnV3Only,
            ["DefinitionRecordJson.leg_price"] = DbnV3Only,
            ["DefinitionRecordJson.leg_delta"] = DbnV3Only,
            ["DefinitionRecordJson.leg_ratio_price_numerator"] = DbnV3Only,
            ["DefinitionRecordJson.leg_ratio_price_denominator"] = DbnV3Only,
            ["DefinitionRecordJson.leg_ratio_qty_numerator"] = DbnV3Only,
            ["DefinitionRecordJson.leg_ratio_qty_denominator"] = DbnV3Only,
            ["DefinitionRecordJson.leg_underlying_id"] = DbnV3Only,
        };

        private const string DbnV1Only = "only in the DBN v1 definition layout (e.g. OPRA.PILLAR, XNAS.ITCH)";

        private const string DbnV3Only = "only in the DBN v3 definition layout (e.g. GLBX.MDP3)";

        private static readonly Dictionary<string, Type> TypeBySchema = new Dictionary<string, Type>
        {
            [Schema.Mbo] = typeof(MboRecordJson),
            [Schema.Mbp1] = typeof(Mbp1RecordJson),
            [Schema.Mbp10] = typeof(Mbp10RecordJson),
            [Schema.Tbbo] = typeof(TbboRecordJson),
            [Schema.Trades] = typeof(TradeRecordJson),
            [Schema.Bbo1Sec] = typeof(BboRecordJson),
            [Schema.Bbo1Min] = typeof(BboRecordJson),
            [Schema.Tcbbo] = typeof(TcbboRecordJson),
            [Schema.Cmbp1] = typeof(Cmbp1RecordJson),
            [Schema.ConsolidatedBBO1Sec] = typeof(CbboRecordJson),
            [Schema.ConsolidatedBBO1Min] = typeof(CbboRecordJson),
            [Schema.Ohlcv1Sec] = typeof(OhlcvRecordJson),
            [Schema.Ohlcv1Min] = typeof(OhlcvRecordJson),
            [Schema.Ohlcv1Hour] = typeof(OhlcvRecordJson),
            [Schema.Ohlcv1Day] = typeof(OhlcvRecordJson),
            [Schema.Definition] = typeof(DefinitionRecordJson),
            [Schema.Statistics] = typeof(StatisticsRecordJson),
            [Schema.Status] = typeof(StatusRecordJson),
            [Schema.Imbalance] = typeof(ImbalanceRecordJson),
        };

        private static readonly Dictionary<string, Type> TypeByEndpoint = new Dictionary<string, Type>
        {
            ["metadata.list_publishers"] = typeof(PublisherInfo),
            ["metadata.list_fields"] = typeof(DataModel.Metadata.FieldInfo),
            ["metadata.list_unit_prices"] = typeof(UnitPriceInfo),
            ["metadata.get_dataset_condition"] = typeof(DatasetCondition),
            ["metadata.get_dataset_range"] = typeof(DateRange),
            ["symbology.resolve"] = typeof(SymbologyResolution),
            ["batch.submit_job"] = typeof(BatchJob),
            ["batch.list_jobs"] = typeof(BatchJob),
            ["batch.get_job_details"] = typeof(BatchJob),
            ["batch.list_files"] = typeof(BatchFile),
        };

        /// <summary>Endpoints whose responses are plain values (strings, numbers) with no model to check.</summary>
        private static readonly HashSet<string> UnmodelledEndpoints = new HashSet<string>(StringComparer.Ordinal)
        {
            "metadata.list_datasets",
            "metadata.list_schemas",
            "metadata.get_record_count",
            "metadata.get_billable_size",
            "metadata.get_cost",
        };

        private static readonly string[] ApiEndpointPrefixes = { "metadata.", "timeseries.", "symbology.", "batch." };

        /// <summary>Returns every mismatch between the captured response and its model; empty when they agree.</summary>
        /// <param name="response">A response captured by <see cref="CapturingHttpTransport"/>.</param>
        public static IReadOnlyList<string> Check(CapturedResponse response)
        {
            string endpoint = response.RequestUri.AbsolutePath.Substring(response.RequestUri.AbsolutePath.LastIndexOf('/') + 1);
            if(!ApiEndpointPrefixes.Any(prefix => endpoint.StartsWith(prefix, StringComparison.Ordinal)) || UnmodelledEndpoints.Contains(endpoint))
            {
                // a batch file download, or a plain value
                return Array.Empty<string>();
            }

            // a response the guard can't read must fail loudly, or the guard turns itself off without anyone noticing
            if(response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) != true)
            {
                return new[] { $"unchecked: {endpoint} answered with content type \"{response.ContentType}\", not JSON" };
            }

            string schema = ParameterValue(response, "schema");
            Type model = null;
            if(endpoint == "timeseries.get_range")
            {
                if(schema != null && TypeBySchema.TryGetValue(schema, out Type recordType))
                {
                    model = recordType;
                }
            }
            else if(TypeByEndpoint.TryGetValue(endpoint, out Type type))
            {
                model = type;
            }

            if(model == null)
            {
                return new[] { $"unchecked: no model is registered for {endpoint}" + (schema == null ? string.Empty : " with schema " + schema) };
            }

            List<JsonElement> objects = new List<JsonElement>();
            foreach(string record in RecordsOf(response.Body))
            {
                using JsonDocument doc = JsonDocument.Parse(record);
                CollectObjects(doc.RootElement.Clone(), objects);
            }

            List<string> problems = new List<string>();
            bool symbolsMapped = ParameterValue(response, "map_symbols") == "true";
            CheckObjects(model, objects, symbolsMapped, problems);
            return problems.Distinct().Select(problem => problem + " (" + endpoint + ")").ToArray();
        }

        // A parameter from the query string, or from the form body of a POST
        private static string ParameterValue(CapturedResponse response, string name)
            => QueryValue(response.RequestUri.Query, name) ?? QueryValue(response.RequestBody, name);

        private static string QueryValue(string query, string name)
        {
            foreach(string pair in query.TrimStart('?').Split('&'))
            {
                int eq = pair.IndexOf('=');
                if(eq > 0 && pair.Substring(0, eq) == name)
                {
                    return Uri.UnescapeDataString(pair.Substring(eq + 1));
                }
            }

            return null;
        }

        // A metadata body is a single JSON value; a timeseries body is one JSON object per line
        private static IEnumerable<string> RecordsOf(string body)
        {
            try
            {
                using JsonDocument whole = JsonDocument.Parse(body);
                return new[] { body };
            }
            catch(JsonException)
            {
                return body.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line));
            }
        }

        private static void CollectObjects(JsonElement element, List<JsonElement> objects)
        {
            if(element.ValueKind == JsonValueKind.Array)
            {
                foreach(JsonElement item in element.EnumerateArray())
                {
                    CollectObjects(item, objects);
                }
            }
            else if(element.ValueKind == JsonValueKind.Object)
            {
                objects.Add(element);
            }
        }

        private static void CheckObjects(Type model, List<JsonElement> objects, bool symbolsMapped, List<string> problems)
        {
            if(objects.Count == 0)
            {
                return;
            }

            // the client deserializes with PropertyNameCaseInsensitive, so keys match the same way
            Dictionary<string, PropertyInfo> mapped = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach(PropertyInfo property in model.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                JsonPropertyNameAttribute name = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                if(name != null)
                {
                    mapped[name.Name] = property;
                }
            }

            // every key the API sends has a property
            foreach(JsonElement obj in objects)
            {
                foreach(JsonProperty key in obj.EnumerateObject())
                {
                    if(!mapped.ContainsKey(key.Name))
                    {
                        problems.Add($"unmapped key: {model.Name} has no property for \"{key.Name}\"");
                    }
                }
            }

            // every live property arrives on at least one object, and its nested objects match their own model
            foreach(KeyValuePair<string, PropertyInfo> entry in mapped)
            {
                List<JsonElement> values = new List<JsonElement>();
                foreach(JsonElement obj in objects)
                {
                    foreach(JsonProperty key in obj.EnumerateObject())
                    {
                        if(string.Equals(key.Name, entry.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            values.Add(key.Value);
                        }
                    }
                }

                if(values.Count == 0)
                {
                    bool obsolete = entry.Value.GetCustomAttribute<ObsoleteAttribute>() != null;
                    bool optional = OptionalKeys.ContainsKey("*." + entry.Key) || OptionalKeys.ContainsKey(model.Name + "." + entry.Key);
                    bool unrequestedSymbol = entry.Key == "symbol" && !symbolsMapped;
                    if(!obsolete && !optional && !unrequestedSymbol)
                    {
                        problems.Add($"never sent: {model.Name}.{entry.Value.Name} maps \"{entry.Key}\", which no object carries");
                    }

                    continue;
                }

                Type nested = NestedModel(entry.Value.PropertyType, out bool isDictionary);
                if(nested != null)
                {
                    List<JsonElement> nestedObjects = new List<JsonElement>();
                    foreach(JsonElement value in values)
                    {
                        if(isDictionary && value.ValueKind == JsonValueKind.Object)
                        {
                            foreach(JsonProperty item in value.EnumerateObject())
                            {
                                CollectObjects(item.Value, nestedObjects);
                            }
                        }
                        else
                        {
                            CollectObjects(value, nestedObjects);
                        }
                    }

                    CheckObjects(nested, nestedObjects, symbolsMapped, problems);
                }
            }
        }

        // The model a property's JSON objects deserialize into, or null for a scalar or a map of scalars
        private static Type NestedModel(Type type, out bool isDictionary)
        {
            isDictionary = false;
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                isDictionary = true;
                type = type.GetGenericArguments()[1];
            }

            if(type.IsArray)
            {
                type = type.GetElementType();
            }
            else if(type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                type = type.GetGenericArguments()[0];
            }

            bool isModel = type.IsClass && type != typeof(string) && type.Namespace?.StartsWith("Databento.", StringComparison.Ordinal) == true;
            return isModel ? type : null;
        }
    }
}
