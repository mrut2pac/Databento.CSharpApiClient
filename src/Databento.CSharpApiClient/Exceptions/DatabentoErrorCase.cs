namespace Databento.CSharpApiClient.Exceptions
{
    /// <summary>
    /// Strongly-typed mapping of the Databento error envelope's <c>detail.case</c> field.
    /// Lets callers branch on a known case instead of substring-matching the response body.
    /// The raw wire string is always preserved on <see cref="DatabentoHttpException.ErrorCase"/>;
    /// unrecognised cases map to <see cref="Unknown"/>.
    /// </summary>
    public enum DatabentoErrorCase
    {
        /// <summary>
        /// The response carried no <c>detail.case</c>, or its value is not one this client version recognises.
        /// Inspect <see cref="DatabentoHttpException.ErrorCase"/> for the raw string.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// <c>symbology_invalid_request</c> — none of the requested symbols could be resolved for the
        /// dataset and time range (the symbols are well-formed but have no instrument mapping).
        /// </summary>
        SymbologyInvalidRequest,

        /// <summary>
        /// <c>symbology_invalid_symbol</c> — a requested symbol is malformed for the dataset's symbology
        /// (e.g. not valid OCC format for an options dataset).
        /// </summary>
        SymbologyInvalidSymbol,

        /// <summary>
        /// <c>data_start_after_available_end</c> — the query <c>start</c> is later than the newest data
        /// available for the dataset.
        /// </summary>
        DataStartAfterAvailableEnd,

        /// <summary>
        /// <c>data_end_after_available_end</c> — the query <c>end</c> is later than the newest data
        /// available for the dataset (e.g. requesting the current day before the bars have settled).
        /// </summary>
        DataEndAfterAvailableEnd,

        /// <summary>
        /// <c>data_schema_not_fully_available</c> — the requested schema is only available for part of the
        /// query range (e.g. high-granularity schemas with a shorter retention window).
        /// </summary>
        DataSchemaNotFullyAvailable,

        /// <summary>
        /// <c>license_not_found_unauthorized</c> — the API key is not licensed/entitled for the requested
        /// dataset.
        /// </summary>
        LicenseNotFoundUnauthorized,
    }
}
