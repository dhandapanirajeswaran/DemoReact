﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Portal.Helper;
using System.Data;
using ClosedXML.Excel;


namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
   
    [System.Web.Mvc.Authorize]
    public class SitesController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();


        // AJAX Methods

        // Coded Only - wired up the postback to backend
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> SavePriceOverrides([FromBody] OverridePricePostViewModel[] postbackKey1 = null)
        {
            try
            {
                if (postbackKey1 != null)
                {
                    List<OverridePricePostViewModel> siteOverridePriceList = postbackKey1.ToList();
                    //postbackKey1[0].OverridePrice = "abc"; // force error
                    if (ModelState.IsValid)
                    {
                        //var siteOverridePriceList = siteOverridePrices;
                        var response = await _serviceFacade.SaveOverridePricesAsync(siteOverridePriceList);

                        return (response == null || !response.Any())
                            ? new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(postbackKey1, null, "ApiFail", "Invalid postback data")
                            : new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(response, null, "ApiSuccess");
                    }
                    var errArray = this.GetUiErrorList();
                    var badRequestResponse = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    };
                    // key and string of arrays
                    return badRequestResponse.ToJsonResult(postbackKey1, errArray, "UIValidationErrors");
                }
                return new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(null, null, "ApiSuccess");
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(postbackKey1, null, "ApiFail",
                    ex.Message);
            }
        }

        /// <summary>
        /// Send email to site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> SendEmailToSite(int siteId = 0)
        {
            List<EmailSendLog> sendLog = null;
            try
            {
                // Email all sites
                var response = await _serviceFacade.EmailUpdatedPricesSites(siteId);
                sendLog = response;
                var sendSummaryString = sendLog.ToSendSummary();
                return (response == null || !response.Any())
                    ? new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(response, null, "ApiFail",
                        "Error: unable to send emails, please check each status for per site - Errors or warnings near envelope icon..\n")
                    : new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(sendLog, null, "ApiSuccess", "", sendSummaryString);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    .ToJsonResult(sendLog, null, "ApiFail", ex.Message);
            }
        }

        [ScriptMethod(UseHttpGet = true)]
        public JsonResult GetSitesWithPricesJson(string date = null, string storeName = "",
            int catNo = 0, int storeNo = 0,
            string storeTown = "", int siteId = 0, int pageNo = 1,
                int pageSize = Constants.PricePageSize, int getCompetitor = 0)
        {
            DateTime forDate;
            if (!DateTime.TryParse(date, out forDate))
            {
                forDate = DateTime.Now;
            }
           
            // POST scenarios use : JsonConvert.SerializeObject(siteView);
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = (getCompetitor != 1)
                ? _serviceFacade.GetSitePrices(forDate, storeName, catNo, storeNo, storeTown, siteId, pageNo, pageSize)
                : _serviceFacade.GetCompetitorsWithPrices(forDate, siteId, pageNo, pageSize); // for getting comps by ajax

            if (getCompetitor == 1)
            {
                sitesViewModelsWithPrices = sitesViewModelsWithPrices.OrderBy(x => x.DriveTime).ToList();
                //sitesViewModelsWithPrices = null; // Force error, should show no competitors
            }

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

        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> GetEmailSendLog(string date = null, int siteId = 0)
        {
            DateTime forDate;
            if (!DateTime.TryParse(date, out forDate)) forDate = DateTime.Now;

            List<EmailSendLog> sendLog = null;
            try
            {
                // Email all sites
                var response = await _serviceFacade.GetEmailSendLog(siteId, forDate);
                sendLog = response;
                return (response == null)
                    ? new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(sendLog, null, "ApiFail",
                        "Invalid data")
                    : new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(sendLog, null, "ApiSuccess");
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    .ToJsonResult(sendLog, null, "ApiFail", ex.Message);
            }
        }

        public ActionResult Index(string msg = "", string storeName = "", string storeTown = "", int catNo = 0, int storeNo = 0)
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;           
            try
            {
                var model = _serviceFacade.GetSites().Where(x => x.IsSainsburysSite);
                // Filtering based on search value

                if (string.IsNullOrWhiteSpace(storeName) == false)
                    model = model.Where(x => string.IsNullOrWhiteSpace(x.SiteName) == false && x.SiteName.ToUpper().Contains(storeName.ToUpper()));

                if (string.IsNullOrWhiteSpace(storeTown) == false)
                    model = model.Where(x => string.IsNullOrWhiteSpace(x.Town) == false && x.Town.ToUpper().Contains(storeTown.ToUpper()));

                if (catNo > 0)
                    model = model.Where(x => x.CatNo == catNo);

                if (storeNo > 0)
                    model = model.Where(x => x.StoreNo == storeNo);


                return View(model.ToList());
            }
            catch(Exception ce)
            {
                return View();
            }
        }



        public ActionResult Create()
        {
            return View(new SiteViewModel());
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(SiteViewModel site)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please check for validation errors under each field.";
                return View(site);
            }
            site.IsSainsburysSite = true;
            var nonBlankVals = new List<SiteEmailViewModel>();
            ListExtensions.ForEach(site.Emails, x =>
            {
                if (!x.EmailAddress.IsNullOrWhiteSpace()) nonBlankVals.Add(x);
            });
            site.Emails = nonBlankVals;

            var createdSite = _serviceFacade.NewSite(site);
            if (createdSite.ViewModel != null) 
                return RedirectToAction("Index", new { msg = "Site: " + createdSite.ViewModel.SiteName + " created successfully" });

			ViewBag.ErrorMessage = createdSite.ErrorMessage;
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
        public ActionResult Prices(int x = 0, string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;
            //var sitesViewModelsWithPrices = _serviceFacade.GetSitePrices();
            // return empty list but never null

             SiteViewModel model = new SiteViewModel();

            // model.ExcludeBrands = _serviceFacade.GetBrands().ToList();
             model.AllBrands = _serviceFacade.GetBrands().ToList();
             var excludebrands = _serviceFacade.GetExcludeBrands();
             model.ExcludeBrands = excludebrands != null ? excludebrands.ToList() : null;
             model.ExcludeBrandsOrg = model.ExcludeBrands;

           
            return View("Prices",model); // Razor based view
        }


        [System.Web.Mvc.HttpPost]
        public ActionResult PostPrices([FromBody] string postBackData = "")
        {
            List<SitePriceViewModel> siteData = JsonConvert.DeserializeObject(postBackData) as List<SitePriceViewModel>;
            if (siteData != null)
            {
                Debug.Write("FormData:" + Request.Form);
            }
            //_serviceFacade.EmailUpdatedPricesToSite();

            //var sitesViewModelsWithPrices = _serviceFacade.GetSitePrices();
            // return empty list but never null

            //return View(sitesViewModelsWithPrices);
            return View("Prices");
        }

        public ActionResult Edit(int id)
        {
            var model = _serviceFacade.GetSite(id);

            var sortedCompetitors = model.Competitors.Where(c => c.IsSainsburysSite == false).OrderBy(c => c.SiteName).ToList();

            /*sortedCompetitors.Insert(0, new SiteViewModel
            {
                SiteName = "Not specified"
            });*/

            model.ExcludeCompetitors = model.ExcludeCompetitors.Distinct().ToList();
            model.AllBrands = _serviceFacade.GetBrands().ToList();
            var excludebrands=_serviceFacade.GetExcludeBrands();
            model.ExcludeBrands = excludebrands!= null ? excludebrands.ToList() : null;
            model.ExcludeBrandsOrg = model.ExcludeBrands;

            model.Competitors = sortedCompetitors;
            model.ExcludeCompetitorsOrg = model.ExcludeCompetitors;

            return View(model);
        }

         [System.Web.Mvc.HttpGet]
        public ActionResult SaveExcludeBrands(string excludbrands)
        {
            SiteViewModel site = new SiteViewModel();
            site.ExcludeBrands = excludbrands.Split(',').ToList();
            _serviceFacade.UpdateExcludeBrands(site);
            return Json("Saved");
        }

         [System.Web.Mvc.HttpGet]
         public ActionResult ExportSiteswithPrices(string date = null, string storeName = "",
            int catNo = 0, int storeNo = 0,
            string storeTown = "", int siteId = 0)
         {
             DateTime forDate = DateTime.Now;
             if (!DateTime.TryParse(date, out forDate))
             {
                 string[] tokenize = date.Split('/');
                 date = tokenize[2] + "/" + tokenize[1] + "/" + tokenize[0];
                 forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
             }
             // forDate = forDate.AddDays(-1);
             IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = _serviceFacade.GetSitePrices(forDate, storeName, catNo, storeNo, storeTown, siteId, 1, 2000);
              var dt = SitesWithPricesToDataTable(forDate, sitesViewModelsWithPrices);
             string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));
             return ExcelDocumentStream(new List<DataTable> { dt }, "SiteWithPrices", filenameSuffix, null);
         }

         public DataTable SitesWithPricesToDataTable(DateTime forDate,
             IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices)
         {
             var dt = new DataTable("Sites");
             dt.Columns.Add("StoreNo.");
             dt.Columns.Add("Store Name");
             dt.Columns.Add("Store Town");
             dt.Columns.Add("Cat No.");
             dt.Columns.Add("PFS No.");
             dt.Columns.Add("UnLeaded ");
             dt.Columns.Add("Diesel ");
             dt.Columns.Add("Super Unleaded ");
             DataRow dr = dt.NewRow();
             dr[5] = forDate.ToString("dd/MM/yyyy");
             dt.Rows.Add(dr);
             int nRow = 2;
             Dictionary<int, int> dicColtoFType = new Dictionary<int, int>();
             dicColtoFType.Add(2, 5);
             dicColtoFType.Add(6, 6);
             dicColtoFType.Add(1, 7);

             foreach (var siteVM in sitesViewModelsWithPrices)
             {
                 dr = dt.NewRow();
                 dr[0] = siteVM.StoreNo;
                 dr[1] = siteVM.StoreName;
                 dr[2] = siteVM.Town;
                 dr[3] = siteVM.CatNo;
                 dr[4] = siteVM.PfsNo;

                 if (siteVM.FuelPrices != null)
                 {
                     foreach (var fp in siteVM.FuelPrices)
                     {
                         if (dicColtoFType.ContainsKey(fp.FuelTypeId))
                         {
                             if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId]]) dr[dicColtoFType[fp.FuelTypeId]] = (fp.TodayPrice / 10.0).ToString();
                            
                         }
                     }
                 }
                 dt.Rows.Add(dr);
                 nRow = nRow + 1;


             }
             return dt;
         }
      

         [System.Web.Mvc.HttpGet]
         public ActionResult ExportPrices(string date = null, string storeName = "",
            int catNo = 0, int storeNo = 0,
            string storeTown = "", int siteId = 0)
         {
             DateTime forDate = DateTime.Now;
             if (!DateTime.TryParse(date, out forDate)) 
             {
                   string[] tokenize = date.Split('/');
                   date = tokenize[2]+"/"+ tokenize[1]+ "/" + tokenize[0];
                   forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
             }
            // forDate = forDate.AddDays(-1);
             IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = _serviceFacade.GetSitePrices(forDate, storeName, catNo, storeNo, storeTown, siteId, 1, 2000);
             Dictionary<int, int> dicgroupRows = new Dictionary<int, int>();
             var dt = SitePricesToDataTable(forDate, sitesViewModelsWithPrices, ref  dicgroupRows);
             string filenameSuffix = String.Format("[{0}]", forDate.ToString("dd-MMM-yyyy"));
             return ExcelDocumentStream(new List<DataTable> { dt }, "SitePricesWithCompetitors", filenameSuffix, dicgroupRows);
         }
         public DataTable SitePricesToDataTable(DateTime forDate,
             IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices, ref  Dictionary<int, int> dicgroupRows)
         {
             var dt = new DataTable("Site Pricing");
             dt.Columns.Add("StoreNo.");
             dt.Columns.Add("Store Name");
             dt.Columns.Add("Store Town");
             dt.Columns.Add("Cat No.");
             dt.Columns.Add("PFS No.");
             dt.Columns.Add("UnLeaded ");
             dt.Columns.Add("UnLeaded");
             dt.Columns.Add("Diff");
             dt.Columns.Add("Diesel ");
             dt.Columns.Add("Diesel");
             dt.Columns.Add("Diff ");
             dt.Columns.Add("Super Unleaded ");
             dt.Columns.Add("Super Unleaded");
             dt.Columns.Add("Diff  ");
             DataRow dr = dt.NewRow();
             DateTime tomorrow = forDate.AddDays(1);
             DateTime yday = forDate.AddDays(-1);
             DateTime daybyday = yday.AddDays(-1);
             dr[5] = yday.ToString("dd/MM/yyyy");
             dr[6] = tomorrow.ToString("dd/MM/yyyy");
             dr[8] = yday.ToString("dd/MM/yyyy");
             dr[9] = tomorrow.ToString("dd/MM/yyyy");
             dr[11] = yday.ToString("dd/MM/yyyy");
             dr[12] = tomorrow.ToString("dd/MM/yyyy");
             dt.Rows.Add(dr);
             int nRow = 2;
             Dictionary<int, int> dicColtoFType = new Dictionary<int, int>();
             dicColtoFType.Add(2, 5);
             dicColtoFType.Add(6, 8);
             dicColtoFType.Add(1, 11);

            foreach(var siteVM in sitesViewModelsWithPrices)
             {
                 dr = dt.NewRow();
                 dr[0] = siteVM.StoreNo;
                 dr[1] = siteVM.StoreName;
                 dr[2] = siteVM.Town;
                 dr[3] = siteVM.CatNo;
                 dr[4] = siteVM.PfsNo;
                
                 if (siteVM.FuelPrices != null)
                 {
                     foreach (var fp in siteVM.FuelPrices)
                     {
                         if (dicColtoFType.ContainsKey(fp.FuelTypeId))
                         {
                             if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId]]) dr[dicColtoFType[fp.FuelTypeId]] = (fp.TodayPrice / 10.0).ToString();
                             if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 1]) dr[dicColtoFType[fp.FuelTypeId] + 1] = (fp.AutoPrice / 10.0).ToString();
                             if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 2])
                             {
                                 dr[dicColtoFType[fp.FuelTypeId] + 2] = fp.AutoPrice > 0 && fp.TodayPrice>0 ? ((fp.AutoPrice - fp.TodayPrice) / 10.0).ToString() : "n/a";
                             }
                         }
                     }
                 }
                 dt.Rows.Add(dr);
                 nRow = nRow+1;
                
                 //Adding Competitors
                if (siteVM.competitors == null) siteVM.competitors = _serviceFacade.GetCompetitorsWithPrices(forDate.AddDays(-1), siteVM.SiteId, 1, 2000).OrderBy(x => x.DriveTime).ToList();
           
                 if (siteVM.competitors != null)
                 {
                     dr = dt.NewRow();
                     dr[1] = "Brand";
                     dr[2] = "Maker";
                     dr[3] = "Drive-Time";
                     dr[4] = "Cat No.";
                     dr[5] = "UnLeaded";
                     dr[6] = "UnLeaded ";
                     dr[7] = "Diff";
                     dr[8] = "Diesel";
                     dr[9] = "Diesel ";
                     dr[10] = "Diff ";
                     dr[11] = "Super Unleaded";
                     dr[12] = "Super Unleaded ";
                     dr[13] = "Diff  ";
                     dt.Rows.Add(dr);
                     dr = dt.NewRow();
                     dr[5] = daybyday.ToString("dd/MM/yyyy");
                     dr[6] = yday.ToString("dd/MM/yyyy");
                     dr[8] = daybyday.ToString("dd/MM/yyyy");
                     dr[9] = yday.ToString("dd/MM/yyyy");
                     dr[11] = daybyday.ToString("dd/MM/yyyy");
                     dr[12] = yday.ToString("dd/MM/yyyy");
                     dt.Rows.Add(dr);
                      foreach (var compitetorVM in siteVM.competitors)
                     {
                         dr = dt.NewRow();
                         dr[1] = compitetorVM.Brand;
                         dr[2] = compitetorVM.StoreName;
                         dr[3] = compitetorVM.DriveTime;
                         dr[4] = compitetorVM.CatNo;
                         if (compitetorVM.FuelPrices != null)
                         {
                             foreach (var fp in compitetorVM.FuelPrices)
                             {
                                 if (dicColtoFType.ContainsKey(fp.FuelTypeId))
                                 {
                                     if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId]]) dr[dicColtoFType[fp.FuelTypeId]] = (fp.YestPrice / 10.0).ToString();
                                     if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 1]) dr[dicColtoFType[fp.FuelTypeId] + 1] = (fp.TodayPrice / 10.0).ToString();
                                     if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 2])
                                     {
                                         dr[dicColtoFType[fp.FuelTypeId] + 2] = fp.TodayPrice > 0 && fp.YestPrice > 0 ? ((fp.TodayPrice - fp.YestPrice) / 10.0).ToString() : "n/a";
                                     }
                                 }
                             }
                         }
                         dt.Rows.Add(dr);
                        
                     }
                      dr = dt.NewRow();
                      dt.Rows.Add(dr);
                     dicgroupRows.Add(nRow, siteVM.competitors.Count+2);
                 }

                
             }
             return dt;
         }
         private ActionResult ExcelDocumentStream(List<DataTable> tables, string fileName, string fileNameSuffix, Dictionary<int, int> dicgroupRows)
         {
             using (var wb = new ClosedXML.Excel.XLWorkbook())
             {
                 foreach (var dt in tables)
                 {
                     var ws = wb.Worksheets.Add(dt);
                     int TotalRows = ws.RowCount();

                     for (int i = 2; i < TotalRows; i++)
                     {
                          ChangeCellColor(ws.Cell(i, 8));
                          ChangeCellColor(ws.Cell(i, 11));
                          ChangeCellColor(ws.Cell(i, 14));
                     }
                     if (dicgroupRows != null)
                     {
                         int nSiteRow = 3;
                         int nRow = 3;
                         while (dicgroupRows.ContainsKey(nRow))
                         {


                             int nCompitetors = dicgroupRows[nRow];

                             var cellrange = string.Format("A{0}:N{1}", nSiteRow + 1, nSiteRow + nCompitetors);
                             var cellrangesecondRow = "A2:N2";
                             ws.Range(cellrangesecondRow)
                                 .Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);
                             ws.Range(cellrange).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);
                             ws.Range(cellrange).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thick;
                             ws.Range(cellrange).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.Gray;

                             ws.Rows(nSiteRow + 1, nSiteRow + nCompitetors).Group();
                             ws.Rows(nSiteRow + 1, nSiteRow + nCompitetors).Collapse();
                             nSiteRow += nCompitetors + 2;
                             nRow++;
                         }
                     }


                 }
                 wb.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                 wb.Style.Font.Bold = true;
                 Response.Clear();
                 Response.Buffer = true;
                 Response.Charset = "";
                 Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                 string downloadHeader = String.Format("attachment;filename= {1}{0}.xlsx",
                     fileNameSuffix,
                     fileName);
                 Response.AddHeader("content-disposition", downloadHeader);
                 using (var myMemoryStream = new System.IO.MemoryStream())
                 {
                     wb.SaveAs(myMemoryStream);
                     myMemoryStream.WriteTo(Response.OutputStream);
                     Response.Flush();
                     Response.End();
                     return new EmptyResult();
                 }
             }
         }

        private void ChangeCellColor(IXLCell cell)
        {
            int iValue = 0;
            bool bResult=Int32.TryParse(cell.Value.ToString(), out iValue);
            if (Convert.ToString(cell.Value).Trim() == "n/a")
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
            }
            else if (Convert.ToString(cell.Value).Trim() == "Diff")
            {
               // cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
            }
            else if (bResult && iValue > 0)
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
            }
            else if (bResult && iValue <0)
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
            }
        }
        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(SiteViewModel site)
        {
			var model = _serviceFacade.GetSite(site.Id);

			var sortedCompetitors = model.Competitors.Where(c => c.IsSainsburysSite == false).OrderBy(c => c.SiteName).ToList();


            model.ExcludeCompetitors = model.ExcludeCompetitors.Distinct().ToList();
			site.Competitors = sortedCompetitors;
            site.ExcludeCompetitorsOrg = model.ExcludeCompetitors;
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please check for validation errors under each field.";

                return View(site);
            }

            var nonBlankVals = new List<SiteEmailViewModel>();
            site.IsSainsburysSite = true; //Only Sainsburys sites are editable
            ListExtensions.ForEach(site.Emails, x =>
            {
                if (!x.EmailAddress.IsNullOrWhiteSpace())
                {
                    x.SiteId = site.Id;
                    nonBlankVals.Add(x);
                }
            });
            site.Emails = nonBlankVals;

            var editSite = _serviceFacade.EditSite(site);

            if (editSite.ViewModel != null)
				return RedirectToAction("Index", new { msg = "Site: " + editSite.ViewModel.SiteName + " updated successfully" });

			ViewBag.ErrorMessage = editSite.ErrorMessage;
			return View(site);
        }

    }
}