using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public static class UiHelper
    {
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
    }
}