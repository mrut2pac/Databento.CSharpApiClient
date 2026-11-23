namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>The compression a batch job writes its output files with.</summary>
    public enum BatchCompression
    {
        /// <summary>Uncompressed (<c>"none"</c>).</summary>
        None,

        /// <summary>Zstandard (<c>"zstd"</c>), the API's default.</summary>
        Zstd,
    }
}
