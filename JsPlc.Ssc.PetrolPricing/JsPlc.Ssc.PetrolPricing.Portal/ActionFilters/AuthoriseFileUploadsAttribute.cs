using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Portal.Security;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.ActionFilters
{
    public class AuthoriseFileUploadsAttribute : ActionFilterAttribute
    {
        public FileUploadsUserPermissions Permissions { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var checker = new AuthorisationPermissionChecker(filterContext);
            var actualPermissions = (int)checker.UserAccess.UserFileUploadsAccess.Permissions;
            checker.EnforcePermissions((int)Permissions, actualPermissions);
        }
    }
}