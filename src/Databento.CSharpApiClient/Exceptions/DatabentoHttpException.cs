using System;

namespace Databento.CSharpApiClient.Exceptions
{
    public sealed class DatabentoHttpException : Exception
    {
        public int StatusCode { get; }

        public string ResponseBody { get; }

        /// <summary>
        /// The machine-readable error case from the Databento API, e.g. "data_end_after_available_end".
        /// Null when the response body is not a parseable JSON error object.
        /// </summary>
        public string ErrorCase { get; }

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
