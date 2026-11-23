namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>The encoding a batch job writes its output in.</summary>
    public enum BatchEncoding
    {
        /// <summary>Databento Binary Encoding (<c>"dbn"</c>), the API's default.</summary>
        Dbn,

        /// <summary>Comma-separated values (<c>"csv"</c>).</summary>
        Csv,

        /// <summary>One JSON object per line (<c>"json"</c>).</summary>
        Json,
    }
}
