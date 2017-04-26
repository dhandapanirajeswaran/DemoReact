using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Layout;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class LayoutController : BaseController
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public LayoutController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        public PartialViewResult TopNavigation()
        {
            var model = new TopNavigationViewModel()
            {
                UserAccess = base.GetUserAccessModel()
            };
            return PartialView("~/Views/Shared/Layout/_TopNavigation.cshtml", model);
        }

        public PartialViewResult DataSanityCheckSummary()
        {
            var model = _serviceFacade.GetDataSanityCheckSummary();
            return PartialView("~/Views/Shared/Layout/_DataSanityCheckSummary.cshtml", model);
        }
    }
}