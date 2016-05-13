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
            if (HttpContext.Current.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
            {

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
                    HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                    faCookie.Expires = DateTime.Now.AddMinutes(timeout);
                    HttpContext.Current.Response.Cookies.Add(faCookie);
                }
                else
                {
                  
                    HttpContext.Current.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType,
             CookieAuthenticationDefaults.AuthenticationType);
                    Response.Redirect("~/Account/LogOff");
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
