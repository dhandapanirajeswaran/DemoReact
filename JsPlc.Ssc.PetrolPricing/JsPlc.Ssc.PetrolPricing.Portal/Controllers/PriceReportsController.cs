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
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
  
	[System.Web.Mvc.Authorize]
	public class PriceReportsController : BaseController
	{
		private readonly ServiceFacade _serviceFacade ;

	    private readonly ILogger _logger;

	    public PriceReportsController()
	    {
	        _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
	    }
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
            catch (Exception ex)
            {
                _logger.Error(ex);
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
            catch (Exception ex)
            {
                _logger.Error(ex);
                return View();
            }

		}

		[System.Web.Mvc.HttpGet]
		public ActionResult NationalAverage(string For = "")
		{
            try
            {
                var item = GetNationalAverageData(For);

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
                _logger.Error(ce);
                return View();
            }
		}
            

		[System.Web.Mvc.HttpGet]
		public ActionResult CompetitorsPriceRange(string For = "")
		{
            try
            {
                var item = CompetitorsPriceRangeData(For);

                return View(item);
            }
            catch (Exception ce)
            {
                _logger.Error(ce);
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
                _logger.Error(ce);
                return View();
            }
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult PriceMovement([FromUri]DateTime? DateFrom, [FromUri]DateTime? DateTo, [FromUri]int FuelTypeId = 0, [FromUri]string BrandName = "", [FromUri]string SiteName = "")
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
                    model.FromDate = model.ToDate.Value.AddDays(-30);
                }

                if (string.IsNullOrWhiteSpace(BrandName) == false)
                {
                    model.Brand = BrandName;
                }

                if (string.IsNullOrWhiteSpace(SiteName) == false)
                {
                    model.SiteName = SiteName;
                }

                if (FuelTypeId > 0)
                {
                    model.FuelTypeId = FuelTypeId;
                }

                var result = LoadPriceMovementReport(model);
                result.ReportWidth =
                    result.PriceMovementReport.ReportRows.First().DataItems.Select(x => x.PriceDate).Count()*150 + 250;

                return View(result);
            }
            catch (Exception ce)
            {
                _logger.Error(ce);
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
                _logger.Error(ce);
                return View();
            }
		}
		#endregion

		#region Export
		//### #### #### #### ####
		//### EXPORT REPORTS ####
		//### #### #### #### ####

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportPriceMovement([FromUri]string downloadId, [FromUri]DateTime DateFrom, [FromUri]DateTime DateTo, [FromUri]int FuelTypeId = 0, [FromUri]string BrandName = "")
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


            return ExcelDocumentStream(new List<DataTable> { dt }, model.Brand + " PriceMovementReport", filenameSuffix, downloadId);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportNationalAverage(string downloadId, string For = "")
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

            return ExcelDocumentStream(new List<DataTable> { dt }, "NationalAverageReport", filenameSuffix, downloadId);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportNationalAverage2(string downloadId, string For = "",bool  viewAllCompitetors=false)
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
                NationalAverageReport = _serviceFacade.GetNationalAverage2(forDate, viewAllCompitetors)
			};

			var dt = reportContainer.ToNationalAverageReport2DataTable();

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

            return ExcelDocumentStream(new List<DataTable> { dt }, "NationalAverageReport2", filenameSuffix, downloadId);
		}

        [System.Web.Mvc.HttpGet]
        public ActionResult ExportCompetitorSites(string downloadId, int siteId)
		{
            var reportContainer = _serviceFacade.GetCompetitorSites(siteId);

            DateTime forDate = DateTime.Now;
          
            var dt = reportContainer.ToCompetitorSitesDataTable();

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

            return ExcelDocumentStream(new List<DataTable> { dt }, "CompetitorSites", filenameSuffix, downloadId);
		}
       

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportCompetitorsPriceRange(string downloadId, string For = "")
		{
			DateTime forDate;
			if (!DateTime.TryParse(For, out forDate))
				forDate = DateTime.Now;

			var reportContainer = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
                NationalAverageReport = _serviceFacade.CompetitorsPriceRangeData(forDate)
			};

			var dtByBrand = reportContainer.ToCompetitorsPriceRangeByBrandDataTable(); // default tableName = PriceMovementReport (also becomes sheet name in Xlsx)

			string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));

            return ExcelDocumentStream(new List<DataTable> { dtByBrand }, "CompetitorsPriceRange", filenameSuffix, downloadId);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportCompetitorsPriceRangeByCompany([FromUri]string downloadId, [FromUri]DateTime DateFor, [FromUri]string SelectedCompanyName, [FromUri]string SelectedBrandName)
		{
			var report = _serviceFacade.GetCompetitorsPriceRangeByCompany(DateFor, SelectedCompanyName, SelectedBrandName);

			report.FuelTypes = _serviceFacade.GetFuelTypes().Where(ft => report.FuelTypeIds.ToArray().Contains(ft.Id)).ToList();

			var dt = report.ToCompetitorsPriceRangeByCompanyDataTable();

			string filenameSuffix = String.Format("{1}-{2} [{0}]", DateFor.ToString("dd-MMM-yyyy"), SelectedCompanyName, SelectedBrandName);

            return ExcelDocumentStream(new List<DataTable> { dt }, "CompetitorsPriceRangeByCompany", filenameSuffix, downloadId);
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult ExportPricePoints(string downloadId, string For = "")
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

            return ExcelDocumentStream(tables, "PricePointsReport", filenameSuffix, downloadId);
		}
		#endregion

		#region Private
		private PriceMovementReportContainerViewModel LoadPriceMovementReport(PriceMovementReportContainerViewModel model)
		{
			model.FuelTypes = LoadFuels(new[] { 1, 2, 6 });
			model.Brands = _serviceFacade.GetBrands().ToList();
            model.Brands.Insert(0,"All");

			if (model.FuelTypeId > 0 && model.ToDate >= model.FromDate)
			{
				var selectedItem = model.FuelTypes[model.FuelTypeId];
				if (model.FuelTypes.ContainsKey(model.FuelTypeId))
				{
					model.FuelTypeName = model.FuelTypes[model.FuelTypeId];
                    model.PriceMovementReport = _serviceFacade.GetPriceMovement(model.Brand, model.FromDate.Value, model.ToDate.Value, model.FuelTypeId, model.SiteName);
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

        private NationalAverageReportContainerViewModel CompetitorsPriceRangeData(string For)
        {
            DateTime forDate;
            if (!DateTime.TryParse(For, out forDate))
                forDate = DateTime.Now;

            var item = new NationalAverageReportContainerViewModel
            {
                ForDate = forDate,
                NationalAverageReport = _serviceFacade.CompetitorsPriceRangeData(forDate)
            };
            return item;
        }

		private NationalAverageReportContainerViewModel GetNationalAverage2Data(string For,bool bViewAllCompetitors=false)
		{
            DateTime forDate = DateTime.Now;
            if (For != "")
            {
                if (!DateTime.TryParse(For, out forDate))
                {
                    string[] tokenize = For.Split('/');
                    forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
                }
            }

			var item = new NationalAverageReportContainerViewModel
			{
				ForDate = forDate,
                NationalAverageReport = _serviceFacade.GetNationalAverage2(forDate, bViewAllCompetitors)
			};
			return item;
		}

        private NationalAverageReportContainerViewModel GetNationalAverageData(string For)
        {
            DateTime forDate=DateTime.Now;
            if (For != "")
            {
                if (!DateTime.TryParse(For, out forDate) && For != "")
                {
                    string[] tokenize = For.Split('/');
                    forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
                }
            }
            var item = new NationalAverageReportContainerViewModel
            {
                ForDate = forDate,
                NationalAverageReport = _serviceFacade.GetNationalAverage(forDate)
            };
            return item;
        }

        private ActionResult ExcelDocumentStream(List<DataTable> tables, string fileName, string fileNameSuffix, string downloadId)
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
                    var ws = wb.Worksheets.Add(dt);
                    if (fileName == "NationalAverageReport2" || fileName == "CompetitorSites" || fileName == "NationalAverageReport" || fileName == "CompetitorsPriceRange" || fileName.Contains("PriceMovementReport"))
                    {
                        ws.Rows().AdjustToContents();
                        ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    }
                    if (fileName.Contains("PriceMovementReport"))
                    {
                        var rangeAddress = ws.Tables.FirstOrDefault().RangeAddress;
                        var cellrange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.LastAddress.ColumnLetter, rangeAddress.FirstAddress.ColumnNumber);

                        var sitesCellRange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.FirstAddress.ColumnLetter, ws.Rows().Count());
                        var Others_CellRange = string.Format("B2:{0}{1}", rangeAddress.LastAddress.ColumnLetter, ws.Rows().Count());

                        ws.Range(cellrange).Style.NumberFormat.SetFormat("@");
                        ws.Range(sitesCellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(Others_CellRange).Style.NumberFormat.SetFormat("0.00");
                    }
                    if (fileName == "CompetitorsPriceRange")
                    {
                        var rangeAddress = ws.Tables.FirstOrDefault().RangeAddress;
                        var cellrange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.LastAddress.ColumnLetter, rangeAddress.FirstAddress.ColumnNumber);

                        var brandCellRange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.FirstAddress.ColumnLetter, ws.Rows().Count());
                        var dieselPriceRange_CellRange = string.Format("F2:F{0}", ws.Rows().Count());
                        var unleadedPriceRange_CellRange = string.Format("G2:G{0}", ws.Rows().Count());
                        var dieselAvgRetails_CellRange = string.Format("B2:B{0}", ws.Rows().Count());
                        var unleadedAvgRetails_CellRange = string.Format("C2:C{0}", ws.Rows().Count());
                        var dieselDiffRetails_CellRange = string.Format("D2:D{0}", ws.Rows().Count());
                        var unleadedDiffRetails_CellRange = string.Format("E2:E{0}", ws.Rows().Count());

                        ws.Range(cellrange).Style.NumberFormat.SetFormat("@");
                        ws.Range(brandCellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(dieselPriceRange_CellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(unleadedPriceRange_CellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(dieselAvgRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(unleadedAvgRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(dieselDiffRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(unleadedDiffRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                    }
                    if (fileName == "CompetitorsPriceRangeByCompany")
                    {
                        var rangeAddress = ws.Tables.FirstOrDefault().RangeAddress;
                        var cellrange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.LastAddress.ColumnLetter, rangeAddress.FirstAddress.ColumnNumber);

                        var companyCellRange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.FirstAddress.ColumnLetter, ws.Rows().Count());
                        var brandCellRange = string.Format("B2:B{0}", ws.Rows().Count());
                        var dieselAvgRetail_CellRange = string.Format("C2:C{0}", ws.Rows().Count());
                        var unleadedAvgRetail_CellRange = string.Format("D2:D{0}", ws.Rows().Count());
                        var unleadedDiffRetails_CellRange = string.Format("E2:E{0}", ws.Rows().Count());
                        var dieselDiffRetails_CellRange = string.Format("F2:F{0}", ws.Rows().Count());
                        var unleadedPriceRange_CellRange = string.Format("G2:G{0}", ws.Rows().Count());
                        var dieselPriceRange_CellRange = string.Format("H2:H{0}", ws.Rows().Count());

                        ws.Range(cellrange).Style.NumberFormat.SetFormat("@");
                        ws.Range(brandCellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(companyCellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(unleadedPriceRange_CellRange).Style.NumberFormat.SetFormat("@");
                        ws.Range(dieselPriceRange_CellRange).Style.NumberFormat.SetFormat("@");

                        ws.Range(dieselAvgRetail_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(unleadedAvgRetail_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(unleadedDiffRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                        ws.Range(dieselDiffRetails_CellRange).Style.NumberFormat.SetFormat("0.00");
                    }
                    if (fileName == "PricePointsReport")
                    {
                        var rangeAddress = ws.Tables.FirstOrDefault().RangeAddress;
                        var cellrange = string.Format("{0}:{1}{2}", rangeAddress.FirstAddress, rangeAddress.LastAddress.ColumnLetter, rangeAddress.FirstAddress.ColumnNumber);


                        ws.Range(cellrange).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Range(cellrange).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Range(cellrange).Style.Alignment.TextRotation = 90;
                        ws.Range(string.Format("{0}", rangeAddress.FirstAddress)).Style.Alignment.TextRotation = 45;
                    }
                    if (fileName == "NationalAverageReport")
                    {
                        int nCount = ws.Rows().Count();
                        for (int i = 2; i <= nCount; i++)
                        {
                            var Cell_DieselVariance = "F" + i.ToString();
                            var Cell_UnleadedVariance = "G" + i.ToString();
                            var Cell_AveragelVariance = "H" + i.ToString();
                            object value_DieselVariance = ws.Cell(Cell_DieselVariance).Value;
                            object value_UnleadedVariance = ws.Cell(Cell_UnleadedVariance).Value;
                            object value_AveragelVariance = ws.Cell(Cell_AveragelVariance).Value;
                            if (value_DieselVariance.ToString() != "")
                            {
                                if (Convert.ToDouble(value_DieselVariance) > 0.0)
                                {
                                    ws.Cell(Cell_DieselVariance).Style.Font.FontColor = XLColor.Green;
                                }
                                else if (Convert.ToDouble(value_DieselVariance) < 0.0)
                                {
                                    ws.Cell(Cell_DieselVariance).Style.Font.FontColor = XLColor.Red;
                                }
                            }

                            if (value_UnleadedVariance.ToString() != "")
                            {
                                if (Convert.ToDouble(value_UnleadedVariance) > 0.0)
                                {
                                    ws.Cell(Cell_UnleadedVariance).Style.Font.FontColor = XLColor.Green;
                                }
                                else if (Convert.ToDouble(value_UnleadedVariance) < 0.0)
                                {
                                    ws.Cell(Cell_UnleadedVariance).Style.Font.FontColor = XLColor.Red;
                                }
                            }

                            if (value_AveragelVariance.ToString() != "")
                            {
                                if (Convert.ToDouble(value_AveragelVariance) > 0.0)
                                {
                                    ws.Cell(Cell_AveragelVariance).Style.Font.FontColor = XLColor.Green;
                                }
                                else if (Convert.ToDouble(value_AveragelVariance) < 0.0)
                                {
                                    ws.Cell(Cell_AveragelVariance).Style.Font.FontColor = XLColor.Red;
                                }
                            }
                        }

                    }

                    if (fileName == "NationalAverageReport2")
                    {
                        int nColCount = ws.Columns().Count();
                        char cellAlphabet = 'E';
                        for (int i = 4; i <= nColCount; i++)
                        {
                            string cell = string.Format("{0}4", cellAlphabet);
                            object cell_value = ws.Cell(cell).Value;
                            if (cell_value.ToString() != "")
                            {
                                if (Convert.ToDouble(cell_value) > 0.0)
                                {
                                    ws.Cell(cell).Style.Font.FontColor = XLColor.Green;
                                }
                                else if (Convert.ToDouble(cell_value) < 0.0)
                                {
                                    ws.Cell(cell).Style.Font.FontColor = XLColor.Red;
                                }
                            }
                            cellAlphabet++;

                        }

                    }
                }
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;

                var excelFilename = String.Format("{1}{0}.xlsx", fileNameSuffix, fileName);

                return base.SendExcelFile(excelFilename, wb, downloadId);
            }
        }
		#endregion
	}
}