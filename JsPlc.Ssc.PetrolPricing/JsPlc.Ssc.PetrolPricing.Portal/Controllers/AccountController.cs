using System;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Web.Security;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;

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
            AuthenticationManager.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType,
              CookieAuthenticationDefaults.AuthenticationType);
            UiHelper.bIsFirstStartupAuthCalled = false;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
               
    }
}