namespace Databento.CSharpApiClient.DataModel
{
    public enum OrderBookAction : byte
    {
        #pragma warning disable IDE0055
        Unknown =    0,
        Add =        (byte)'A',
        Modify =     (byte)'M',
        Delete =     (byte)'D',
        Reset =      (byte)'R',
        Update =     (byte)'U',
        Fill =       (byte)'F',
        Trade =      (byte)'T',
    }
}
