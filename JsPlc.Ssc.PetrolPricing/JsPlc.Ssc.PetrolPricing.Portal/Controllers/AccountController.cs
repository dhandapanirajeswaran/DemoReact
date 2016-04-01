using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;

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
            return RedirectToAction("Index", "Home");
        }
    }
}