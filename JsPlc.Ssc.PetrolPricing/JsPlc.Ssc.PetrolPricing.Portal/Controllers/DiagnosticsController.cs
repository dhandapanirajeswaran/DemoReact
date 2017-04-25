using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Portal.ActionFilters;
using JsPlc.Ssc.PetrolPricing.Models.Enums;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class DiagnosticsController : BaseController
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public DiagnosticsController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        [HttpGet]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.View)]
        public ActionResult Index(string message = "")
        {
            var daysAgo = 14;
            var model = _serviceFacade.GetDiagnostics(daysAgo);
            model.ActionMessage = message;
            return View(model);
        }

        [HttpPost]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.View | DiagnosticsUserPermissions.Edit)]
        public ActionResult Index(DiagnosticsSettingsViewModel model)
        {
            _serviceFacade.UpdateDiagnosticsSettings(model);
            return RedirectToAction("Index", new { message = "Diagnostic Settings Updated" });
        }

        [HttpPost]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.View)]
        public ActionResult ClearLog()
        {
            _serviceFacade.ClearDiagnosticsLog();
            return RedirectToAction("Index", new { message = "Diagnostics Log Cleared" });
        }
    }
}