using Databento.CSharpApiClient.Exceptions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Databento.CSharpApiClient.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="DatabentoHttpException"/> — error-envelope parsing and the
    /// raw-case to <see cref="DatabentoErrorCase"/> mapping.
    /// </summary>
    [TestClass]
    public class DatabentoHttpExceptionTests
    {
        private const string SymbologyInvalidRequestBody =
            "{\"detail\":{\"case\":\"symbology_invalid_request\",\"message\":\"None of the symbols could be resolved\"}}";
        private const string DataEndAfterAvailableEndBody =
            "{\"detail\":{\"case\":\"data_end_after_available_end\",\"message\":\"The `end` in the query is after the available range.\"}}";
        private const string DataSchemaNotFullyAvailableBody =
            "{\"detail\":{\"case\":\"data_schema_not_fully_available\",\"message\":\"Schema `cbbo-1s` for dataset `OPRA.PILLAR` is only available between ...\"}}";

        [TestMethod]
        public void Create_SymbologyInvalidRequest_MapsCodeAndPreservesRawCase()
        {
            DatabentoHttpException ex = DatabentoHttpException.Create(422, SymbologyInvalidRequestBody);

            Assert.AreEqual(422, ex.StatusCode);
            Assert.AreEqual("symbology_invalid_request", ex.ErrorCase);
            Assert.AreEqual(DatabentoErrorCase.SymbologyInvalidRequest, ex.Code);
            Assert.AreEqual(SymbologyInvalidRequestBody, ex.ResponseBody);
        }

        [TestMethod]
        public void Create_DataEndAfterAvailableEnd_MapsCode()
        {
            DatabentoHttpException ex = DatabentoHttpException.Create(422, DataEndAfterAvailableEndBody);

            Assert.AreEqual(DatabentoErrorCase.DataEndAfterAvailableEnd, ex.Code);
        }

        [TestMethod]
        public void Create_DataSchemaNotFullyAvailable_MapsCode()
        {
            DatabentoHttpException ex = DatabentoHttpException.Create(422, DataSchemaNotFullyAvailableBody);

            Assert.AreEqual(DatabentoErrorCase.DataSchemaNotFullyAvailable, ex.Code);
        }

        [TestMethod]
        public void Create_NonJsonBody_LeavesCaseNullAndCodeUnknown()
        {
            DatabentoHttpException ex = DatabentoHttpException.Create(500, "Internal Server Error");

            Assert.AreEqual(500, ex.StatusCode);
            Assert.IsNull(ex.ErrorCase);
            Assert.AreEqual(DatabentoErrorCase.Unknown, ex.Code);
        }

        [TestMethod]
        public void Create_NullBody_DoesNotThrowAndYieldsUnknown()
        {
            DatabentoHttpException ex = DatabentoHttpException.Create(403, null);

            Assert.AreEqual(403, ex.StatusCode);
            Assert.AreEqual(string.Empty, ex.ResponseBody);
            Assert.AreEqual(DatabentoErrorCase.Unknown, ex.Code);
        }

        [TestMethod]
        public void Create_UnrecognisedCase_PreservesRawAndMapsUnknown()
        {
            string body = "{\"detail\":{\"case\":\"some_future_case\",\"message\":\"x\"}}";

            DatabentoHttpException ex = DatabentoHttpException.Create(400, body);

            Assert.AreEqual("some_future_case", ex.ErrorCase);
            Assert.AreEqual(DatabentoErrorCase.Unknown, ex.Code);
        }

        [TestMethod]
        public void MapErrorCase_AllKnownCases_MapToDistinctMembers()
        {
            Assert.AreEqual(DatabentoErrorCase.SymbologyInvalidRequest, DatabentoHttpException.MapErrorCase("symbology_invalid_request"));
            Assert.AreEqual(DatabentoErrorCase.SymbologyInvalidSymbol, DatabentoHttpException.MapErrorCase("symbology_invalid_symbol"));
            Assert.AreEqual(DatabentoErrorCase.DataStartAfterAvailableEnd, DatabentoHttpException.MapErrorCase("data_start_after_available_end"));
            Assert.AreEqual(DatabentoErrorCase.DataEndAfterAvailableEnd, DatabentoHttpException.MapErrorCase("data_end_after_available_end"));
            Assert.AreEqual(DatabentoErrorCase.DataSchemaNotFullyAvailable, DatabentoHttpException.MapErrorCase("data_schema_not_fully_available"));
            Assert.AreEqual(DatabentoErrorCase.LicenseNotFoundUnauthorized, DatabentoHttpException.MapErrorCase("license_not_found_unauthorized"));
            Assert.AreEqual(DatabentoErrorCase.Unknown, DatabentoHttpException.MapErrorCase(null));
        }
    }
}
