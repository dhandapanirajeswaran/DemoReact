﻿using JsPlc.Ssc.PetrolPricing.Core;
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
  
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public HomeController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        public ActionResult Index()
        {
            var model = _serviceFacade.GetRecentFileUploadSummary();
            model.UserAccess = base.GetUserAccessModel();
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            var model = _serviceFacade.GetContactDetails();
            return View(model);
        }

        public ActionResult AccessDenied()
        {
            var model = new AccessDeniedViewModel()
            {
                Message = "You do not have permission to access this section."
            };
            return View("~/Views/Shared/AccessDenied.cshtml", model);
        }

        public ActionResult PleaseSignIn()
        {
            return View("~/Views/Shared/PleaseSignIn.cshtml");
        }

        public ActionResult AccountInactive()
        {
            return View("~/Views/Shared/AccountInactive.cshtml");
        }
    }
}