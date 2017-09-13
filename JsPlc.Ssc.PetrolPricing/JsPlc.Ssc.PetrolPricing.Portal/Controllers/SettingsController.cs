using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;
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


        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult Schedule(NotifyMessageType notify = NotifyMessageType.None)
        {
            var model = new ScheduleViewModel()
            {
                NotifyMessage = NotifyMessages.MessageFor(notify),
                NotifyClass = NotifyMessages.ClassFor(notify, ""),
                NotifyMessageType = notify,
                ScheduledItems = _serviceFacade.GetWinServiceScheduledItems(),
                EventLogItems = _serviceFacade.GetWinServiceEventLog()
            };
            return View(model);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult LoadEmailScheduleItem(int winScheduleId)
        {
            var result = _serviceFacade.WinServiceGetScheduleItem(winScheduleId);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpPost]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult SaveEmailScheduleItem(ScheduleItemViewModel model)
        {
            var current = _serviceFacade.WinServiceGetScheduleItem(model.WinServiceScheduleId);
            current.IsActive = model.IsActive;
            current.EmailAddress = model.EmailAddress;
            current.ScheduledFor = model.ScheduledFor;
            var result = _serviceFacade.UpsertWinServiceSchedule(current);
            return base.JsonGetResult(result);
        }


        [System.Web.Mvc.HttpGet]
        public ActionResult ExecuteWinServiceSchedule()
        {
            var result = _serviceFacade.ExecuteWinServiceSchedule();
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult ClearWinServiceEventLog()
        {
            var result = _serviceFacade.ClearWinServiceEventLog();
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult PriceFreeze()
        {
            var model = _serviceFacade.GetPriceFreezePage();
            return View(model);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult GetPriceFreezeEvent(int eventId)
        {
            var result = _serviceFacade.GetPriceFreezeEvent(eventId);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpPost]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult updatePriceFreezeEvent(PriceFreezeEventViewModel priceFreezeEvent)
        {
            priceFreezeEvent.CreatedBy = User.Identity.Name;
            priceFreezeEvent.CreatedOn = DateTime.Now;
            priceFreezeEvent.DateTo = priceFreezeEvent.DateFrom.AddDays(priceFreezeEvent.Days - 1);
            var result = _serviceFacade.UpsertPriceFreezeEvent(priceFreezeEvent);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult DeletePriceFreezeEvent(int eventId)
        {
            var result = _serviceFacade.DeletePriceFreezeEvent(eventId);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult ExportSettings()
        {
            var settingsXml = _serviceFacade.ExportSettings();
            var filename = "PetrolPricing-SystemSettings.xml";
            var fileBytes = System.Text.Encoding.ASCII.GetBytes(settingsXml);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        [System.Web.Mvc.HttpGet]
        [AuthoriseSystemSettings(Permissions = SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit)]
        public ActionResult ImportSettings()
        {
            // TODO - file upload and error handling
            var settingsXml = "todo";
            _serviceFacade.ImportSettings(settingsXml);
            var model = new object();
            return View(model);
        }
    }
}