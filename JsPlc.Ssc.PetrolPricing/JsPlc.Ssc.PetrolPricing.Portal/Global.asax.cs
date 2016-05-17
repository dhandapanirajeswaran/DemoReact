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
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public class MvcApplication : System.Web.HttpApplication
    {
       
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //RepositoryInit.InitializeDatabase();
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
