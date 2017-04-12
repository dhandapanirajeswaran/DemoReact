using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Index()
        {
            if (!CanUserViewDiagnostics())
                return AccessDenied();
            var daysAgo = 14;
            var model = _serviceFacade.GetDiagnostics(daysAgo);
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(DiagnosticsSettingsViewModel model)
        {
            if (!CanUserViewDiagnostics())
                return AccessDenied();
            _serviceFacade.UpdateDiagnosticsSettings(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ClearLog()
        {
            if (!CanUserViewDiagnostics())
                return AccessDenied();
            _serviceFacade.ClearDiagnosticsLog();
            return RedirectToAction("Index");
        }

        #region private methods
        private bool CanUserViewDiagnostics()
        {
            var userAccess = base.GetUserAccessModel();
            return userAccess.IsUserAuthenticated 
                && userAccess.UserDiagnosticsAccess.CanView;
        }

        private ActionResult AccessDenied()
        {
            return base.AccessDenied("You do not have access to the Diagnostics section");
        }

        #endregion
    }
}