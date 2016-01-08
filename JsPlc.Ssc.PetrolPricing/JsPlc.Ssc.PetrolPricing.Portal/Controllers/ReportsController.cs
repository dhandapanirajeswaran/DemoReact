using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Linq;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg = "")
        {
            ViewBag.Message = msg;

            return View();
        }

        [HttpGet]
        public ActionResult CompetitorSites(CompetitorSiteViewModel item)
        {
            if (ModelState.IsValid)
            {
                item.Report = _serviceFacade.GetCompetitorSites(item.SiteId);
            }

            Load(item);
            return View(item);
        }

        [HttpGet]
        public ActionResult PricePoints(PricePointReportContainerViewModel item)
        {
            if (!item.For.HasValue) item.For = DateTime.Now;

            var dieselReport = _serviceFacade.GetPricePoints(item.For.Value, (int)FuelTypeItem.Diesel);
            var unleadedReport = _serviceFacade.GetPricePoints(item.For.Value, (int)FuelTypeItem.Unleaded);
            
            item.PricePointReports.Add(dieselReport);
            item.PricePointReports.Add(unleadedReport);
            
            return View(item);
        }

        [HttpGet]
        public ActionResult NationalAverage()
        {
            return View();
        }

        private void Load(CompetitorSiteViewModel item)
        {
            item.Sites = _serviceFacade.GetSites().OrderBy(x => x.SiteName).ToList();
        }
    }
}