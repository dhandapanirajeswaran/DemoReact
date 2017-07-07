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

            // model.ExcludeBrands = _serviceFacade.GetBrands().ToList();

            var allBrands = _serviceFacade.GetBrands();
            model.AllBrands = allBrands != null ? allBrands.ToList() : new List<string>();
            var excludebrands = _serviceFacade.GetExcludeBrands();
            model.ExcludeBrands = excludebrands != null ? excludebrands.ToList() : null;
            model.ExcludeBrandsOrg = model.ExcludeBrands;

            PopulatePageData(model);

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

            var sortedCompetitors = model.Competitors.Where(c => c.IsSainsburysSite == false).OrderBy(c => c.SiteName).ToList();

            sortedCompetitors.Insert(0, new SiteViewModel
            {
                SiteName = "Not specified"
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

            var pfsList = GetJsSitesByPfsNum();
            var workbook = new JsSitesWithPricesExporter().ToExcelWorkbook(forDate, sitesViewModelsWithPrices, pfsList);
            var excelFilename = String.Format("SiteWithPrices[{0}].xlsx", forDate.ToString("dd-MMM-yyyy"));
            return base.SendExcelFile(excelFilename, workbook, downloadId);
        }

        private void ValidateSiteEditOrCreate(SiteViewModel site)
        {
            switch (site.PriceMatchType)
            {
                case PriceMatchType.SoloPrice:
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

        private List<int> GetJsSitesByPfsNum()
        {
            List<int> lstJsList = new List<int>();
            lstJsList.Add(578);
            lstJsList.Add(196);
            lstJsList.Add(1006);
            lstJsList.Add(200);
            lstJsList.Add(456);
            lstJsList.Add(64);
            lstJsList.Add(746);
            lstJsList.Add(1100);
            lstJsList.Add(1054);
            lstJsList.Add(165);
            lstJsList.Add(96);
            lstJsList.Add(586);
            lstJsList.Add(155);
            lstJsList.Add(459);
            lstJsList.Add(210);
            lstJsList.Add(150);
            lstJsList.Add(1008);
            lstJsList.Add(563);
            lstJsList.Add(477);
            lstJsList.Add(179);
            lstJsList.Add(439);
            lstJsList.Add(137);
            lstJsList.Add(223);
            lstJsList.Add(592);
            lstJsList.Add(127);
            lstJsList.Add(1835);
            lstJsList.Add(449);
            lstJsList.Add(1231);
            lstJsList.Add(896);
            lstJsList.Add(86);
            lstJsList.Add(238);
            lstJsList.Add(486);
            lstJsList.Add(159);
            lstJsList.Add(80);
            lstJsList.Add(487);
            lstJsList.Add(591);
            lstJsList.Add(1001);
            lstJsList.Add(91);
            lstJsList.Add(162);
            lstJsList.Add(476);
            lstJsList.Add(239);
            lstJsList.Add(148);
            lstJsList.Add(785);
            lstJsList.Add(1106);
            lstJsList.Add(240);
            lstJsList.Add(1071);
            lstJsList.Add(457);
            lstJsList.Add(134);
            lstJsList.Add(139);
            lstJsList.Add(178);
            lstJsList.Add(185);
            lstJsList.Add(233);
            lstJsList.Add(473);
            lstJsList.Add(138);
            lstJsList.Add(126);
            lstJsList.Add(583);
            lstJsList.Add(97);
            lstJsList.Add(176);
            lstJsList.Add(1010);
            lstJsList.Add(1073);
            lstJsList.Add(145);
            lstJsList.Add(221);
            lstJsList.Add(197);
            lstJsList.Add(189);
            lstJsList.Add(174);
            lstJsList.Add(218);
            lstJsList.Add(1067);
            lstJsList.Add(458);
            lstJsList.Add(577);
            lstJsList.Add(1549);
            lstJsList.Add(89);
            lstJsList.Add(232);
            lstJsList.Add(492);
            lstJsList.Add(493);
            lstJsList.Add(169);
            lstJsList.Add(82);
            lstJsList.Add(325);
            lstJsList.Add(192);
            lstJsList.Add(481);
            lstJsList.Add(168);
            lstJsList.Add(95);
            lstJsList.Add(77);
            lstJsList.Add(81);
            lstJsList.Add(465);
            lstJsList.Add(584);
            lstJsList.Add(92);
            lstJsList.Add(120);
            lstJsList.Add(461);
            lstJsList.Add(482);
            lstJsList.Add(1081);
            lstJsList.Add(1625);
            lstJsList.Add(149);
            lstJsList.Add(432);
            lstJsList.Add(597);
            lstJsList.Add(836);
            lstJsList.Add(99);
            lstJsList.Add(94);
            lstJsList.Add(114);
            lstJsList.Add(464);
            lstJsList.Add(483);
            lstJsList.Add(158);
            lstJsList.Add(234);
            lstJsList.Add(75);
            lstJsList.Add(1268);
            lstJsList.Add(1288);
            lstJsList.Add(151);
            lstJsList.Add(520);
            lstJsList.Add(66);
            lstJsList.Add(479);
            lstJsList.Add(180);
            lstJsList.Add(1015);
            lstJsList.Add(1114);
            lstJsList.Add(451);
            lstJsList.Add(141);
            lstJsList.Add(1107);
            lstJsList.Add(1105);
            lstJsList.Add(587);
            lstJsList.Add(152);
            lstJsList.Add(497);
            lstJsList.Add(431);
            lstJsList.Add(107);
            lstJsList.Add(184);
            lstJsList.Add(68);
            lstJsList.Add(469);
            lstJsList.Add(147);
            lstJsList.Add(491);
            lstJsList.Add(132);
            lstJsList.Add(494);
            lstJsList.Add(173);
            lstJsList.Add(575);
            lstJsList.Add(1005);
            lstJsList.Add(235);
            lstJsList.Add(1009);
            lstJsList.Add(111);
            lstJsList.Add(1011);
            lstJsList.Add(396);
            lstJsList.Add(452);
            lstJsList.Add(131);
            lstJsList.Add(478);
            lstJsList.Add(108);
            lstJsList.Add(213);
            lstJsList.Add(1269);
            lstJsList.Add(85);
            lstJsList.Add(438);
            lstJsList.Add(498);
            lstJsList.Add(1038);
            lstJsList.Add(582);
            lstJsList.Add(394);
            lstJsList.Add(175);
            lstJsList.Add(193);
            lstJsList.Add(1170);
            lstJsList.Add(692);
            lstJsList.Add(488);
            lstJsList.Add(446);
            lstJsList.Add(1246);
            lstJsList.Add(525);
            lstJsList.Add(110);
            lstJsList.Add(485);
            lstJsList.Add(495);
            lstJsList.Add(1046);
            lstJsList.Add(480);
            lstJsList.Add(112);
            lstJsList.Add(1063);
            lstJsList.Add(1059);
            lstJsList.Add(104);
            lstJsList.Add(76);
            lstJsList.Add(136);
            lstJsList.Add(182);
            lstJsList.Add(190);
            lstJsList.Add(230);
            lstJsList.Add(157);
            lstJsList.Add(594);
            lstJsList.Add(470);
            lstJsList.Add(181);
            lstJsList.Add(161);
            lstJsList.Add(78);
            lstJsList.Add(1004);
            lstJsList.Add(1548);
            lstJsList.Add(113);
            lstJsList.Add(1696);
            lstJsList.Add(177);
            lstJsList.Add(231);
            lstJsList.Add(1080);
            lstJsList.Add(472);
            lstJsList.Add(466);
            lstJsList.Add(1000);
            lstJsList.Add(90);
            lstJsList.Add(109);
            lstJsList.Add(1644);
            lstJsList.Add(1079);
            lstJsList.Add(187);
            lstJsList.Add(1061);
            lstJsList.Add(88);
            lstJsList.Add(219);
            lstJsList.Add(468);
            lstJsList.Add(1168);
            lstJsList.Add(133);
            lstJsList.Add(580);
            lstJsList.Add(1113);
            lstJsList.Add(877);
            lstJsList.Add(146);
            lstJsList.Add(754);
            lstJsList.Add(216);
            lstJsList.Add(467);
            lstJsList.Add(1003);
            lstJsList.Add(565);
            lstJsList.Add(209);
            lstJsList.Add(154);
            lstJsList.Add(1078);
            lstJsList.Add(186);
            lstJsList.Add(79);
            lstJsList.Add(1324);
            lstJsList.Add(98);
            lstJsList.Add(140);
            lstJsList.Add(211);
            lstJsList.Add(142);
            lstJsList.Add(135);
            lstJsList.Add(198);
            lstJsList.Add(84);
            lstJsList.Add(437);
            lstJsList.Add(199);
            lstJsList.Add(1295);
            lstJsList.Add(1244);
            lstJsList.Add(144);
            lstJsList.Add(143);
            lstJsList.Add(45);
            lstJsList.Add(103);
            lstJsList.Add(496);
            lstJsList.Add(1040);
            lstJsList.Add(156);
            lstJsList.Add(1610);
            lstJsList.Add(188);
            lstJsList.Add(191);
            lstJsList.Add(195);
            lstJsList.Add(445);
            lstJsList.Add(183);
            lstJsList.Add(194);
            lstJsList.Add(842);
            lstJsList.Add(83);
            lstJsList.Add(471);
            lstJsList.Add(129);
            lstJsList.Add(160);
            lstJsList.Add(1134);
            lstJsList.Add(1110);
            lstJsList.Add(1092);
            lstJsList.Add(1023);
            lstJsList.Add(1180);
            lstJsList.Add(1517);
            lstJsList.Add(1518);
            lstJsList.Add(1513);
            lstJsList.Add(1221);
            lstJsList.Add(1181);
            lstJsList.Add(1225);
            lstJsList.Add(1154);
            lstJsList.Add(1186);
            lstJsList.Add(1093);
            lstJsList.Add(1162);
            lstJsList.Add(1850);
            lstJsList.Add(1199);
            lstJsList.Add(1169);
            lstJsList.Add(1097);
            lstJsList.Add(1247);
            lstJsList.Add(1249);
            lstJsList.Add(1196);
            lstJsList.Add(1255);
            lstJsList.Add(1248);
            lstJsList.Add(1274);
            lstJsList.Add(1200);
            lstJsList.Add(1655);
            lstJsList.Add(1220);
            lstJsList.Add(1082);
            lstJsList.Add(1240);
            lstJsList.Add(1272);
            lstJsList.Add(1286);
            lstJsList.Add(1077);
            lstJsList.Add(1290);
            lstJsList.Add(1293);
            lstJsList.Add(1304);
            lstJsList.Add(1281);
            lstJsList.Add(1507);
            lstJsList.Add(1239);
            lstJsList.Add(1297);
            lstJsList.Add(1303);
            lstJsList.Add(1524);
            lstJsList.Add(1275);
            lstJsList.Add(1313);
            lstJsList.Add(1309);
            lstJsList.Add(1308);
            lstJsList.Add(1283);
            lstJsList.Add(1314);
            lstJsList.Add(1267);
            lstJsList.Add(1526);
            lstJsList.Add(1525);
            lstJsList.Add(1527);
            lstJsList.Add(1319);
            lstJsList.Add(1528);
            lstJsList.Add(1529);
            lstJsList.Add(1340);
            lstJsList.Add(1095);
            lstJsList.Add(1252);
            lstJsList.Add(1589);
            lstJsList.Add(1289);
            lstJsList.Add(1070);
            lstJsList.Add(1030);
            lstJsList.Add(1254);
            lstJsList.Add(1158);
            lstJsList.Add(1550);
            lstJsList.Add(1271);

            return lstJsList;

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

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(SiteViewModel site)
        {
			var model = _serviceFacade.GetSite(site.Id);

            ValidateSiteEditOrCreate(site);

			List<SiteViewModel> sortedCompetitors = model.Competitors.Where(c => c.IsSainsburysSite == false).OrderBy(c => c.SiteName).ToList();
           
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
                _serviceFacade.CalcDailyPrices(site.Id);
                _serviceFacade.TriggerDailyPriceRecalculation(DateTime.Now.Date);
            }
            if (editSite.ViewModel != null)
            {
                if (site.CalledFromSection == SiteSectionType.SitePricing)
                    return RedirectToAction("Prices", "Sites");
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
                || model.TrailPriceCompetitorId != site.TrailPriceCompetitorId;
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

        private void PopulatePageData(SiteViewModel model)
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
        }
    }
}