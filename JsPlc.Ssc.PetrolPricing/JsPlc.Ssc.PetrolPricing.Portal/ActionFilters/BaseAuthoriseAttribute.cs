using JsPlc.Ssc.PetrolPricing.Portal.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.ActionFilters
{
    public class BaseAuthoriseAttribute : ActionFilterAttribute
    {
        protected void EnforceAuthorisationPermissions(ActionExecutingContext filterContext, int requiredPermissionsFlags, int actualPermissionsFlags)
        {
            var userAccess = CurrentUser.GetUserAccess(filterContext.HttpContext.Request);

            if (!userAccess.IsUserAuthenticated && requiredPermissionsFlags != 0)
                filterContext.Result = new RedirectResult("~/PleaseSignIn");
            else
            {
                if ((actualPermissionsFlags & requiredPermissionsFlags) != requiredPermissionsFlags)
                    filterContext.Result = new RedirectResult("~/AccessDenied");
            }
        }
    }
}