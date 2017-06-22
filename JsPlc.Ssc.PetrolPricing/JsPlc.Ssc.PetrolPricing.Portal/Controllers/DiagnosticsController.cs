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
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Business;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class DiagnosticsController : BaseController
    {
        private readonly IAppSettings _appSettings;
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        private const string appLogFilePath = @"C:\Websites\pplog";

        public DiagnosticsController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
            _appSettings = new AppSettings();
        }

        [HttpGet]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.View)]
        public ActionResult Index(string message = "")
        {
            var daysAgo = 7;
            var model = _serviceFacade.GetDiagnostics(daysAgo, appLogFilePath);
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

        [HttpPost]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.ResetDatabase)] 
        public ActionResult DeleteAllData()
        {
            var success = _serviceFacade.DeleteAllData();
            if (success)
                return RedirectToAction("Index", new { message = "All Database Data Deleted" });
            else
                return RedirectToAction("Index", new { message = "Error - Unable to delete data!" });
        }

        [HttpGet]
        [AuthoriseDiagnostics(Permissions = DiagnosticsUserPermissions.View | DiagnosticsUserPermissions.Edit)]
        public async Task<ActionResult> DownloadErrorLogFile(string filename)
        {
            var file = await _serviceFacade.GetDiagnosticsErrorLogFile(appLogFilePath, filename);
            if (file.FileBytes != null)
                return File(file.FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.FileName);
            return new RedirectResult("?msg=Unable to find file");
        }
    }
}