namespace Databento.CSharpApiClient.DataModel.Batch
{
    /// <summary>The longest period one output file of a batch job covers. Periods are UTC, and a week starts on Sunday.</summary>
    public enum BatchSplitDuration
    {
        /// <summary>One file per day (<c>"day"</c>), the API's default.</summary>
        Day,

        /// <summary>One file per week (<c>"week"</c>).</summary>
        Week,

        /// <summary>One file per month (<c>"month"</c>).</summary>
        Month,

        /// <summary>One file per year (<c>"year"</c>).</summary>
        Year,

        /// <summary>No split by period (<c>"none"</c>).</summary>
        None,
    }
}
