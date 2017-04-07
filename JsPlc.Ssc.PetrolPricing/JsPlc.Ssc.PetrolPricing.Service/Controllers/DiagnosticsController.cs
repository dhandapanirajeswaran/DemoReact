using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using System;
using System.Web.Http;
using System.Web.Http.Results;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class DiagnosticsController : ApiController
    {
        private IDiagnosticsService _diagnosticService;
        private ILogger _logger;

        public DiagnosticsController(IDiagnosticsService diagnosticsService)
        {
            _diagnosticService = diagnosticsService;
            _logger = new PetrolPricingLogger();
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/GetDiagnostics/")]
        public IHttpActionResult GetDiagnostics([FromUri] int daysAgo)
        {
            try
            {
                var result = _diagnosticService.GetDiagnostics(daysAgo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }
    }
}