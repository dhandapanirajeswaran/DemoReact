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
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
                return;

            HttpCookie authCookiePath = Request.Cookies[FormsAuthentication.FormsCookiePath];
            if (authCookiePath != null)
                return;

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);


            if (null == authTicket)
                return;

            if (authTicket.Expired)
            {
                authCookie.Expires = DateTime.Now.AddDays(-1d);
                Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
                var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookiePath, "");
                faCookie.Expires = DateTime.Now.AddMinutes(timeout);
                HttpContext.Current.Response.Cookies.Add(faCookie);

                
                Response.Redirect("~/Account/LogOff");                   
                return;
            }

            HttpContext.Current.User = new CustomPrincipal(authTicket.UserData);

            if (null == authTicket.UserData)
               return;
                       
            FormsAuthentication.RenewTicketIfOld(authTicket);
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
    }
}
