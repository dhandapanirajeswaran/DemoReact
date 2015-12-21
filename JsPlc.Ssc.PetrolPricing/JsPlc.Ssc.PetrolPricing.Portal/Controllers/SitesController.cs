﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [System.Web.Mvc.Authorize]
    public class SitesController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        // AJAX Methods

        // Coded Only - TODO wire up the postback to backend
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> PutPriceOverride([FromBody] List<SitePriceViewModel> sitePricesView)
        {
            if (ModelState.IsValid)
            {
                var response = await _serviceFacade.UpdateSitePricesAsync(sitePricesView);
                return !response.Any() ? new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(null, null, "ApiFail") : 
                    new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(response, null, "ApiSuccess");
            }
            var errArray = this.GetUiErrorList();
            var badRequestResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };
            // key and string of arrays
            return badRequestResponse.ToJsonResult(null, errArray, "UIValidationErrors");
        }

        [ScriptMethod(UseHttpGet = true)]
        public JsonResult GetSitesWithPricesJson(string date=null, int siteId=0, int pageNo=1, 
                int pageSize=Constants.PricePageSize, int getCompetitor=0)
        {
            DateTime forDate;
            if (!DateTime.TryParse(date, out forDate)) forDate = DateTime.Now;
            // POST scenarios use : JsonConvert.SerializeObject(siteView);
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = (getCompetitor != 1)
                ? _serviceFacade.GetSitePrices(forDate, siteId, pageNo, pageSize)
                : _serviceFacade.GetCompetitorsWithPrices(forDate, siteId, pageNo, pageSize); // for getting comps by ajax, not used yet
            //sitesViewModelsWithPrices = null; // Force error

            if (getCompetitor == 1)
                sitesViewModelsWithPrices = sitesViewModelsWithPrices.OrderBy(x => x.DriveTime).ToList();
            // eager load comps
            //if (sitesViewModelsWithPrices != null)
            //{
            //    sitesViewModelsWithPrices.ForEach(model =>
            //    {
            //        var competitors = _serviceFacade.GetCompetitorsWithPrices(forDate, model.SiteId, pageNo, pageSize);
            //        model.hasCompetitors = false;
            //        model.competitors = new List<SitePriceViewModel>();
            //        var compList = competitors as List<SitePriceViewModel> ?? competitors.ToList();

            //        if (!compList.Any()) return;
            //        model.hasCompetitors = true;
            //        compList = compList.OrderBy(x => x.DriveTime).ToList();
            //        model.competitors = compList;
            //    });
            //}

            var jsonData = sitesViewModelsWithPrices != null ? (object)sitesViewModelsWithPrices : "Error";
            // NOTE: The prices are still in 4 digit format (do price/10 for display)
            // -- Uses SitePrice table, shows no prices until populated (using CalcPrice calls)

            var jsonResult = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = jsonData
            };
            return jsonResult;
        }

        public ActionResult Index(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;

            var model = _serviceFacade.GetSites().Where(x => x.IsSainsburysSite);
            return View(model);
        }

        public ActionResult Create()
        {
            return View(new Site());
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(Site site)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please check for validation errors under each field.";
                return View(site);
            }
            site.IsSainsburysSite = true; 
            var nonBlankVals = new List<SiteEmail>();
            site.Emails.ForEach(x =>
            {
                if (!x.EmailAddress.IsNullOrWhiteSpace()) nonBlankVals.Add(x);
            });
            site.Emails = nonBlankVals;

            var createdSite = _serviceFacade.NewSite(site);
            if (createdSite != null) return RedirectToAction("Index", new { msg = "Site: " + createdSite.SiteName + " created successfully" });

            ViewBag.ErrorMessage = "Unable to create site. Check if this CatNo or SiteName already exists.";
            return View(site);
        }

        public ActionResult Details(int id)
        {
            var model = _serviceFacade.GetSite(id);
            return View(model);
        }
        /// <summary>
        /// Works on List of SitePriceViewModel to build core page
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ActionResult Prices(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;
            //var sitesViewModelsWithPrices = _serviceFacade.GetSitePrices();
            // return empty list but never null

            return View("Prices"); // Razor based view
        }


        [System.Web.Mvc.HttpPost]
        public ActionResult Prices()
        {
            //_serviceFacade.EmailUpdatedPricesToSite();

            //var sitesViewModelsWithPrices = _serviceFacade.GetSitePrices();
            // return empty list but never null

            //return View(sitesViewModelsWithPrices);
            return View("Prices");
        }

        public ActionResult Edit(int id)
        {
            var model = _serviceFacade.GetSite(id);
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(Site site)
        {
            // TODO Somehow email edits fail on VM, MediaType Formatter error when text/html returned from api.
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please check for validation errors under each field.";
                return View(site);
            }
            site.IsSainsburysSite = true;
            var nonBlankVals = new List<SiteEmail>();
            site.Emails.ForEach(x =>
            {
                if (!x.EmailAddress.IsNullOrWhiteSpace())
                {
                    x.SiteId = site.Id;
                    nonBlankVals.Add(x);
                }
            });
            site.Emails = nonBlankVals;

            var editSite = _serviceFacade.EditSite(site);

            if (editSite != null) return RedirectToAction("Index", new { msg = "Site: " + editSite.SiteName + " updated successfully" });

            ViewBag.ErrorMessage = "Unable to create site.";
            return View();
        }

    }
}