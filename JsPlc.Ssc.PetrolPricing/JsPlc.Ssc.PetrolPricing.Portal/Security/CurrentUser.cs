using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Security
{
    public static class CurrentUser
    {
        private const string UserAccessCacheKey = "UserAccess";

        private static ILogger _logger;
        private static ServiceFacade _serviceFacade;

        static CurrentUser()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        public static UserAccessViewModel GetUserAccess(HttpRequestBase httpRequest)
        {
            if (!httpRequest.IsAuthenticated)
                return new UserAccessViewModel();

            var userName = "";

            IOwinContext context = httpRequest.GetOwinContext();
            if (context != null && context.Authentication != null && context.Authentication.User != null)
                userName = (context.Authentication.User.Identity.Name + "#").Split('#')[0];

            if (String.IsNullOrWhiteSpace(userName))
                return new UserAccessViewModel();

            var model = System.Web.HttpContext.Current.Items[UserAccessCacheKey] as UserAccessViewModel;
            if (model == null)
            {
                model = _serviceFacade.GetUserAccessModel(userName);
                System.Web.HttpContext.Current.Items[UserAccessCacheKey] = model;
            }
            return model;
        }
    }
}