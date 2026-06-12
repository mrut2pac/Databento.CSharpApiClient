using System;
using System.Text.Json;

namespace Databento.CSharpApiClient.Exceptions
{
    /// <summary>
    /// Thrown when the Databento Historical API returns a non-success HTTP status code. Carries the HTTP
    /// status, the raw response body, and the error envelope's <c>detail.case</c> in both raw
    /// (<see cref="ErrorCase"/>) and strongly-typed (<see cref="Code"/>) form, so callers can branch on a
    /// known case instead of substring-matching the message.
    /// </summary>
    public sealed class DatabentoHttpException : Exception
    {
        /// <summary>HTTP status code returned by the API (e.g. 400, 403, 422, 429, 500).</summary>
        public int StatusCode { get; }

        /// <summary>Raw response body, truncated to 512 characters when very long.</summary>
        public string ResponseBody { get; }

        /// <summary>
        /// Machine-readable error case from the Databento error envelope, e.g.
        /// <c>"data_end_after_available_end"</c> or <c>"license_not_found_unauthorized"</c>.
        /// <see langword="null"/> when the response body is not a structured JSON error object.
        /// </summary>
        public string ErrorCase { get; }

        /// <summary>
        /// Strongly-typed form of <see cref="ErrorCase"/>. <see cref="DatabentoErrorCase.Unknown"/> when
        /// the case is absent or not recognised by this client version (the raw string is still on
        /// <see cref="ErrorCase"/>).
        /// </summary>
        public DatabentoErrorCase Code { get; }

        /// <summary>
        /// Initialises a new instance with the given HTTP status, body, and optional error case.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="responseBody">Response body text.</param>
        /// <param name="errorCase">Raw machine-readable error case, or <see langword="null"/>.</param>
        public DatabentoHttpException(int statusCode, string responseBody, string errorCase = null)
            : base(BuildMessage(statusCode, responseBody, errorCase))
        {
            this.StatusCode = statusCode;
            this.ResponseBody = responseBody;
            this.ErrorCase = errorCase;
            this.Code = MapErrorCase(errorCase);
        }

        /// <summary>
        /// Builds the exception for the given HTTP status, parsing the error case out of the response body.
        /// Both clients route every HTTP failure through this factory.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="responseBody">Raw response body (may be <see langword="null"/>).</param>
        /// <returns>A populated <see cref="DatabentoHttpException"/>.</returns>
        public static DatabentoHttpException Create(int statusCode, string responseBody)
        {
            string body = responseBody ?? string.Empty;
            return new DatabentoHttpException(statusCode, body, ExtractErrorCase(body));
        }

        /// <summary>
        /// Extracts the <c>detail.case</c> value from a Databento JSON error envelope, or
        /// <see langword="null"/> when the body is not a structured error.
        /// </summary>
        /// <param name="body">Raw response body.</param>
        /// <returns>The error case string, or <see langword="null"/>.</returns>
        public static string ExtractErrorCase(string body)
        {
            if(string.IsNullOrEmpty(body))
            {
                return null;
            }

            try
            {
                using(JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement detail;
                    if(!doc.RootElement.TryGetProperty("detail", out detail))
                    {
                        return null;
                    }

                    if(detail.ValueKind == JsonValueKind.Object)
                    {
                        JsonElement caseEl;
                        if(detail.TryGetProperty("case", out caseEl))
                        {
                            return caseEl.GetString();
                        }
                    }
                }
            }
            catch(JsonException)
            {
                // Not a structured JSON error.
            }

            return null;
        }

        /// <summary>
        /// Maps a raw <c>detail.case</c> string to its <see cref="DatabentoErrorCase"/>, returning
        /// <see cref="DatabentoErrorCase.Unknown"/> for absent or unrecognised values.
        /// </summary>
        /// <param name="errorCase">Raw error case string, or <see langword="null"/>.</param>
        /// <returns>The mapped enum value.</returns>
        public static DatabentoErrorCase MapErrorCase(string errorCase)
        {
            switch(errorCase)
            {
                case "symbology_invalid_request":
                    return DatabentoErrorCase.SymbologyInvalidRequest;
                case "symbology_invalid_symbol":
                    return DatabentoErrorCase.SymbologyInvalidSymbol;
                case "data_start_after_available_end":
                    return DatabentoErrorCase.DataStartAfterAvailableEnd;
                case "data_end_after_available_end":
                    return DatabentoErrorCase.DataEndAfterAvailableEnd;
                case "data_schema_not_fully_available":
                    return DatabentoErrorCase.DataSchemaNotFullyAvailable;
                case "license_not_found_unauthorized":
                    return DatabentoErrorCase.LicenseNotFoundUnauthorized;
                default:
                    return DatabentoErrorCase.Unknown;
            }
        }

        private static string BuildMessage(int statusCode, string body, string errorCase)
        {
            string msg = "HTTP " + statusCode;

            if(!string.IsNullOrEmpty(errorCase))
            {
                msg += " [" + errorCase + "]";
            }

            if(!string.IsNullOrEmpty(body))
            {
                string truncated = body.Length > 512 ? string.Concat(body.AsSpan(0, 512), "...") : body;
                msg += ": " + truncated;
            }

            return msg;
        }
    }
}
