using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;
using JsPlc.Ssc.PetrolPricing.Models;
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

        // BOILERPLATE Code Only - not functional
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> PostSiteForm([FromBody] Site siteView)
        {
            if (ModelState.IsValid)
            {
                var siteViewJson = JsonConvert.SerializeObject(siteView);
                var response = await _serviceFacade.RunAsync(siteViewJson, HttpMethod.Post);
                if (!response.IsSuccessStatusCode) return response.ToJsonResult(null, null, "ApiFail");

                var meetingUrl = response.Headers.Location;
                return response.ToJsonResult(meetingUrl.AbsoluteUri, null, "ApiSuccess");
            }
            var errArray = this.GetUiErrorList();
            var badRequestResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };
            // key and string of arrays
            return badRequestResponse.ToJsonResult(null, errArray, "UIValidationErrors");
        }

        // AJAX Callable
        // BOILERPLATE Code Only - not functional
        [ScriptMethod(UseHttpGet = true)]
        public JsonResult GetLinkForm(int siteId)
        {
            // POST scenarios use : JsonConvert.SerializeObject(meetingView);

            var facade = _serviceFacade;

            var site = facade.GetSite(siteId);

            var jsonData = site != null ? (object) site : "Error";

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

            var model = _serviceFacade.GetSites();
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

        public ActionResult Prices(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;

            // var model = _serviceFacade.GetSites();
            return View();
        }

        public ActionResult Edit(int id)
        {
            var model = _serviceFacade.GetSite(id);
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(Site site)
        {
            site.IsSainsburysSite = true;
            var nonBlankVals = new List<SiteEmail>();

            var editSite = _serviceFacade.EditSite(site);

            if (editSite != null) return RedirectToAction("Index", new { msg = "Site: " + editSite.SiteName + " updated successfully" });

            ViewBag.ErrorMessage = "Unable to create site.";
            return View();
        }

    }
}