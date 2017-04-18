using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Security
{
    public class AuthorisationPermissionChecker
    {
        private ActionExecutingContext _filterContext;
        private UserAccessViewModel _userAccess;

        public AuthorisationPermissionChecker(ActionExecutingContext filterContext)
        {
            _filterContext = filterContext;
            _userAccess = CurrentUser.GetUserAccess(filterContext.HttpContext.Request);
        }

        public UserAccessViewModel UserAccess
        {
            get { return _userAccess; }
        }

        public void EnforcePermissions(int requiredPermissionsFlags, int actualPermissionsFlags)
        {
            if (_userAccess.IsUserAuthenticated && !_userAccess.IsActive)
                _filterContext.Result = new RedirectResult("~/Home/AccountInactive");
            else if (!_userAccess.IsUserAuthenticated && requiredPermissionsFlags != 0)
                _filterContext.Result = new RedirectResult("~/Home/PleaseSignIn");
            else if ((actualPermissionsFlags & requiredPermissionsFlags) != requiredPermissionsFlags)
                _filterContext.Result = new RedirectResult("~/Home/AccessDenied");
        }
    }
}