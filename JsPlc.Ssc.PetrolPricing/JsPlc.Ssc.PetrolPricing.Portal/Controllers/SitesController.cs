using System;
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
using JsPlc.Ssc.PetrolPricing.Portal.DataExporters;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Exporting.Exporters;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{

    [System.Web.Mvc.Authorize]
    public class SitesController : BaseController
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public SitesController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }
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
                _logger.Error(ex);
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
        public async Task<JsonResult> SendEmailToSite(int emailTemplateId, string siteIdsList)
        {
            List<EmailSendLog> sendLog = null;
              try
            {
                // Email all sites
                var response = await _serviceFacade.EmailUpdatedPricesSites(emailTemplateId, siteIdsList);
                sendLog = response;
                var sendSummaryString = sendLog.ToSendSummary();
                return (response == null || !response.Any())
                    ? new HttpResponseMessage(HttpStatusCode.BadRequest).ToJsonResult(response, null, "ApiFail",
                        "Error: unable to send emails, please check each status for per site - Errors or warnings near envelope icon..\n")
                    : new HttpResponseMessage(HttpStatusCode.OK).ToJsonResult(sendLog, null, "ApiSuccess", "", sendSummaryString);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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

            DiagnosticLog.AddLog("Debug", "Started: GetSitesWithPricesJson");

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

            /*  Task.Run(() =>
               {
                   Dictionary<int, int> dicgroupRows = new Dictionary<int, int>();
                   var dt = SitePricesToDataTable(forDate, sitesViewModelsWithPrices, ref  dicgroupRows);

               });*/

            DiagnosticLog.AddLog("Debug", "Finished: GetSitesWithPricesJson");

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
                _logger.Error(ex);
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
            catch (Exception ce)
            {
                _logger.Error(ce);
                return View();
            }
        }

        public ActionResult Create()
        {
            var model = new SiteViewModel();

            return View(new SiteViewModel());
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(SiteViewModel site)
        {
            ValidateSiteEditOrCreate(site);

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

        public ActionResult NearbySites(int id)
        {
            var model = _serviceFacade.GetNearbyCompetitorSites(id);
            return View(model);
        }

        /// <summary>
        /// Works on List of SitePriceViewModel to build core page
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ActionResult Prices(int x = 0, string msg = "", string date = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;
            //var sitesViewModelsWithPrices = _serviceFacade.GetSitePrices();
            // return empty list but never null

            SiteViewModel model = new SiteViewModel();

            var requestDate = String.IsNullOrEmpty(date) ? DateTime.Now.Date : DateTime.Parse(date);

            model.RecentFileUploads = _serviceFacade.GetRecentFileUploadSummary();
            model.PriceSnapshot = _serviceFacade.GetPriceSnapshotForDay(requestDate) ?? new PriceSnapshotViewModel();

            var allBrands = _serviceFacade.GetBrands();
            model.AllBrands = allBrands != null ? allBrands.ToList() : new List<string>();
            var excludebrands = _serviceFacade.GetExcludeBrands();
            model.ExcludeBrands = excludebrands != null ? excludebrands.ToList() : null;
            model.ExcludeBrandsOrg = model.ExcludeBrands;

            PopulatePageData(model, requestDate);

            return View("Prices", model); // Razor based view
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

        public ActionResult Edit(int id, SiteSectionType calledFromSection = SiteSectionType.None)
        {
            var model = _serviceFacade.GetSite(id);
            model.CalledFromSection = calledFromSection;

            var sortedCompetitors = model.Competitors.OrderBy(c => c.SiteName).ToList();

            sortedCompetitors.Insert(0, new SiteViewModel
            {
                SiteName = "------- Not specified --------"
            });

            model.ExcludeCompetitors = model.ExcludeCompetitors.Distinct().ToList();
            model.AllBrands = _serviceFacade.GetBrands().ToList();
            var excludebrands = _serviceFacade.GetExcludeBrands();
            model.ExcludeBrands = excludebrands != null ? excludebrands.ToList() : null;
            model.ExcludeBrandsOrg = model.ExcludeBrands;

            model.Competitors = sortedCompetitors;
            model.ExcludeCompetitorsOrg = model.ExcludeCompetitors;

            return View(model);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult SaveExcludeBrands(string excludbrands)
        {
            SiteViewModel site = new SiteViewModel();
            site.ExcludeBrands = excludbrands == "null" ? new List<String>() : excludbrands.Split(',').ToList();
            _serviceFacade.UpdateExcludeBrands(site);
            return Json("Saved", JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult ExportSiteswithPrices(string downloadId, string date = null, string storeName = "",
           int catNo = 0, int storeNo = 0,
           string storeTown = "", int siteId = 0)
        {
            if (String.IsNullOrWhiteSpace(downloadId))
                throw new ArgumentException("DownloadId cannot be empty!");

            DateTime forDate = DateTime.Now;
            if (!DateTime.TryParse(date, out forDate))
            {
                string[] tokenize = date.Split('/');
                date = tokenize[2] + "/" + tokenize[1] + "/" + tokenize[0];
                forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
            }
            // forDate = forDate.AddDays(-1);
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = _serviceFacade.GetSitePrices(forDate, storeName, catNo, storeNo, storeTown, siteId, 1, 2000);

            var pfsList = _serviceFacade.GetJsSitesByPfsNum();
            var workbook = new JsSitesWithPricesExporter().ToExcelWorkbook(forDate, sitesViewModelsWithPrices, pfsList);
            var excelFilename = String.Format("SiteWithPrices[{0}].xlsx", forDate.ToString("dd-MMM-yyyy"));
            return base.SendExcelFile(excelFilename, workbook, downloadId);
        }

        private void ValidateSiteEditOrCreate(SiteViewModel site)
        {
            switch (site.PriceMatchType)
            {
                case PriceMatchType.StandardPrice:
                    site.CompetitorPriceOffset = 0.0;
                    site.TrailPriceCompetitorId = null;
                    site.CompetitorPriceOffsetNew = 0;
                    break;

                case PriceMatchType.TrailPrice:
                    site.TrailPriceCompetitorId = null;
                    site.CompetitorPriceOffsetNew = 0;
                    break;

                case PriceMatchType.MatchCompetitorPrice:
                    if (!site.TrailPriceCompetitorId.HasValue || site.TrailPriceCompetitorId.Value == 0)
                        ModelState.AddModelError("TrailPriceCompetitorId", "Please choose a Competitor Site");
                    else
                        site.CompetitorPriceOffset = 0.0;
                    break;
            }
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult ExportPrices(string downloadId, string date = null, string storeName = "",
           int catNo = 0, int storeNo = 0,
           string storeTown = "", int siteId = 0)
        {
            if (String.IsNullOrWhiteSpace(downloadId))
                throw new ArgumentException("DownloadId cannot be empty!");

            DateTime forDate = DateTime.Now;
            if (!DateTime.TryParse(date, out forDate))
            {
                string[] tokenize = date.Split('/');
                date = tokenize[2] + "/" + tokenize[1] + "/" + tokenize[0];
                forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
            }
            // forDate = forDate.AddDays(-1);
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = _serviceFacade.GetSitePrices(forDate, storeName, catNo, storeNo, storeTown, siteId, 1, 2000);


            var getCompetitorsWithPricesService = new GetCompetitorsWithPricesService(_serviceFacade);

            var workbook = new AllSitesPricesExporter().ToExcelWorkbook(sitesViewModelsWithPrices, forDate, getCompetitorsWithPricesService);
            var excelFilename = String.Format("SitePricesWithCompetitors[{0}].xlsx", forDate.ToString("dd-MMM-yyyy"));
            return base.SendExcelFile(excelFilename, workbook, downloadId);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult ExportCompPrices(string downloadId, string date = null)
        {
            if (String.IsNullOrEmpty(downloadId))
                throw new ArgumentException("DownloadId cannot be empty!");

            DateTime forDate = DateTime.Now;
            if (!DateTime.TryParse(date, out forDate))
            {
                string[] tokenize = date.Split('/');
                date = tokenize[2] + "/" + tokenize[1] + "/" + tokenize[0];
                forDate = new DateTime(Convert.ToInt16(tokenize[2]), Convert.ToInt16(tokenize[1]), Convert.ToInt16(tokenize[0]));
            }

            ExportCompetitorPricesViewModel compPrices = new ExportCompetitorPricesViewModel();
            compPrices.SainsburysSitePrices = _serviceFacade.GetSitePrices(forDate, "", 0, 0, "", 0);
            compPrices.DriveTimeMarkups = _serviceFacade.GetAllDriveTimeMarkups();

            var siteIds = compPrices.SainsburysSitePrices.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);
            compPrices.CompetitorPrices = _serviceFacade.GetCompetitorsWithPrices(forDate, 0, 1, 2000, siteIds);

            var workbook = new CompetitorPricesExporter().ToExcelWorkbook(compPrices, forDate);
            var excelFilename = String.Format("AllCompetitorPrices[{0}].xlsx", forDate.ToString("dd-MMM-yyyy"));
            return base.SendExcelFile(excelFilename, workbook, downloadId);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(SiteViewModel site)
        {
			var model = _serviceFacade.GetSite(site.Id);

            ValidateSiteEditOrCreate(site);

			List<SiteViewModel> sortedCompetitors = model.Competitors.OrderBy(c => c.SiteName).ToList();

            sortedCompetitors.Insert(0, new SiteViewModel
            {
                SiteName = "------- Not specified --------"
            });

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

            bool isRecalcRequired = DoesSiteEditRequireRecalculation(model, site);

            var editSite = _serviceFacade.EditSite(site);

            if (isRecalcRequired)
            {
                _serviceFacade.TriggerDailyPriceRecalculation(DateTime.Now.Date);
            }
            if (editSite.ViewModel != null)
            {
                if (site.CalledFromSection == SiteSectionType.SitePricing)
                    return RedirectToAction("Prices", "Sites");
                else if (site.CalledFromSection == SiteSectionType.SiteEmails)
                    return RedirectToAction("SiteEmails", "Sites");
                else
                    return RedirectToAction("Index", new { msg = "Site: " + editSite.ViewModel.SiteName + " updated successfully" });
            }

			ViewBag.ErrorMessage = editSite.ErrorMessage;

           
			return View(site);
        }

        /// <summary>
        /// Determine if an edit to a Site would trigger a recalculation of prices
        /// </summary>
        /// <param name="model"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool DoesSiteEditRequireRecalculation(SiteViewModel model, SiteViewModel site)
        {
            var excludeCompetitorsAreDifferent = String.Join(",", model.ExcludeCompetitors ?? new List<int>()) != String.Join(",", site.ExcludeCompetitors ?? new List<int>());
            var excludeBrandsAreDifferent = String.Join(",", model.ExcludeBrands ?? new List<string>()) != String.Join(",", site.ExcludeBrands ?? new List<string>());

            return model.IsActive != site.IsActive
                || model.IsSainsburysSite != site.IsSainsburysSite
                || model.PriceMatchType != site.PriceMatchType
                || excludeCompetitorsAreDifferent
                || excludeBrandsAreDifferent
                || model.TrailPriceCompetitorId != site.TrailPriceCompetitorId
                || model.CompetitorPriceOffset != site.CompetitorPriceOffset
                || model.CompetitorPriceOffsetNew != site.CompetitorPriceOffsetNew;
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult GetSiteNote([FromUri]int siteId)
        {
            var response = _serviceFacade.GetSiteNote(siteId);

            if (response == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    .ToJsonResult(response, null, "ApiFail");

            return new HttpResponseMessage(HttpStatusCode.OK)
                .ToJsonResult(response, null, "ApiSuccess");
        }

        [ValidateInput(false)]
        [System.Web.Mvc.HttpPost]
        public ActionResult UpdateSiteNote([FromBody]SiteNoteUpdateViewModel model)
        {
            var result = _serviceFacade.UpdateSiteNote(model);

            var jsonResult = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                Data = result
            };

            return jsonResult;
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult DeleteSiteNote([FromUri] int siteId)
        {
            var result = _serviceFacade.DeleteSiteNode(siteId);
            var jsonResult = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                Data = result
            };

            return jsonResult;
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult RecalculateDailyPrices()
        {
            var when = DateTime.Now.Date;
            var result = _serviceFacade.RecalculateDailyPrices(when);
            return base.JsonGetResult(result);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult SitePricingSettings()
        {
            var result = _serviceFacade.GetSitePricingSettings();

            var jsonResult = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = result
            };
            return jsonResult;
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult TriggerDailyPriceRecalculation()
        {
            var result = _serviceFacade.TriggerDailyPriceRecalculation(DateTime.Now.Date);
            return base.StandardJsonResultMessage(result);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult RemoveAllSiteEmailAddresses()
        {
            var result = _serviceFacade.RemoveAllSiteEmailAddresses();
            return base.StandardJsonResultMessage(result);
        }

        public ActionResult SiteEmails()
        {
            var model = new SiteEmailsPageViewModel()
            {
                SiteEmails = _serviceFacade.GetAllSiteEmailAddresses(),
                SystemSettings = _serviceFacade.GetSystemSettings()
            };

            return View(model);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult ExportSiteEmails(string downloadId)
        {
            if (String.IsNullOrWhiteSpace(downloadId))
                throw new ArgumentException("DownloadId cannot be empty!");

            var emailAddreses = _serviceFacade.GetAllSiteEmailAddresses().OrderBy(x => x.StoreName).ToList();
            var workbook = new SiteEmailAddressesExporter().ToExcelWorkbook(emailAddreses);
            var excelFilename = String.Format("SiteEmailAddresses[{0}].xlsx", DateTime.Now.ToString("dd-MMM-yyyy"));
            return base.SendExcelFile(excelFilename, workbook, downloadId);
        }

        public ActionResult UploadSiteEmails()
        {
            var model = new SiteEmailImportResultViewModel();
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UploadSiteEmails(HttpPostedFileBase file)
        {
            var model = new SiteEmailImportResultViewModel();
            try
            {
                // for some reason model is not populating
                var settings = new ImportSiteEmailSettings()
                {
                    ImportCatNo = !String.IsNullOrEmpty(Request.Form["ImportCatNo"]),
                    ImportPfsNo = !String.IsNullOrEmpty(Request.Form["ImportPfsNo"]),
                    AllowSharedEmails = !String.IsNullOrEmpty(Request.Form["AllowSitesToShareEmails"])
                };

                if (file == null || file.ContentLength <= 0)
                {
                    _logger.Error(new ApplicationException("Uploaded Site Emails file is empty"));
                    throw new ApplicationException("Uploaded Site Emails file is empty");
                }

                model = _serviceFacade.ImportFileEmailFile(file, settings);
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                model.Status.ErrorMessage = "Unable to upload Site Emails file";
            }
            return View(model);
        }

        [ScriptMethod(UseHttpGet = true)]
        public JsonResult HistoricPricesForSite([FromUri] int siteId, [FromUri] string startDate, [FromUri] string endDate)
        {
            DateTime startingDate;
            DateTime endingDate;
            if (!DateTime.TryParse(startDate, out startingDate))
                startingDate = DateTime.Now.Date;
            if (!DateTime.TryParse(endDate, out endingDate))
                endingDate = startingDate.Date.AddDays(-14);

            var historicPrices = _serviceFacade.GetHistoricPricesForSite(siteId, startingDate, endingDate);

            var jsonResult = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = historicPrices
            };
            return jsonResult;
        }

        [ScriptMethod(UseHttpGet =true)]
        public JsonResult GetPriceFreezeEventForDate(DateTime forDate)
        {
            var result = _serviceFacade.GetPriceFreezeEventForDate(forDate);
            return base.JsonGetResult(result);
        }

        [ScriptMethod(UseHttpGet =true)]
        public JsonResult GetSiteEmailTodaySendStatuses([FromUri] string forDate)
        {
            DateTime theDate;
            if (!DateTime.TryParse(forDate, out theDate))
                theDate = DateTime.Now.Date;

            var statuses = _serviceFacade.GetSiteEmailTodaySendStatuses(theDate);
            return base.JsonGetResult(statuses);
        }

        [ScriptMethod(UseHttpGet =true)]
        public JsonResult GetJsPriceOverrides([FromUri] int fileUploadId)
        {
            JsPriceOverrideViewModel model = _serviceFacade.GetJsPriceOverrides(fileUploadId);
            return base.JsonGetResult(model);
        }

        [ScriptMethod(UseHttpGet = true)]
        public JsonResult GetEmailSendLogView(int emailSendLogId)
        {
            var model = _serviceFacade.GetEmailSendLogView(emailSendLogId);
            return base.JsonGetResult(model);
        }


        #region private methods

        private void PopulatePageData(SiteViewModel model, DateTime requestDate)
        {
            var pagedata = model.PageData;

            var lastDailyPriceDataUpload = model.RecentFileUploads.Files.FirstOrDefault(x => x.UploadTypeId == 1 && x.ImportStatusId == 10);
            if (lastDailyPriceDataUpload != null)
            {
                pagedata.DailyPriceData.IsMissing = false;
                pagedata.DailyPriceData.IsOutdated = lastDailyPriceDataUpload.UploadDateTime.Date != DateTime.Now.Date;
            }

            var latestJsPriceDataUpload = model.RecentFileUploads.Files.FirstOrDefault(x => x.UploadTypeId == 3 && x.ImportStatusId == 10 && x.UploadDateTime.Date == DateTime.Now.Date);
            if (latestJsPriceDataUpload != null)
            {
                pagedata.LatestPriceData.IsMissing = false;
            }

            if (model.PriceSnapshot != null)
            {
                pagedata.PriceSnapshot.IsActive = model.PriceSnapshot.IsActive;
                pagedata.PriceSnapshot.IsOutdated = model.PriceSnapshot.IsOutdated;
            }

            pagedata.PriceFreezeEvent = _serviceFacade.GetPriceFreezeEventForDate(requestDate);
        }

        #endregion
    }
}