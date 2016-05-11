using System;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Web.Security;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
       
    [Authorize]
    public class AccountController : Controller
    {
        private IAuthenticationManager AuthenticationManager { get { return HttpContext.GetOwinContext().Authentication; } }

        public AccountController()
        {
        }

        [HttpGet]
        public ActionResult LogOff()
        {

            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.Now.AddDays(-1d);
                Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
                var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookiePath, "");
                faCookie.Expires = DateTime.Now.AddMinutes(timeout);
                HttpContext.Response.Cookies.Add(faCookie);
            }
       
            AuthenticationManager.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, 
                CookieAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Index", "Home");
        }
    }
}