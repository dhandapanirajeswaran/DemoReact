using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
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

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("api/UpdateDiagnosticsSettings")]
        public IHttpActionResult UpdateDiagnosticsSettings([FromBody] DiagnosticsSettingsViewModel settings)
        {
            try
            {
                var result = _diagnosticService.UpdateDiagnosticsSettings(settings);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/ClearDiagnosticsLog")]
        public IHttpActionResult ClearDiagnosticsLog()
        {
            try
            {
                var result = _diagnosticService.ClearDiagnosticsLog();
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