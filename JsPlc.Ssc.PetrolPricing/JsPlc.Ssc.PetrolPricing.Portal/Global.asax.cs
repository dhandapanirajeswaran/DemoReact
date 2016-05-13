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
           // if (HttpContext.Current.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
            {
                HttpCookie authCookieName = Request.Cookies[FormsAuthentication.FormsCookieName];
                var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
          
                if (authCookieName == null)
                {
                
                    HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                    faCookie.Expires = DateTime.Now.AddMinutes(timeout);
                    HttpContext.Current.Response.Cookies.Add(faCookie);
                    Response.Redirect("~/Account/LogOff");

                }
                

                 HttpCookie authCookiePath = Request.Cookies[FormsAuthentication.FormsCookiePath];
                 if (authCookiePath == null)
                 {
                     HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                     faCookie.Expires = DateTime.Now.AddMinutes(timeout);
                     HttpContext.Current.Response.Cookies.Add(faCookie);


                     HttpCookie faCookiePath = new HttpCookie(FormsAuthentication.FormsCookiePath, "");
                     faCookiePath.Expires = DateTime.Now.AddMinutes((timeout / 2));
                     HttpContext.Current.Response.Cookies.Add(faCookiePath);
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
