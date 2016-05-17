using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web.Http;
using ClosedXML.Excel;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Linq;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
  
	[System.Web.Mvc.Authorize]
	public class PriceReportsController : Controller
	{
		private readonly ServiceFacade _serviceFacade = new ServiceFacade();

		#region Actions
		public ActionResult Index(string msg = "")
		{
			ViewBag.Message = msg;

			return View();
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult CompetitorSites(CompetitorSiteViewModel item)
		{
            try
            {
                if (ModelState.IsValid)
                {
                    item.Report = _serviceFacade.GetCompetitorSites(item.SiteId);
                }

                if (item.Report == null) return View();

                Load(item);

                if (!item.Sites.Any() || item.Sites.First().SiteName == "")
                    return View(item);

                var tempSites = item.Sites;
                item.Sites = new List<Site> { 
                new Site { SiteName = "SAINSBURYS ALL", Id = 0 },
                new Site { SiteName = "SAINSBURYS ALL NORMALISED", Id = -1 }
                 };

                item.Sites.AddRange(tempSites);

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult PricePoints(string For = "")
		{
            try
            {
                DateTime forDate;
                if (!DateTime.TryParse(For, out forDate))
                    forDate = DateTime.Now;

                var item = new PricePointReportContainerViewModel { ForDate = forDate };

                var dieselReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Diesel);
                var unleadedReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Unleaded);

                item.PricePointReports.Add(dieselReport);
                item.PricePointReports.Add(unleadedReport);

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }

		}

		[System.Web.Mvc.HttpGet]
		public ActionResult NationalAverage(string For = "")
		{
            try
            {
                DateTime forDate;
                if (!DateTime.TryParse(For, out forDate))
                    forDate = DateTime.Now;

                var item = new NationalAverageReportContainerViewModel
                {
                    ForDate = forDate,
                    NationalAverageReport = _serviceFacade.GetNationalAverage(forDate)
                };

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult NationalAverage2(string For = "")
		{
            try
            {
                var item = GetNationalAverage2Data(For);

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult CompetitorsPriceRange(string For = "")
		{
            try
            {
                var item = GetNationalAverage2Data(For);

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult CompetitorsPriceRangeByCompany([FromUri]DateTime? DateFor, [FromUri]string SelectedCompanyName, [FromUri]string SelectedBrandName)
		{
            try
            {
                var model = new CompetitorsPriceRangeByCompanyViewModel();

                if (DateFor.HasValue)
                    model.Date = DateFor.Value;

                if (string.IsNullOrWhiteSpace(SelectedCompanyName) == false)
                    model.SelectedCompanyName = SelectedCompanyName;

                if (string.IsNullOrWhiteSpace(SelectedBrandName) == false)
                    model.SelectedBrandName = SelectedBrandName;

                model.FuelTypes = _serviceFacade.GetFuelTypes().Where(ft => model.FuelTypeIds.ToArray().Contains(ft.Id)).ToList();

                var companies = _serviceFacade.GetCompanies();

                if (model.Companies.Any())
                {
                    model.Companies["All"] = companies.Sum(c => c.Value);
                }

                _serviceFacade.GetBrands().ForEach(b => model.Brands.Add(b));

                companies.ForEach(c => model.Companies.Add(c.Key, c.Value));

                var result = _serviceFacade.GetCompetitorsPriceRangeByCompany(model.Date, model.SelectedCompanyName, model.SelectedBrandName);

                if (result != null)
                {
                    model.ReportCompanies = result.ReportCompanies;

                    model.SainsburysPrices = result.SainsburysPrices;
                }

                return View(model);
            }
            catch(Exception ce)
            {
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult PriceMovement([FromUri]DateTime? DateFrom, [FromUri]DateTime? DateTo, [FromUri]int FuelTypeId = 0, [FromUri]string BrandName = "")
		{
            try
            {
                var model = new PriceMovementReportContainerViewModel();

                if (DateFrom.HasValue)
                    model.FromDate = DateFrom.Value;

                if (DateTo.HasValue)
                    model.ToDate = DateTo.Value;

                if (model.FromDate > model.ToDate)
                {
                    ViewBag.ErrorMessage = "Date From must be after or at the date To. Please fix the issue and try again.";
                    model.FromDate = model.ToDate.Value.AddDays(-3);
                }

                if (string.IsNullOrWhiteSpace(BrandName) == false)
                {
                    model.Brand = BrandName;
                }

                if (FuelTypeId > 0)
                {
                    model.FuelTypeId = FuelTypeId;
                }

                var result = LoadPriceMovementReport(model);

                return View(result);
            }
            catch (Exception ce)
            {
                return View();
            }
		}

		/// <summary>
		/// By default on load, it runs for the previous day.. (as today would not make sense) 
		/// Since we wont get compliance for today by default as it is against DailyUpload yet to happen tomorrow.
		/// </summary>
		/// <param name="For"></param>
		/// <returns></returns>
		[System.Web.Mvc.HttpGet]
		public ActionResult Compliance(string For = "")
		{
            try
            {
                DateTime forDate;
                if (!DateTime.TryParse(For, out forDate))
                    forDate = DateTime.Now.AddDays(-1);

                var item = new ComplianceReportContainerViewModel
                {
                    ForDate = forDate,
                    ComplianceReport = _serviceFacade.GetReportCompliance(forDate)
                };

                return View(item);
            }
            catch (Exception ce)
            {
                return View();
            }
		}
		#endregion

		#region Export
		//### #### #### #### ####
		//### EXPORT REPORTS ####
		//### #### #### #### ####

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportPriceMovement([FromUri]DateTime DateFrom, [FromUri]DateTime DateTo, [FromUri]int FuelTypeId = 0, [FromUri]string BrandName = "")
		{
			var model = new PriceMovementReportContainerViewModel
			{
				FromDate = DateFrom,
				ToDate = DateTo,
				FuelTypeId = FuelTypeId,
				Brand = BrandName
			};

			var reportContainer = LoadPriceMovementReport(model);

			var dt = reportContainer.ToPriceMovementReportDataTable(model.Brand + " PriceMovementReport"); // default tableName = PriceMovementReport (also becomes sheet name in Xlsx)

			string filenameSuffix = String.Format("[{0}] [{1} to {2}]",
				reportContainer.FuelTypeName,
					reportContainer.FromDate.Value.ToString("dd-MMM-yyyy"),
					reportContainer.ToDate.Value.ToString("dd-MMM-yyyy"));


			return ExcelDocumentStream(new List<DataTable> { dt }, model.Brand + " PriceMovementReport", filenameSuffix);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportNationalAverage(string For = "")
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
				NationalAverageReport = _serviceFacade.GetNationalAverage(forDate)
			};

			var dt = reportContainer.ToNationalAverageReportDataTable(); // default tableName = PriceMovementReport (also becomes sheet name in Xlsx)

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

			return ExcelDocumentStream(new List<DataTable> { dt }, "NationalAverageReport", filenameSuffix);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportNationalAverage2(string For = "")
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
				NationalAverageReport = _serviceFacade.GetNationalAverage2(forDate)
			};

			var dt = reportContainer.ToNationalAverageReport2DataTable();

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

			return ExcelDocumentStream(new List<DataTable> { dt }, "NationalAverageReport2", filenameSuffix);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportCompetitorsPriceRange(string For = "")
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
				NationalAverageReport = _serviceFacade.GetNationalAverage2(forDate)
			};

			var dtByBrand = reportContainer.ToCompetitorsPriceRangeByBrandDataTable(); // default tableName = PriceMovementReport (also becomes sheet name in Xlsx)

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

			return ExcelDocumentStream(new List<DataTable> { dtByBrand }, "CompetitorsPriceRange", filenameSuffix);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportCompetitorsPriceRangeByCompany([FromUri]DateTime DateFor, [FromUri]string SelectedCompanyName, [FromUri]string SelectedBrandName)
		{
			var report = _serviceFacade.GetCompetitorsPriceRangeByCompany(DateFor, SelectedCompanyName, SelectedBrandName);

			report.FuelTypes = _serviceFacade.GetFuelTypes().Where(ft => report.FuelTypeIds.ToArray().Contains(ft.Id)).ToList();

			var dt = report.ToCompetitorsPriceRangeByCompanyDataTable();

			string filenameSuffix = String.Format("{1}-{2} [{0}]", DateFor.ToString("dd-MMM-yyyy"), SelectedCompanyName, SelectedBrandName);

			return ExcelDocumentStream(new List<DataTable> { dt }, "CompetitorsPriceRangeByCompany", filenameSuffix);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportPricePoints(string For = "")
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new PricePointReportContainerViewModel { ForDate = forDate };

			var dieselReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Diesel);
			var unleadedReport = _serviceFacade.GetPricePoints(forDate, (int)FuelTypeItem.Unleaded);

			reportContainer.PricePointReports.Add(dieselReport);
			reportContainer.PricePointReports.Add(unleadedReport);

			var tables = reportContainer.ToPricePointsReportDataTable(); // Each tableName = As per fuelname in each PricePointReports

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

			return ExcelDocumentStream(tables, "PricePointsReport", filenameSuffix);
		}
		#endregion

		#region Private
		private PriceMovementReportContainerViewModel LoadPriceMovementReport(PriceMovementReportContainerViewModel model)
		{
			model.FuelTypes = LoadFuels(new[] { 1, 2, 6 });
			model.Brands = _serviceFacade.GetBrands().ToList();

			if (model.FuelTypeId > 0 && model.ToDate >= model.FromDate)
			{
				var selectedItem = model.FuelTypes[model.FuelTypeId];
				if (model.FuelTypes.ContainsKey(model.FuelTypeId))
				{
					model.FuelTypeName = model.FuelTypes[model.FuelTypeId];
					model.PriceMovementReport = _serviceFacade.GetPriceMovement(model.Brand, model.FromDate.Value, model.ToDate.Value, model.FuelTypeId);
				}
			}

			return model;
		}

		/// <summary>
		/// Builds a select list from the Fuels Enum
		/// </summary>
		/// <param name="listOfFuelIds"></param>
		/// <param name="selectedfuelId"></param>
		/// <returns></returns>
		private static Dictionary<int, string> LoadFuels(IEnumerable<int> listOfFuelIds)
		{
			var result = new Dictionary<int, string>();
			
			var fuelTypes = from FuelTypeItem s in Enum.GetValues(typeof(FuelTypeItem))
							select new SelectItemViewModel { Id = (int)s, Name = s.ToString().Replace('_',' ') };

			fuelTypes.Where(x => listOfFuelIds.Contains(x.Id)).ForEach(x => result.Add(x.Id, x.Name));

			return result;
		}

		private void Load(CompetitorSiteViewModel item)
		{
			item.Sites = _serviceFacade.GetSites().OrderBy(x => x.SiteName).ToList();
		}

		private NationalAverageReportContainerViewModel GetNationalAverage2Data(string For)
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var item = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
				NationalAverageReport = _serviceFacade.GetNationalAverage2(forDate)
			};
			return item;
		}

		private ActionResult ExcelDocumentStream(List<DataTable> tables, string fileName, string fileNameSuffix)
		{
			//if (!tables.Any())
			//{
			//	return new ContentResult { Content = "No data to download..", ContentType = "text/plain" };
			//}

			//if (tables[0].Rows.Count <= 1) // Model != null && Model.NationalAverageReport != null && Model.NationalAverageReport.Fuels.Any()
			//{
			//	return new ContentResult { Content = "No data to download..", ContentType = "text/plain" };
			//}

			using (var wb = new XLWorkbook())
			{
				foreach (var dt in tables)
				{
					wb.Worksheets.Add(dt);
				}
				wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				wb.Style.Font.Bold = true;

				Response.Clear();
				Response.Buffer = true;
				Response.Charset = "";
				Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				string downloadHeader = String.Format("attachment;filename= {1}{0}.xlsx",
					fileNameSuffix,
					fileName);
				Response.AddHeader("content-disposition", downloadHeader);

				using (var myMemoryStream = new MemoryStream())
				{
					wb.SaveAs(myMemoryStream);
					myMemoryStream.WriteTo(Response.OutputStream);
					Response.Flush();
					Response.End();
					return new EmptyResult();
				}
			}
		}
		#endregion
	}
}