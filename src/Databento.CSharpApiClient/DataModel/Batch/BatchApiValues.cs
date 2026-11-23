using System;

namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>
    /// The values <c>batch.submit_job</c> takes for the batch enums, as Databento's own clients send them. A value outside the enum
    /// throws <see cref="ArgumentOutOfRangeException"/> naming <paramref name="paramName"/>, so it never reaches the API as text it
    /// doesn't know.
    /// </summary>
    internal static class BatchApiValues
    {
        public static string ToApiValue(this BatchEncoding encoding, string paramName)
        {
            return encoding switch
            {
                BatchEncoding.Dbn => "dbn",
                BatchEncoding.Csv => "csv",
                BatchEncoding.Json => "json",
                _ => throw new ArgumentOutOfRangeException(paramName, encoding, "Encoding is not a BatchEncoding."),
            };
        }

        public static string ToApiValue(this BatchCompression compression, string paramName)
        {
            return compression switch
            {
                BatchCompression.None => "none",
                BatchCompression.Zstd => "zstd",
                _ => throw new ArgumentOutOfRangeException(paramName, compression, "Compression is not a BatchCompression."),
            };
        }

        public static string ToApiValue(this BatchSplitDuration splitDuration, string paramName)
        {
            return splitDuration switch
            {
                BatchSplitDuration.Day => "day",
                BatchSplitDuration.Week => "week",
                BatchSplitDuration.Month => "month",
                BatchSplitDuration.Year => "year",
                BatchSplitDuration.None => "none",
                _ => throw new ArgumentOutOfRangeException(paramName, splitDuration, "SplitDuration is not a BatchSplitDuration."),
            };
        }
    }
}
