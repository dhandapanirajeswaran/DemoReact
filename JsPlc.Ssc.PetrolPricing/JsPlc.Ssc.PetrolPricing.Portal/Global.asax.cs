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

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private  ILogger _logger ;

        public MvcApplication()
        {
            _logger = new PetrolPricingLogger();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            String StrAppPath = ConfigurationManager.AppSettings["LogFilePath"];

          /*  XDocument document = XDocument.Load(Server.MapPath("~/Web.config"));
            var log = document.Root.Element("elmah").Element("errorLog");

            log.SetAttributeValue("logPath", StrAppPath);
            document.Save(Server.MapPath("~/Web.config"));*/
        
           
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            _logger.Error(Server.GetLastError());
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (UiHelper.bIsFirstStartupAuthCalled)
            {
                HttpCookie authCookieName = Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookieName == null)
                {
                    UiHelper.CreateAuthCookie1();
                    Response.Redirect("~/Account/LogOff");
                }

                HttpCookie authCookiePath = Request.Cookies[FormsAuthentication.FormsCookiePath];
                if (authCookiePath == null)
                {
                    UiHelper.CreateAuthCookie1();
                    UiHelper.CreateAuthCookie2();
                }
            }
        }

       
        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
    }
}
