using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Linq;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ReportsController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg = "")
        {
            ViewBag.Message = msg;

            return View();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult CompetitorSites(CompetitorSiteViewModel item)
        {
            if (ModelState.IsValid)
            {
                item.Report = _serviceFacade.GetCompetitorSites(item.SiteId);
            }
            Load(item);
            if (!item.Sites.Any() || item.Sites.First().SiteName == "") return View(item);

            var tempSites = item.Sites;
            item.Sites = new List<Site> { new Site { SiteName = "Please select..." } };
            item.Sites.AddRange(tempSites);
            
            return View(item);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult PricePoints(string For="")
        {
            var item = new PricePointReportContainerViewModel{For = For};

            DateTime forDate;
            if (!DateTime.TryParse(item.For, out forDate))
                forDate = DateTime.Now;

            item.ForDate = forDate;

            var dieselReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Diesel);
            var unleadedReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Unleaded);

            item.PricePointReports.Add(dieselReport);
            item.PricePointReports.Add(unleadedReport);

            return View(item);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult NationalAverage(string For="")
        {
            var item = new NationalAverageReportContainerViewModel {For = For};
            DateTime forDate;
            if (!DateTime.TryParse(item.For, out forDate))
                forDate = DateTime.Now;

            item.ForDate = forDate;
            item.NationalAverageReport = _serviceFacade.GetNationalAverage(forDate);
            return View(item);
        }

        private void Load(CompetitorSiteViewModel item)
        {
            item.Sites = _serviceFacade.GetSites().OrderBy(x => x.SiteName).ToList();
        }
    }
}