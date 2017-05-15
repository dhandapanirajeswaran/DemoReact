using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Portal.ActionFilters;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class SettingsController : BaseController
    {

        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public SettingsController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        // GET: Settings
        [HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult Index()
        {
            var model = _serviceFacade.GetSystemSettings();
            return View(model);
        }

        [HttpPost]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.Edit)]
        public ActionResult Index(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                model = _serviceFacade.UpdateSystemSettings(model);
            }
            return View(model);
        }
    }
}