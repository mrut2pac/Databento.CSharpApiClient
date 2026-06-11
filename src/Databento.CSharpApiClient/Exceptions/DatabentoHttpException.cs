using System;

namespace Databento.CSharpApiClient.Exceptions
{
    /// <summary>
    /// Thrown when the Databento Historical API returns a non-success HTTP status code.
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
        /// Initialises a new instance with the given HTTP status, body, and optional error case.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="responseBody">Response body text.</param>
        /// <param name="errorCase">Machine-readable error case, or <see langword="null"/>.</param>
        public DatabentoHttpException(int statusCode, string responseBody, string errorCase = null)
            : base(BuildMessage(statusCode, responseBody, errorCase))
        {
            this.StatusCode = statusCode;
            this.ResponseBody = responseBody;
            this.ErrorCase = errorCase;
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
