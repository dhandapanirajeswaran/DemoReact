using System.Collections;
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
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;

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

        [System.Web.Mvc.HttpGet]
        public ActionResult PriceMovement(string dateFrom = "", string dateTo = "", int id=0)
        {
            var listOfFuelIds = new []{1, 2, 6};

            var reportContainer = new PriceMovementReportContainerViewModel();

            DateTime fromDate, toDate;
            if (!DateTime.TryParse(dateFrom, out fromDate))
                fromDate = DateTime.Now;
            if (!DateTime.TryParse(dateTo, out toDate))
                toDate = DateTime.Now;

            // swap them around if in wrong order..
            if (fromDate > toDate)
            {
                DateTime tmpDate = fromDate;
                fromDate = toDate;
                toDate = tmpDate;
            }

            var fuelsSelectList = LoadFuels(listOfFuelIds, id);
            ViewData["fuelTypes"] = fuelsSelectList;

            reportContainer.FromDate = fromDate;
            reportContainer.ToDate = toDate;
            reportContainer.FuelTypeId = id;

            if (id != 0 && toDate >= fromDate)
            {
                var selectedItem = fuelsSelectList.FirstOrDefault(x => x.Selected);
                if (selectedItem != null && selectedItem.Value != null && selectedItem.Value != "0")
                {
                    reportContainer.FuelTypeName = selectedItem.Text;
                    reportContainer.PriceMovementReport = _serviceFacade.GetPriceMovement(fromDate, toDate, id);
                }
            }

            var model = reportContainer;
            return View(model);
        }

        /// <summary>
        /// Builds a select list from the Fuels Enum
        /// </summary>
        /// <param name="listOfFuelIds"></param>
        /// <param name="selectedfuelId"></param>
        /// <returns></returns>
        private static SelectList LoadFuels(IEnumerable<int> listOfFuelIds, int selectedfuelId=0)
        {
            var fuellist = new List<SelectItemViewModel>
            {
                new SelectItemViewModel {Id = 0, Name = "Select fuel.."}
            };

            var fuelTypes = from FuelTypeItem s in Enum.GetValues(typeof(FuelTypeItem))
                            select new SelectItemViewModel { Id = (int)s, Name = s.ToString() };

            fuellist.AddRange(fuelTypes.Where(x => listOfFuelIds.Contains(x.Id))); // limit items

            var retval = new SelectList(fuellist, "Id", "Name", selectedfuelId);

            return retval;
        }

        private void Load(CompetitorSiteViewModel item)
        {
            item.Sites = _serviceFacade.GetSites().OrderBy(x => x.SiteName).ToList();
        }
    }
}