﻿using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public DiagnosticsController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }


        // GET: Diagnostics
        public ActionResult Index()
        {
            var daysAgo = 7;
            var model = _serviceFacade.GetDiagnostics(daysAgo);

            return View(model);
        }
    }
}