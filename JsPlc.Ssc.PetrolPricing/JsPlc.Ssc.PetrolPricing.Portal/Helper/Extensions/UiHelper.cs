using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebGrease.Css.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public static class UiHelper
    {
        public static bool bIsFirstStartupAuthCalled=false;
        /// <summary>
        /// ErrorList as a List for View to highlight errors on Input fields
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string[]>> GetUiErrorList(this Controller controller)
        {
            var errorList = controller.ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            var errArray = new List<KeyValuePair<string, string[]>>();
            errorList.ForEach(kvp =>
            {
                if (kvp.Value.Any())
                {
                    errArray.Add(kvp);
                }
            });
            return errArray;
        }

        public static IEnumerable<KeyValuePair<string, string[]>> Errors(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                return modelState
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                    .Where(m => m.Value.Any());
            }

            return null;
        }

        public static void CreateAuthCookie1()
        {
            var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
          
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            faCookie.Expires = DateTime.Now.AddMinutes(timeout);
            HttpContext.Current.Response.Cookies.Add(faCookie);
        }

        public static void CreateAuthCookie2()
        {
            var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);

            HttpCookie faCookiePath = new HttpCookie(FormsAuthentication.FormsCookiePath, "");
            faCookiePath.Expires = DateTime.Now.AddMinutes((timeout / 2));
            HttpContext.Current.Response.Cookies.Add(faCookiePath);
        }
    }
}