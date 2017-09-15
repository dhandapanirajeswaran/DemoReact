using System.Data.Entity;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web;
using System.Web.Security;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsPlc.Ssc.PetrolPricing.Portal.Models;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Diagnostics;
using System.Web.Http.ExceptionHandling;
using System.Xml.Linq;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using WebGrease;
using WebGrease.Extensions;
using JsPlc.Ssc.PetrolPricing.Portal.Helper;
using System.Reflection;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    //
    // NOTE: This is public API and does not require the [Authorize] attribute
    //

    public class PublicApiController : BaseController
    {
        private ILogger _logger;

        public PublicApiController()
        {
            _logger = new PetrolPricingLogger();
        }

        // GET: PublicApi
        public ActionResult Index()
        {
            var model = new StatusViewModel()
            {
                ErrorMessage = "Public listing of API is not available"
            };
            return base.JsonGetResult(model);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult PollEmailSchedule()
        {
            var serviceFacade = new ServiceFacade(_logger);
            var model = serviceFacade.ExecuteWinServiceSchedule();
            return base.JsonGetResult(model);
        }

    }
}