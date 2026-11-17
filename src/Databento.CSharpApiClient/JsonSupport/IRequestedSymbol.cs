namespace Databento.CSharpApiClient.JsonSupport
{
    /// <summary>
    /// A JSON record that names the symbol it belongs to, which the client fills from the request when the response does not.
    /// </summary>
    internal interface IRequestedSymbol
    {
        string Symbol { get; set; }
    }
}
