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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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
        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult Index()
        {
            var model = _serviceFacade.GetSystemSettings();
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.Edit)]
        public ActionResult Index(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                model = _serviceFacade.UpdateSystemSettings(model);
            }
            return View(model);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult DriveTime()
        {
            var model = _serviceFacade.GetAllDriveTimeMarkups();
            return View(model);
        }

        [ValidateInput(false)]
        [System.Web.Mvc.HttpPost]
        //[AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult UpdateDriveTimeMarkups(List<DriveTimeMarkupViewModel> model)
        {
            var result = _serviceFacade.UpdateDriveTimeMarkups(model);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult GetDriveTimeMarkupsJson()
        {
            var result = _serviceFacade.GetAllDriveTimeMarkups();

            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult Brands()
        {
            var model = _serviceFacade.GetBrandSettings();
            return View(model);
        }

        [ValidateInput(false)]
        [System.Web.Mvc.HttpPost]
        //[AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult UpdateBrandSettings(BrandsSettingsUpdateViewModel model)
        {
            var result = _serviceFacade.UpdateBrandSettings(model);
            return base.JsonGetResult(result);
        }
    }
}