using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
//using JsPlc.Ssc.PetrolPricing.Business;
using System.Web.Routing;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
//using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using Microsoft.Ajax.Utilities;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class SitesController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;

            var model = _serviceFacade.GetSites();
            return View(model);
        }
        public ActionResult Prices(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;

            // var model = _serviceFacade.GetSites();
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Site site)
        {
            site.IsSainsburysSite = true; 
            var createdSite = _serviceFacade.NewSite(site);
            return RedirectToAction("Index", new {msg = "Site: " + createdSite.SiteName + " created successfully"});
        }
    }
}